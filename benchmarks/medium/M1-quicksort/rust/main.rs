use std::fmt::Write as FmtWrite;
use std::time::Instant;

// --- Inline SHA-256 ---

const SHA256_K: [u32; 64] = [
    0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4,
    0xab1c5ed5, 0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe,
    0x9bdc06a7, 0xc19bf174, 0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f,
    0x4a7484aa, 0x5cb0a9dc, 0x76f988da, 0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
    0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967, 0x27b70a85, 0x2e1b2138, 0x4d2c6dfc,
    0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85, 0xa2bfe8a1, 0xa81a664b,
    0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070, 0x19a4c116,
    0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
    0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7,
    0xc67178f2,
];

struct Sha256 {
    state: [u32; 8],
    buf: [u8; 64],
    buf_len: usize,
    total: u64,
}

impl Sha256 {
    fn new() -> Self {
        Self {
            state: [
                0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c,
                0x1f83d9ab, 0x5be0cd19,
            ],
            buf: [0; 64],
            buf_len: 0,
            total: 0,
        }
    }

    fn compress(&mut self, block: &[u8; 64]) {
        let mut w = [0u32; 64];
        for i in 0..16 {
            w[i] = u32::from_be_bytes([
                block[i * 4],
                block[i * 4 + 1],
                block[i * 4 + 2],
                block[i * 4 + 3],
            ]);
        }
        for i in 16..64 {
            let s0 = w[i - 15].rotate_right(7) ^ w[i - 15].rotate_right(18) ^ (w[i - 15] >> 3);
            let s1 = w[i - 2].rotate_right(17) ^ w[i - 2].rotate_right(19) ^ (w[i - 2] >> 10);
            w[i] = w[i - 16]
                .wrapping_add(s0)
                .wrapping_add(w[i - 7])
                .wrapping_add(s1);
        }
        let [mut a, mut b, mut c, mut d, mut e, mut f, mut g, mut h] = self.state;
        for i in 0..64 {
            let s1 = e.rotate_right(6) ^ e.rotate_right(11) ^ e.rotate_right(25);
            let ch = (e & f) ^ ((!e) & g);
            let t1 = h
                .wrapping_add(s1)
                .wrapping_add(ch)
                .wrapping_add(SHA256_K[i])
                .wrapping_add(w[i]);
            let s0 = a.rotate_right(2) ^ a.rotate_right(13) ^ a.rotate_right(22);
            let maj = (a & b) ^ (a & c) ^ (b & c);
            let t2 = s0.wrapping_add(maj);
            h = g;
            g = f;
            f = e;
            e = d.wrapping_add(t1);
            d = c;
            c = b;
            b = a;
            a = t1.wrapping_add(t2);
        }
        self.state[0] = self.state[0].wrapping_add(a);
        self.state[1] = self.state[1].wrapping_add(b);
        self.state[2] = self.state[2].wrapping_add(c);
        self.state[3] = self.state[3].wrapping_add(d);
        self.state[4] = self.state[4].wrapping_add(e);
        self.state[5] = self.state[5].wrapping_add(f);
        self.state[6] = self.state[6].wrapping_add(g);
        self.state[7] = self.state[7].wrapping_add(h);
    }

    fn update(&mut self, data: &[u8]) {
        self.total += data.len() as u64;
        let mut pos = 0;
        while pos < data.len() {
            let space = 64 - self.buf_len;
            let take = (data.len() - pos).min(space);
            self.buf[self.buf_len..self.buf_len + take].copy_from_slice(&data[pos..pos + take]);
            self.buf_len += take;
            pos += take;
            if self.buf_len == 64 {
                let block = self.buf;
                self.compress(&block);
                self.buf_len = 0;
            }
        }
    }

    fn finalize(mut self) -> [u8; 32] {
        let bit_len = self.total * 8;
        self.buf[self.buf_len] = 0x80;
        self.buf_len += 1;
        if self.buf_len > 56 {
            for b in self.buf[self.buf_len..].iter_mut() {
                *b = 0;
            }
            let block = self.buf;
            self.compress(&block);
            self.buf_len = 0;
        }
        for b in self.buf[self.buf_len..56].iter_mut() {
            *b = 0;
        }
        self.buf[56..64].copy_from_slice(&bit_len.to_be_bytes());
        let block = self.buf;
        self.compress(&block);
        let mut out = [0u8; 32];
        for (i, &s) in self.state.iter().enumerate() {
            out[i * 4..i * 4 + 4].copy_from_slice(&s.to_be_bytes());
        }
        out
    }
}

// --- QuickSort (Lomuto, iterative) ---

fn partition(arr: &mut [i32], lo: usize, hi: usize) -> usize {
    let pivot = arr[hi];
    let mut i = lo;
    for j in lo..hi {
        if arr[j] <= pivot {
            arr.swap(i, j);
            i += 1;
        }
    }
    arr.swap(i, hi);
    i
}

fn quicksort(arr: &mut [i32]) {
    if arr.len() <= 1 {
        return;
    }
    let n = arr.len();
    let mut stack: Vec<(usize, usize)> = Vec::with_capacity(64);
    stack.push((0, n - 1));
    while let Some((lo, hi)) = stack.pop() {
        if lo < hi {
            let p = partition(arr, lo, hi);
            if lo < p {
                stack.push((lo, p - 1));
            }
            stack.push((p + 1, hi));
        }
    }
}

// --- Main ---

fn main() {
    const N: usize = 5_000_000;
    const RUNS: usize = 10;
    let mut timings = [0.0f64; RUNS];

    println!("=== BENCHMARK: M1 - Custom QuickSort ===");
    println!("Language: Rust");
    println!("Runtime:  rustc (native)");
    println!();

    let mut arr = vec![0i32; N];
    let mut hash = String::new();

    for run in 0..RUNS {
        // Generate array with LCG (seed=42), interpreted as int32
        let mut state: u32 = 42;
        for v in arr.iter_mut() {
            state = state.wrapping_mul(1664525).wrapping_add(1013904223);
            *v = state as i32;
        }

        let start = Instant::now();
        quicksort(&mut arr);
        let elapsed = start.elapsed().as_secs_f64() * 1000.0;
        timings[run] = elapsed;
        println!("Run {:2}: {:.2} ms", run + 1, elapsed);

        if run == RUNS - 1 {
            let mut hasher = Sha256::new();
            let mut num_buf = String::with_capacity(12);
            for (i, &v) in arr.iter().enumerate() {
                if i > 0 {
                    hasher.update(b",");
                }
                num_buf.clear();
                write!(num_buf, "{}", v).unwrap();
                hasher.update(num_buf.as_bytes());
            }
            let digest = hasher.finalize();
            for b in &digest {
                write!(hash, "{:02x}", b).unwrap();
            }
        }
    }

    println!();
    let min = timings.iter().cloned().fold(f64::INFINITY, f64::min);
    let max = timings.iter().cloned().fold(f64::NEG_INFINITY, f64::max);
    let avg = timings.iter().sum::<f64>() / RUNS as f64;
    let variance = timings.iter().map(|t| (t - avg).powi(2)).sum::<f64>() / RUNS as f64;
    let stddev = variance.sqrt();

    println!("Min:    {:.2} ms", min);
    println!("Avg:    {:.2} ms", avg);
    println!("Max:    {:.2} ms", max);
    println!("StdDev: {:.2} ms", stddev);
    println!();
    println!("Verification: SHA-256 = {}", hash);
}

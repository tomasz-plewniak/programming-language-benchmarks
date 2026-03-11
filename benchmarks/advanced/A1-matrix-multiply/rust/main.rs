use std::time::Instant;

const N: usize = 1000;
const RUNS: usize = 10;

fn main() {
    println!("=== BENCHMARK: A1 - Matrix Multiplication ===");
    println!("Language: Rust");
    println!("Runtime:  rustc (native)");
    println!();

    let mut timings = [0.0f64; RUNS];

    // Initialize matrices
    let mut a = vec![0.0f64; N * N];
    let mut b = vec![0.0f64; N * N];
    for i in 0..N {
        for j in 0..N {
            a[i * N + j] = ((i * 1000 + j) % 97) as f64 * 0.01;
            b[i * N + j] = ((j * 1000 + i) % 89) as f64 * 0.01;
        }
    }

    let mut c = vec![0.0f64; N * N];

    for run in 0..RUNS {
        for v in c.iter_mut() {
            *v = 0.0;
        }

        let start = Instant::now();
        for i in 0..N {
            for k in 0..N {
                let a_ik = a[i * N + k];
                for j in 0..N {
                    c[i * N + j] += a_ik * b[k * N + j];
                }
            }
        }
        let elapsed = start.elapsed().as_secs_f64() * 1000.0;
        timings[run] = elapsed;
        println!("Run {:2}: {:.2} ms", run + 1, elapsed);
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
    println!("Verification:");
    println!("  C[0][0]     = {:.6}", c[0]);
    println!("  C[500][500] = {:.6}", c[500 * N + 500]);
    println!("  C[999][999] = {:.6}", c[999 * N + 999]);
}

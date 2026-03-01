use std::hint::black_box;
use std::time::Instant;

#[inline(never)]
fn fib(n: u32) -> u64 {
    if n <= 1 {
        return n as u64;
    }
    fib(n - 1) + fib(n - 2)
}

fn main() {
    const N: u32 = 40;
    const RUNS: usize = 10;
    let mut timings = [0.0f64; RUNS];

    println!("=== BENCHMARK: B1 - Fibonacci ===");
    println!("Language: Rust");
    println!("Runtime:  rustc (native)");
    println!();

    let mut result = 0u64;
    for i in 0..RUNS {
        let start = Instant::now();
        result = black_box(fib(black_box(N)));
        let elapsed = start.elapsed().as_secs_f64() * 1000.0; // ms
        timings[i] = elapsed;
        println!("Run {:2}: {:.2} ms", i + 1, elapsed);
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
    let status = if result == 102334155 { "✓" } else { "✗" };
    println!("Verification: {} {}", result, status);
}

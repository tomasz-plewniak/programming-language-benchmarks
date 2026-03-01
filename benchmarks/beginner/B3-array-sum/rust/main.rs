use std::hint::black_box;
use std::time::Instant;

fn main() {
    const N: usize = 10_000_000;
    const RUNS: usize = 10;
    const EXPECTED: i64 = 50000005000000;
    let mut timings = [0.0f64; RUNS];

    println!("=== BENCHMARK: B3 - Array Sum ===");
    println!("Language: Rust");
    println!("Runtime:  rustc (native)");
    println!();

    let mut result: i64 = 0;
    for i in 0..RUNS {
        let start = Instant::now();
        let arr: Vec<i64> = (1..=N as i64).collect();
        let s: i64 = black_box(&arr).iter().sum();
        result = black_box(s);
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
    let status = if result == EXPECTED { "✓" } else { "✗" };
    println!("Verification: {} {}", result, status);
}

use std::collections::HashMap;
use std::time::Instant;

const N: usize = 5_000_000;
const LOOKUPS: usize = 1_000_000;
const RUNS: usize = 10;

fn main() {
    let mut insert_timings = [0.0f64; RUNS];
    let mut lookup_timings = [0.0f64; RUNS];

    println!("=== BENCHMARK: M3 - Hash Map Stress Test ===");
    println!("Language: Rust");
    println!("Runtime:  rustc (native)");
    println!();

    let mut verify_sum: i64 = 0;

    for run in 0..RUNS {
        // Insert phase
        let start = Instant::now();
        let mut map: HashMap<String, i64> = HashMap::with_capacity(N);
        for i in 0..N {
            let key = format!("user_{}", i);
            let value = (i as i64) * 31 + 7;
            map.insert(key, value);
        }
        let insert_elapsed = start.elapsed().as_secs_f64() * 1000.0;
        insert_timings[run] = insert_elapsed;

        // Lookup phase
        let start = Instant::now();
        let mut state: u32 = 42;
        let mut sum: i64 = 0;
        for _ in 0..LOOKUPS {
            state = state.wrapping_mul(1664525).wrapping_add(1013904223);
            let idx = (state as u64 % N as u64) as usize;
            let key = format!("user_{}", idx);
            sum += map[&key];
        }
        let lookup_elapsed = start.elapsed().as_secs_f64() * 1000.0;
        lookup_timings[run] = lookup_elapsed;

        verify_sum = sum;
        println!(
            "Run {:2}: Insert: {:>8.2} ms | Lookup: {:>8.2} ms | Total: {:>8.2} ms",
            run + 1,
            insert_elapsed,
            lookup_elapsed,
            insert_elapsed + lookup_elapsed
        );
    }

    println!();

    // Insert stats
    let min = insert_timings.iter().cloned().fold(f64::INFINITY, f64::min);
    let max = insert_timings.iter().cloned().fold(f64::NEG_INFINITY, f64::max);
    let avg = insert_timings.iter().sum::<f64>() / RUNS as f64;
    let variance = insert_timings.iter().map(|t| (t - avg).powi(2)).sum::<f64>() / RUNS as f64;
    let stddev = variance.sqrt();
    println!("Insert:");
    println!("  Min:    {:.2} ms", min);
    println!("  Avg:    {:.2} ms", avg);
    println!("  Max:    {:.2} ms", max);
    println!("  StdDev: {:.2} ms", stddev);
    println!();

    // Lookup stats
    let min = lookup_timings.iter().cloned().fold(f64::INFINITY, f64::min);
    let max = lookup_timings.iter().cloned().fold(f64::NEG_INFINITY, f64::max);
    let avg = lookup_timings.iter().sum::<f64>() / RUNS as f64;
    let variance = lookup_timings.iter().map(|t| (t - avg).powi(2)).sum::<f64>() / RUNS as f64;
    let stddev = variance.sqrt();
    println!("Lookup:");
    println!("  Min:    {:.2} ms", min);
    println!("  Avg:    {:.2} ms", avg);
    println!("  Max:    {:.2} ms", max);
    println!("  StdDev: {:.2} ms", stddev);
    println!();

    // Total stats
    let total_timings: Vec<f64> = insert_timings
        .iter()
        .zip(lookup_timings.iter())
        .map(|(i, l)| i + l)
        .collect();
    let min = total_timings.iter().cloned().fold(f64::INFINITY, f64::min);
    let max = total_timings.iter().cloned().fold(f64::NEG_INFINITY, f64::max);
    let avg = total_timings.iter().sum::<f64>() / RUNS as f64;
    let variance = total_timings.iter().map(|t| (t - avg).powi(2)).sum::<f64>() / RUNS as f64;
    let stddev = variance.sqrt();
    println!("Total:");
    println!("  Min:    {:.2} ms", min);
    println!("  Avg:    {:.2} ms", avg);
    println!("  Max:    {:.2} ms", max);
    println!("  StdDev: {:.2} ms", stddev);
    println!();

    println!("Verification: sum = {}", verify_sum);
}

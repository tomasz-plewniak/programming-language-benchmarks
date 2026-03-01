use std::fs;
use std::io::{BufRead, BufReader, BufWriter, Write};
use std::path::PathBuf;
use std::time::Instant;

const LINES: usize = 1_000_000;
const RUNS: usize = 10;

fn data_path() -> PathBuf {
    let mut p = std::env::current_dir().unwrap();
    for _ in 0..4 {
        p.pop();
    }
    p.push("data");
    p.push("b2_lines.txt");
    p
}

fn write_file(path: &PathBuf) {
    fs::create_dir_all(path.parent().unwrap()).unwrap();
    let f = fs::File::create(path).unwrap();
    let mut w = BufWriter::new(f);
    for i in 1..=LINES {
        writeln!(w, "Line {}: The quick brown fox jumps over the lazy dog", i).unwrap();
    }
    w.flush().unwrap();
}

fn read_and_count(path: &PathBuf) -> usize {
    let f = fs::File::open(path).unwrap();
    let reader = BufReader::new(f);
    reader.lines().count()
}

fn main() {
    let path = data_path();
    let mut write_timings = [0.0f64; RUNS];
    let mut read_timings = [0.0f64; RUNS];

    println!("=== BENCHMARK: B2 - File Line Counter ===");
    println!("Language: Rust");
    println!("Runtime:  rustc (native)");
    println!();

    let mut count = 0usize;
    for i in 0..RUNS {
        let start = Instant::now();
        write_file(&path);
        let write_elapsed = start.elapsed().as_secs_f64() * 1000.0;

        let start = Instant::now();
        count = read_and_count(&path);
        let read_elapsed = start.elapsed().as_secs_f64() * 1000.0;

        write_timings[i] = write_elapsed;
        read_timings[i] = read_elapsed;
        let total = write_elapsed + read_elapsed;
        println!("Run {:2}: Write {:.2} ms | Read {:.2} ms | Total {:.2} ms", i + 1, write_elapsed, read_elapsed, total);
    }

    println!();
    for (label, timings) in [("Write", &write_timings), ("Read", &read_timings)] {
        let min = timings.iter().cloned().fold(f64::INFINITY, f64::min);
        let max = timings.iter().cloned().fold(f64::NEG_INFINITY, f64::max);
        let avg = timings.iter().sum::<f64>() / RUNS as f64;
        let variance = timings.iter().map(|t| (t - avg).powi(2)).sum::<f64>() / RUNS as f64;
        let stddev = variance.sqrt();
        println!("{} — Min: {:.2} ms | Avg: {:.2} ms | Max: {:.2} ms | StdDev: {:.2} ms", label, min, avg, max, stddev);
    }

    let totals: Vec<f64> = (0..RUNS).map(|i| write_timings[i] + read_timings[i]).collect();
    let min = totals.iter().cloned().fold(f64::INFINITY, f64::min);
    let max = totals.iter().cloned().fold(f64::NEG_INFINITY, f64::max);
    let avg = totals.iter().sum::<f64>() / RUNS as f64;
    let variance = totals.iter().map(|t| (t - avg).powi(2)).sum::<f64>() / RUNS as f64;
    let stddev = variance.sqrt();
    println!("Total — Min: {:.2} ms | Avg: {:.2} ms | Max: {:.2} ms | StdDev: {:.2} ms", min, avg, max, stddev);

    println!();
    let status = if count == LINES { "✓" } else { "✗" };
    println!("Verification: {} {}", count, status);

    fs::remove_file(&path).ok();
}

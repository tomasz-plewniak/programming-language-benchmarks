use std::collections::HashMap;
use std::fs;
use std::io::{BufRead, BufReader, Write, BufWriter};
use std::path::Path;
use std::sync::{Arc, Mutex};
use std::thread;
use std::time::Instant;

const NUM_FILES: usize = 100;
const LINES_PER_FILE: usize = 100_000;
const RUNS: usize = 10;
const SEED: i64 = 42;
const WORDS: &[&str] = &[
    "the", "be", "to", "of", "and", "a", "in", "that", "have", "I",
    "it", "for", "not", "on", "with", "he", "as", "you", "do", "at",
    "this", "but", "his", "by", "from", "they", "we", "her", "she", "or",
    "an", "will", "my", "one", "all", "would", "there", "their", "what", "so",
    "up", "out", "if", "about", "who", "get", "which", "go", "me", "when",
    "make", "can", "like", "time", "no", "just", "him", "know", "take", "people",
    "into", "year", "your", "good", "some", "could", "them", "see", "other", "than",
    "then", "now", "look", "only", "come", "its", "over", "think", "also", "back",
    "after", "use", "two", "how", "our", "work", "first", "well", "way", "even",
    "new", "want", "because", "any", "these", "give", "day", "most", "us", "great",
    "between", "need", "large", "often", "hand", "high", "place", "old", "while", "mean",
    "keep", "let", "begin", "seem", "help", "show", "hear", "play", "run", "move",
    "live", "believe", "bring", "happen", "write", "provide", "sit", "stand", "lose", "pay",
    "meet", "include", "continue", "set", "learn", "change", "lead", "understand", "watch", "follow",
    "stop", "create", "speak", "read", "allow", "add", "spend", "grow", "open", "walk",
    "win", "teach", "offer", "remember", "love", "consider", "appear", "buy", "wait", "serve",
    "die", "send", "expect", "build", "stay", "fall", "cut", "reach", "kill", "remain",
    "suggest", "raise", "pass", "sell", "require", "report", "decide", "pull", "develop", "eat",
    "return", "hold", "cover", "point", "turn", "start", "close", "small", "number", "group",
    "always", "music", "those", "both", "mark", "call", "ask", "late", "home", "last",
    "long", "best", "still", "find", "head", "body", "water", "word", "money", "story",
    "fact", "month", "lot", "right", "study", "book", "eye", "job", "business", "issue",
    "side", "kind", "four", "room", "heart", "friend", "power", "city", "house", "party",
    "world", "area", "company", "problem", "during", "family", "government", "country", "question", "school",
    "state", "program", "information", "system", "service", "part", "idea", "table", "game", "child",
    "process", "since", "line", "result", "team", "model", "product", "market", "level", "local",
    "computer", "field", "car", "force", "food", "community", "end", "light", "real", "history",
    "political", "social", "general", "personal", "public", "national", "court", "young", "council", "war",
    "health", "age", "face", "policy", "research", "street", "law", "door", "office", "trade",
    "report", "student", "human", "data", "form", "value", "rate", "land", "project", "control",
    "action", "support", "order", "today", "figure", "class", "mother", "special", "case", "reason",
    "morning", "record", "air", "nature", "north", "sound", "effort", "fish", "plant", "true",
    "paper", "space", "event", "range", "plan", "type", "police", "road", "view", "south",
    "board", "cover", "price", "letter", "current", "future", "present", "past", "foreign", "central",
    "digital", "global", "final", "major", "natural", "popular", "cultural", "serious", "recent", "common",
    "left", "simple", "entire", "clear", "certain", "single", "source", "detail", "standard", "share",
    "modern", "potential", "key", "strong",
];

fn generate_files(data_dir: &Path) {
    let mut state: i64 = SEED;
    let word_count = WORDS.len() as i64;
    for f in 0..NUM_FILES {
        let file_path = data_dir.join(format!("file_{:03}.txt", f));
        let file = fs::File::create(&file_path).unwrap();
        let mut writer = BufWriter::new(file);
        for _ in 0..LINES_PER_FILE {
            state = (state.wrapping_mul(1103515245) + 12345) & 0x7FFFFFFF;
            let wc = (state % 11) + 5;
            for w in 0..wc {
                if w > 0 {
                    writer.write_all(b" ").unwrap();
                }
                state = (state.wrapping_mul(1103515245) + 12345) & 0x7FFFFFFF;
                let idx = (state % word_count) as usize;
                writer.write_all(WORDS[idx].as_bytes()).unwrap();
            }
            writer.write_all(b"\n").unwrap();
        }
    }
}

fn main() {
    println!("=== BENCHMARK: A2 - Concurrent File Processing ===");
    println!("Language: Rust");
    println!("Runtime:  rustc (native)");
    println!();

    let data_dir = Path::new("data");
    fs::create_dir_all(data_dir).unwrap();
    generate_files(data_dir);

    let mut timings = [0.0f64; RUNS];

    for run in 0..RUNS {
        let start = Instant::now();

        let global_counts: Arc<Mutex<HashMap<String, i64>>> =
            Arc::new(Mutex::new(HashMap::new()));

        let mut handles = Vec::with_capacity(NUM_FILES);
        for f in 0..NUM_FILES {
            let file_path = data_dir.join(format!("file_{:03}.txt", f));
            let global = Arc::clone(&global_counts);
            handles.push(thread::spawn(move || {
                let file = fs::File::open(&file_path).unwrap();
                let reader = BufReader::new(file);
                let mut local_counts: HashMap<String, i64> = HashMap::new();
                for line in reader.lines() {
                    let line = line.unwrap();
                    for word in line.split_whitespace() {
                        *local_counts.entry(word.to_string()).or_insert(0) += 1;
                    }
                }
                let mut global = global.lock().unwrap();
                for (word, count) in local_counts {
                    *global.entry(word).or_insert(0) += count;
                }
            }));
        }
        for h in handles {
            h.join().unwrap();
        }

        let elapsed = start.elapsed().as_secs_f64() * 1000.0;
        timings[run] = elapsed;
        println!("Run {:2}: {:.2} ms", run + 1, elapsed);

        if run == RUNS - 1 {
            let counts = global_counts.lock().unwrap();
            let mut sorted: Vec<_> = counts.iter().collect();
            sorted.sort_by(|a, b| b.1.cmp(a.1).then_with(|| a.0.cmp(b.0)));

            println!();
            println!("Verification:");
            println!("  Total unique words: {}", counts.len());
            println!("  Top 5 words:");
            for (i, (word, count)) in sorted.iter().take(5).enumerate() {
                println!("    {}. {} = {}", i + 1, word, count);
            }
            let idx = std::cmp::min(99, sorted.len() - 1);
            println!("  Word #100: {} = {}", sorted[idx].0, sorted[idx].1);
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

    fs::remove_dir_all(data_dir).unwrap();
}

# Cross-Language Performance Benchmark Suite

**Languages:** C# · Python · JavaScript (Node.js) · Go
**Goal:** Identical tasks, identical inputs/outputs — measure raw execution time and resource usage.

---

## Benchmark Rules

1. **Same algorithm** — every language must implement the same logic
2. **Same input data** — use shared generated data files or identical seed-based generation
3. **Timing** — measure only the task execution, not startup or file loading (except where I/O *is* the benchmark)
4. **Iterations** — run each task **10 times**, report **min / avg / max** in milliseconds
5. **Environment** — same machine, same OS, no other load; document CPU, RAM, OS version
6. **Output verification** — each task produces a checksum or hash of its output to confirm correctness

---

## Timing Harness (pseudo-code)

```
results = []
for i in 1..10:
    start = high_resolution_timer()
    run_task()
    elapsed = high_resolution_timer() - start
    results.append(elapsed)

print("Min: {min(results)} ms")
print("Avg: {avg(results)} ms")
print("Max: {max(results)} ms")
```

**Per-language timer:**
| Language   | Timer                                      |
|------------|--------------------------------------------|
| C#         | `Stopwatch.StartNew()` / `.ElapsedMilliseconds` |
| Python     | `time.perf_counter()`                      |
| JavaScript | `process.hrtime.bigint()`                  |
| Go         | `time.Now()` / `time.Since()`              |

---

## Repository Structure

```
programming-language-benchmarks/
├── benchmarks/
│   ├── beginner/
│   │   ├── B1-fibonacci/
│   │   │   ├── csharp/
│   │   │   ├── python/
│   │   │   ├── javascript/
│   │   │   └── go/
│   │   ├── B2-file-line-counter/
│   │   └── B3-array-sum/
│   ├── medium/
│   │   ├── M1-quicksort/
│   │   ├── M2-csv-processing/
│   │   └── M3-hashmap-stress/
│   ├── advanced/
│   │   ├── A1-matrix-multiplication/
│   │   ├── A2-concurrent-file-processing/
│   │   └── A3-binary-tree/
│   └── pro/
│       ├── P1-parallel-merge-sort/
│       ├── P2-http-server/
│       └── P3-memory-pool/
├── data/        ← shared generated input files (gitignored when large)
└── results/     ← timing CSVs and summary dashboard
```

Each task folder contains one subfolder per language. Language-specific run commands:

| Language   | Project file              | Run command             |
|------------|---------------------------|-------------------------|
| C#         | `csharp/Benchmark.csproj` | `dotnet run -c Release` |
| Python     | `python/main.py`          | `python main.py`        |
| JavaScript | `javascript/main.js`      | `node main.js`          |
| Go         | `go/main.go` + `go.mod`   | `go run main.go`        |

---

## BEGINNER LEVEL

### B1 — Fibonacci (CPU-bound)

**Task:** Compute the 40th Fibonacci number using **naive recursion** (no memoization).

| Detail        | Value              |
|---------------|--------------------|
| Input         | `n = 40`           |
| Expected output | `102334155`      |
| What it tests | Function call overhead, stack performance |
| Category      | CPU-bound          |

**Rules:** Must use recursive `fib(n) = fib(n-1) + fib(n-2)` with base cases `fib(0)=0`, `fib(1)=1`. No caching.

---

### B2 — File Line Counter (I/O-bound)

**Task:** Generate a text file with **1,000,000 lines** (each line: `"Line {i}: The quick brown fox jumps over the lazy dog"`), then read it back and count lines.

| Detail        | Value                         |
|---------------|-------------------------------|
| Input         | Generate → read               |
| Expected output | `1000000`                   |
| What it tests | File write speed, file read speed, line parsing |
| Category      | I/O-bound                     |

**Rules:** Measure write and read separately. Report both times.

---

### B3 — Array Sum (Memory / CPU)

**Task:** Create an array of **10,000,000** integers (values 1 to 10M), then compute the sum.

| Detail        | Value                            |
|---------------|----------------------------------|
| Input         | Array `[1, 2, 3, ..., 10_000_000]` |
| Expected output | `50000005000000`               |
| What it tests | Array allocation speed, iteration speed |
| Category      | Memory + CPU                     |

**Rules:** Allocate array first (include in timing), then sum. No parallel or SIMD.

---

## MEDIUM LEVEL

### M1 — Custom QuickSort (CPU-bound)

**Task:** Implement QuickSort from scratch and sort an array of **5,000,000** random integers.

| Detail        | Value                                        |
|---------------|----------------------------------------------|
| Input         | 5M random `int32` values, seed = `42`        |
| Expected output | Sorted array; report SHA-256 of joined string |
| What it tests | Recursion, partitioning, memory allocation   |
| Category      | CPU-bound                                    |

**Rules:**
- Use Lomuto or Hoare partition scheme (state which)
- Generate random numbers with a seeded PRNG for reproducibility
- **Do NOT use built-in sort** — implement the full algorithm
- Verify: SHA-256 hash of output array joined by commas must match across all languages

---

### M2 — CSV Processing (I/O + CPU)

**Task:** Generate a CSV file with **1,000,000 rows** and 6 columns, then read it, filter, aggregate, and write results.

**Generated CSV schema:**
```
id,first_name,last_name,email,department,salary
1,John,Smith,john.smith@company.com,Engineering,85000.50
...
```

**Departments** (cycle through): Engineering, Marketing, Sales, HR, Finance, Operations, Support, Legal
**Salaries:** Random between 30000.00 and 150000.00 (seed = 42)

**Processing steps:**
1. Read the CSV
2. Filter rows where `salary > 75000`
3. Group by `department`
4. Calculate per department: `count`, `avg_salary`, `max_salary`, `min_salary`
5. Sort results by `avg_salary` descending
6. Write results to `output.csv`

| Detail        | Value                                |
|---------------|--------------------------------------|
| What it tests | File I/O, string parsing, hash maps, sorting |
| Category      | I/O + CPU                            |

**Rules:** No CSV libraries — parse manually (split by comma is sufficient; no quoted fields in this dataset).

---

### M3 — Hash Map Stress Test (Memory-intensive)

**Task:** Insert **5,000,000** key-value pairs into a hash map, then look up **1,000,000** random keys.

| Detail        | Value                                          |
|---------------|-------------------------------------------------|
| Key format    | `"user_{i}"` where i = 0..4,999,999            |
| Value         | `i * 31 + 7` (deterministic)                   |
| Lookup keys   | Random 1M keys from the set (seed = 42)         |
| Expected      | Sum of all looked-up values                      |
| What it tests | Hash map allocation, hashing, memory layout      |
| Category      | Memory-intensive                                 |

**Rules:** Measure insert time and lookup time separately.

---

## ADVANCED LEVEL

### A1 — Matrix Multiplication (CPU-bound)

**Task:** Multiply two **1000×1000** matrices of `double`/`float64` values.

| Detail        | Value                                         |
|---------------|-----------------------------------------------|
| Matrix A      | `A[i][j] = (i * 1000 + j) % 97 * 0.01`      |
| Matrix B      | `B[i][j] = (j * 1000 + i) % 89 * 0.01`      |
| Expected      | Report `C[0][0]`, `C[500][500]`, `C[999][999]` (6 decimal places) |
| What it tests | Nested loops, cache performance, floating point |
| Category      | CPU-bound                                      |

**Rules:**
- Use naive triple-loop `O(n³)` multiplication — no Strassen, no BLAS, no SIMD
- Row-major storage in all languages
- Values must match across languages (within floating-point tolerance ±0.0001)

---

### A2 — Concurrent File Processing (I/O + Concurrency)

**Task:** Generate **100 text files** (each ~100,000 lines of random words), then concurrently read all files, count word frequencies across ALL files, and write top 100 words to output.

**File generation (seed = 42):**
- Word pool: 500 common English words (provided list below)
- Each line: 5-15 random words separated by spaces
- Files named `file_000.txt` through `file_099.txt`

| Detail        | Value                                       |
|---------------|---------------------------------------------|
| What it tests | Concurrent I/O, thread-safe data structures, merging |
| Category      | I/O + Concurrency                            |

**Rules:**
- Files must be read concurrently (not sequentially)
- Word counting must be thread-safe
- C#: `Task.WhenAll` + `ConcurrentDictionary`
- Python: `concurrent.futures.ThreadPoolExecutor`
- JavaScript: `Promise.all` + worker approach or async file reads
- Go: goroutines + `sync.Map` or mutex-protected map
- Report total unique words and top 100 by frequency

---

### A3 — Binary Tree Operations (Memory + CPU)

**Task:** Build a binary search tree with **2,000,000** nodes (random int64 values, seed = 42), then perform:

1. **In-order traversal** — collect all values into array
2. **Search** — look up 100,000 random values, count how many were found
3. **Calculate tree height**

| Detail        | Value                                         |
|---------------|-----------------------------------------------|
| What it tests | Pointer/reference overhead, recursion depth, memory allocation |
| Category      | Memory + CPU                                   |

**Rules:** Implement BST from scratch — no built-in tree structures. Use iterative approaches where stack overflow is a risk.

---

## PRO LEVEL

### P1 — Parallel Merge Sort with Thread Pool (CPU + Concurrency)

**Task:** Sort **50,000,000** random integers using parallel merge sort.

| Detail        | Value                                          |
|---------------|-------------------------------------------------|
| Input         | 50M random `int32`, seed = 42                   |
| Expected      | SHA-256 hash of sorted output matches across all |
| What it tests | Thread management, work stealing, merge overhead |
| Category      | CPU + Concurrency                                |

**Rules:**
- Split work across available CPU cores
- Below threshold (e.g., 10,000 elements), fall back to sequential sort
- Measure: total time, speedup vs single-threaded version
- C#: `Parallel.Invoke` / `Task`
- Python: `multiprocessing.Pool` (to escape GIL)
- JavaScript: `Worker threads`
- Go: goroutines + channels

---

### P2 — HTTP Server Benchmark (I/O + Concurrency)

**Task:** Implement a minimal HTTP server that handles a JSON endpoint, then benchmark it with a client.

**Server endpoint:** `GET /data?n={number}`
- Returns JSON: `{ "numbers": [list of first n prime numbers], "count": n, "sum": sum_of_primes }`
- Must handle concurrent connections

**Client:**
- Send **10,000 requests** with `n` values between 10 and 1000 (random, seed = 42)
- Use **50 concurrent connections**
- Measure: total time, requests/second, avg latency, p99 latency

| Detail        | Value                                        |
|---------------|----------------------------------------------|
| What it tests | HTTP handling, JSON serialization, concurrency, prime computation |
| Category      | I/O + CPU + Concurrency                      |

**Rules:**
- Use standard library HTTP servers (no frameworks):
  - C#: `Kestrel` minimal API or `HttpListener`
  - Python: `asyncio` + `aiohttp` or raw sockets
  - JavaScript: `http` module
  - Go: `net/http`
- Prime calculation must be done per-request (no caching)

---

### P3 — Memory Pool / Object Reuse (Memory-intensive)

**Task:** Simulate a high-throughput message processing system.

**Specification:**
1. Create a pool of **1,000 pre-allocated message objects** (each ~1KB: id, timestamp, payload byte array, headers map)
2. Process **10,000,000 messages** total:
   - Acquire message from pool (or allocate if pool empty)
   - Fill with data (simulate: write random bytes to payload)
   - "Process" (compute CRC32 of payload)
   - Return to pool
3. Track: total allocations, pool hits, pool misses, GC pauses (where measurable)

| Detail        | Value                                         |
|---------------|-----------------------------------------------|
| What it tests | GC pressure, object pooling efficiency, allocation patterns |
| Category      | Memory-intensive                               |

**Rules:**
- C#: `ObjectPool<T>` or custom `ConcurrentBag`-based pool
- Python: Custom pool with `deque`
- JavaScript: Custom pool with array
- Go: `sync.Pool`
- Report: total time, allocations count, GC pause time (if measurable)

---

## Word Pool for Task A2 (500 words)

```
the, be, to, of, and, a, in, that, have, I, it, for, not, on, with, he, as, you, do, at,
this, but, his, by, from, they, we, her, she, or, an, will, my, one, all, would, there,
their, what, so, up, out, if, about, who, get, which, go, me, when, make, can, like, time,
no, just, him, know, take, people, into, year, your, good, some, could, them, see, other,
than, then, now, look, only, come, its, over, think, also, back, after, use, two, how, our,
work, first, well, way, even, new, want, because, any, these, give, day, most, us, great,
between, need, large, often, hand, high, place, old, while, mean, keep, let, begin, seem,
help, show, hear, play, run, move, live, believe, bring, happen, write, provide, sit, stand,
lose, pay, meet, include, continue, set, learn, change, lead, understand, watch, follow, stop,
create, speak, read, allow, add, spend, grow, open, walk, win, teach, offer, remember, love,
consider, appear, buy, wait, serve, die, send, expect, build, stay, fall, cut, reach, kill,
remain, suggest, raise, pass, sell, require, report, decide, pull, develop, eat, return, hold,
cover, point, turn, start, close, small, number, group, always, music, those, both, mark, call,
ask, late, home, last, long, best, still, find, head, body, water, word, money, story, fact,
month, lot, right, study, book, eye, job, business, issue, side, kind, four, room, heart, friend,
power, city, house, party, world, area, company, problem, during, family, government, country,
question, school, state, program, information, system, service, part, idea, table, game, child,
process, since, line, result, team, model, product, market, level, local, computer, field, car,
force, food, community, end, light, real, history, political, social, general, personal, public,
national, court, young, council, war, health, age, face, policy, research, street, law, door,
office, trade, report, student, human, data, form, value, rate, land, project, control, action,
support, order, today, figure, class, mother, special, case, reason, morning, record, air, nature,
north, sound, effort, fish, plant, true, paper, space, event, range, plan, type, police, road,
view, south, board, cover, price, letter, current, future, present, past, foreign, central,
digital, global, final, major, natural, popular, cultural, serious, recent, common, left, simple,
entire, clear, certain, single, source, detail, standard, share, modern, potential, key, strong
```

---

## Output Template

Each benchmark should output results in this format:

```
=== BENCHMARK: [Task ID] - [Task Name] ===
Language: [C# / Python / JavaScript / Go]
Runtime:  [.NET 9 / Python 3.12 / Node 22 / Go 1.22]

Run  1: 1234.56 ms
Run  2: 1230.12 ms
...
Run 10: 1228.91 ms

Min:  1228.91 ms
Avg:  1232.45 ms
Max:  1240.33 ms
StdDev: 3.12 ms

Verification: [checksum/hash/expected value] ✓
Memory Peak:  [if measurable] MB
```

---

## Summary Dashboard Template

After running all benchmarks, populate this comparison table:

```
| Task | Category    | C# (avg ms) | Python (avg ms) | JS (avg ms) | Go (avg ms) | Fastest |
|------|-------------|-------------|-----------------|-------------|-------------|---------|
| B1   | CPU         |             |                 |             |             |         |
| B2   | I/O         |             |                 |             |             |         |
| B3   | Memory+CPU  |             |                 |             |             |         |
| M1   | CPU         |             |                 |             |             |         |
| M2   | I/O+CPU     |             |                 |             |             |         |
| M3   | Memory      |             |                 |             |             |         |
| A1   | CPU         |             |                 |             |             |         |
| A2   | I/O+Conc    |             |                 |             |             |         |
| A3   | Memory+CPU  |             |                 |             |             |         |
| P1   | CPU+Conc    |             |                 |             |             |         |
| P2   | I/O+CPU+C   |             |                 |             |             |         |
| P3   | Memory      |             |                 |             |             |         |
```
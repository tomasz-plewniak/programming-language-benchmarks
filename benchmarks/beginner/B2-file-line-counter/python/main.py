import os
import time
import math

DATA_DIR = os.path.join(os.path.dirname(__file__), "..", "..", "..", "..", "data")
FILE_PATH = os.path.join(DATA_DIR, "b2_lines.txt")
LINES = 1_000_000
RUNS = 10


def write_file():
    os.makedirs(DATA_DIR, exist_ok=True)
    with open(FILE_PATH, "w") as f:
        for i in range(1, LINES + 1):
            f.write(f"Line {i}: The quick brown fox jumps over the lazy dog\n")


def read_and_count():
    count = 0
    with open(FILE_PATH, "r") as f:
        for _ in f:
            count += 1
    return count


def main():
    write_timings = []
    read_timings = []

    print("=== BENCHMARK: B2 - File Line Counter ===")
    print("Language: Python")
    print(f"Runtime:  Python {__import__('sys').version.split()[0]}")
    print()

    for i in range(1, RUNS + 1):
        start = time.perf_counter()
        write_file()
        write_elapsed = (time.perf_counter() - start) * 1000

        start = time.perf_counter()
        count = read_and_count()
        read_elapsed = (time.perf_counter() - start) * 1000

        write_timings.append(write_elapsed)
        read_timings.append(read_elapsed)
        total = write_elapsed + read_elapsed
        print(f"Run {i:2d}: Write {write_elapsed:.2f} ms | Read {read_elapsed:.2f} ms | Total {total:.2f} ms")

    print()
    for label, timings in [("Write", write_timings), ("Read", read_timings)]:
        min_t = min(timings)
        avg_t = sum(timings) / len(timings)
        max_t = max(timings)
        variance = sum((t - avg_t) ** 2 for t in timings) / len(timings)
        stddev = math.sqrt(variance)
        print(f"{label} — Min: {min_t:.2f} ms | Avg: {avg_t:.2f} ms | Max: {max_t:.2f} ms | StdDev: {stddev:.2f} ms")

    totals = [w + r for w, r in zip(write_timings, read_timings)]
    min_t = min(totals)
    avg_t = sum(totals) / len(totals)
    max_t = max(totals)
    variance = sum((t - avg_t) ** 2 for t in totals) / len(totals)
    stddev = math.sqrt(variance)
    print(f"Total — Min: {min_t:.2f} ms | Avg: {avg_t:.2f} ms | Max: {max_t:.2f} ms | StdDev: {stddev:.2f} ms")

    print()
    status = "✓" if count == LINES else "✗"
    print(f"Verification: {count} {status}")

    os.remove(FILE_PATH)


if __name__ == "__main__":
    main()

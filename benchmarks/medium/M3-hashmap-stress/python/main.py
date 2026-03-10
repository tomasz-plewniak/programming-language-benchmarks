import math
import sys
import time


def main():
    N = 5_000_000
    LOOKUPS = 1_000_000
    RUNS = 10

    insert_timings = []
    lookup_timings = []

    print("=== BENCHMARK: M3 - Hash Map Stress Test ===")
    print("Language: Python")
    print(f"Runtime:  Python {sys.version.split()[0]}")
    print()

    verify_sum = 0

    for run in range(1, RUNS + 1):
        # Insert phase
        start = time.perf_counter()
        m = {}
        for i in range(N):
            m[f"user_{i}"] = i * 31 + 7
        insert_elapsed = (time.perf_counter() - start) * 1000
        insert_timings.append(insert_elapsed)

        # Lookup phase
        start = time.perf_counter()
        state = 42
        total = 0
        for _ in range(LOOKUPS):
            state = (state * 1664525 + 1013904223) & 0xFFFFFFFF
            idx = state % N
            total += m[f"user_{idx}"]
        lookup_elapsed = (time.perf_counter() - start) * 1000
        lookup_timings.append(lookup_elapsed)

        verify_sum = total
        print(f"Run {run:2d}: Insert: {insert_elapsed:8.2f} ms | Lookup: {lookup_elapsed:8.2f} ms | Total: {insert_elapsed + lookup_elapsed:8.2f} ms")

    print()
    print_stats("Insert", insert_timings)
    print_stats("Lookup", lookup_timings)

    total_timings = [i + l for i, l in zip(insert_timings, lookup_timings)]
    print_stats("Total", total_timings)

    print(f"Verification: sum = {verify_sum}")


def print_stats(label, timings):
    min_t = min(timings)
    avg_t = sum(timings) / len(timings)
    max_t = max(timings)
    variance = sum((t - avg_t) ** 2 for t in timings) / len(timings)
    stddev = math.sqrt(variance)

    print(f"{label}:")
    print(f"  Min:    {min_t:.2f} ms")
    print(f"  Avg:    {avg_t:.2f} ms")
    print(f"  Max:    {max_t:.2f} ms")
    print(f"  StdDev: {stddev:.2f} ms")
    print()


if __name__ == "__main__":
    main()

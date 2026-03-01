import time
import math


def main():
    N = 10_000_000
    RUNS = 10
    EXPECTED = 50000005000000
    timings = []

    print("=== BENCHMARK: B3 - Array Sum ===")
    print("Language: Python")
    print(f"Runtime:  Python {__import__('sys').version.split()[0]}")
    print()

    for i in range(1, RUNS + 1):
        start = time.perf_counter()
        arr = list(range(1, N + 1))
        result = sum(arr)
        elapsed = (time.perf_counter() - start) * 1000  # ms
        timings.append(elapsed)
        print(f"Run {i:2d}: {elapsed:.2f} ms")

    print()
    min_t = min(timings)
    avg_t = sum(timings) / len(timings)
    max_t = max(timings)
    variance = sum((t - avg_t) ** 2 for t in timings) / len(timings)
    stddev = math.sqrt(variance)

    print(f"Min:    {min_t:.2f} ms")
    print(f"Avg:    {avg_t:.2f} ms")
    print(f"Max:    {max_t:.2f} ms")
    print(f"StdDev: {stddev:.2f} ms")
    print()
    status = "✓" if result == EXPECTED else "✗"
    print(f"Verification: {result} {status}")


if __name__ == "__main__":
    main()

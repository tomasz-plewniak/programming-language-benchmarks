import time
import math


def fib(n):
    if n <= 1:
        return n
    return fib(n - 1) + fib(n - 2)


def main():
    N = 40
    RUNS = 10
    timings = []

    print("=== BENCHMARK: B1 - Fibonacci ===")
    print("Language: Python")
    print(f"Runtime:  Python {__import__('sys').version.split()[0]}")
    print()

    for i in range(1, RUNS + 1):
        start = time.perf_counter()
        result = fib(N)
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
    status = "✓" if result == 102334155 else "✗"
    print(f"Verification: {result} {status}")


if __name__ == "__main__":
    main()

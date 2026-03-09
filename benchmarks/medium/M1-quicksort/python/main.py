import hashlib
import math
import sys
import time


def partition(arr, lo, hi):
    pivot = arr[hi]
    i = lo - 1
    for j in range(lo, hi):
        if arr[j] <= pivot:
            i += 1
            arr[i], arr[j] = arr[j], arr[i]
    arr[i + 1], arr[hi] = arr[hi], arr[i + 1]
    return i + 1


def quicksort(arr, lo, hi):
    stack = [(lo, hi)]
    while stack:
        l, h = stack.pop()
        if l < h:
            p = partition(arr, l, h)
            stack.append((l, p - 1))
            stack.append((p + 1, h))


def main():
    N = 5_000_000
    RUNS = 10
    timings = []

    print("=== BENCHMARK: M1 - Custom QuickSort ===")
    print("Language: Python")
    print(f"Runtime:  Python {sys.version.split()[0]}")
    print()

    hash_val = ""

    for run in range(1, RUNS + 1):
        # Generate array with LCG (seed=42), interpreted as int32
        state = 42
        arr = [0] * N
        for i in range(N):
            state = (state * 1664525 + 1013904223) & 0xFFFFFFFF
            arr[i] = state if state < 0x80000000 else state - 0x100000000

        start = time.perf_counter()
        quicksort(arr, 0, N - 1)
        elapsed = (time.perf_counter() - start) * 1000
        timings.append(elapsed)
        print(f"Run {run:2d}: {elapsed:.2f} ms")

        if run == RUNS:
            h = hashlib.sha256()
            comma = b','
            for i, v in enumerate(arr):
                if i > 0:
                    h.update(comma)
                h.update(str(v).encode('ascii'))
            hash_val = h.hexdigest()

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
    print(f"Verification: SHA-256 = {hash_val}")


if __name__ == "__main__":
    main()

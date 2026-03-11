import time
import math
import sys

N = 1000
RUNS = 10

def main():
    print("=== BENCHMARK: A1 - Matrix Multiplication ===")
    print(f"Language: Python")
    print(f"Runtime:  Python {sys.version.split()[0]}")
    print()

    # Initialize matrices (flat row-major)
    a = [0.0] * (N * N)
    b = [0.0] * (N * N)
    for i in range(N):
        for j in range(N):
            a[i * N + j] = ((i * 1000 + j) % 97) * 0.01
            b[i * N + j] = ((j * 1000 + i) % 89) * 0.01

    c = [0.0] * (N * N)
    timings = []

    for run in range(RUNS):
        for i in range(N * N):
            c[i] = 0.0

        start = time.perf_counter()
        for i in range(N):
            for k in range(N):
                a_ik = a[i * N + k]
                for j in range(N):
                    c[i * N + j] += a_ik * b[k * N + j]
        elapsed = (time.perf_counter() - start) * 1000.0
        timings.append(elapsed)
        print(f"Run {run + 1:2}: {elapsed:.2f} ms")

    print()
    min_t = min(timings)
    max_t = max(timings)
    avg_t = sum(timings) / RUNS
    variance = sum((t - avg_t) ** 2 for t in timings) / RUNS
    stddev = math.sqrt(variance)

    print(f"Min:    {min_t:.2f} ms")
    print(f"Avg:    {avg_t:.2f} ms")
    print(f"Max:    {max_t:.2f} ms")
    print(f"StdDev: {stddev:.2f} ms")
    print()
    print("Verification:")
    print(f"  C[0][0]     = {c[0]:.6f}")
    print(f"  C[500][500] = {c[500 * N + 500]:.6f}")
    print(f"  C[999][999] = {c[999 * N + 999]:.6f}")

if __name__ == "__main__":
    main()

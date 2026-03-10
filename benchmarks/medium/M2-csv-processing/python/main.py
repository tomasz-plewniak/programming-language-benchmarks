import hashlib
import math
import os
import sys
import time

FIRST_NAMES = ["James", "Mary", "John", "Patricia", "Robert",
               "Jennifer", "Michael", "Linda", "David", "Elizabeth"]
LAST_NAMES = ["Smith", "Johnson", "Williams", "Brown", "Jones",
              "Garcia", "Miller", "Davis", "Rodriguez", "Martinez"]
DEPARTMENTS = ["Engineering", "Marketing", "Sales", "HR",
               "Finance", "Operations", "Support", "Legal"]

N = 1_000_000
RUNS = 10
CSV_FILE = "input.csv"
OUTPUT_FILE = "output.csv"


def generate_csv():
    state = 42
    with open(CSV_FILE, "w") as f:
        f.write("id,first_name,last_name,email,department,salary\n")
        for i in range(N):
            state = (state * 1664525 + 1013904223) & 0xFFFFFFFF
            salary = 30000.0 + (state / 4294967295.0) * 120000.0
            first = FIRST_NAMES[i % 10]
            last = LAST_NAMES[i % 10]
            dept = DEPARTMENTS[i % 8]
            f.write(f"{i + 1},{first},{last},{first.lower()}.{last.lower()}@company.com,{dept},{salary:.2f}\n")


def process():
    # Read CSV
    rows = []
    with open(CSV_FILE, "r") as f:
        f.readline()  # skip header
        for line in f:
            parts = line.rstrip('\n').split(',')
            salary = float(parts[5])
            if salary > 75000.0:
                rows.append((parts[4], salary))

    # Group by department
    groups = {}
    for dept, salary in rows:
        if dept not in groups:
            groups[dept] = [0, 0.0, -1e18, 1e18]
        g = groups[dept]
        g[0] += 1
        g[1] += salary
        if salary > g[2]:
            g[2] = salary
        if salary < g[3]:
            g[3] = salary

    # Build results
    results = []
    for dept, g in groups.items():
        count, total, mx, mn = g
        results.append((dept, count, total / count, mx, mn))

    # Sort by avg_salary descending
    results.sort(key=lambda x: -x[2])

    # Write output
    with open(OUTPUT_FILE, "w") as f:
        f.write("department,count,avg_salary,max_salary,min_salary\n")
        for dept, count, avg, mx, mn in results:
            f.write(f"{dept},{count},{avg:.2f},{mx:.2f},{mn:.2f}\n")


def main():
    timings = []

    print("=== BENCHMARK: M2 - CSV Processing ===")
    print("Language: Python")
    print(f"Runtime:  Python {sys.version.split()[0]}")
    print()

    generate_csv()

    for run in range(1, RUNS + 1):
        start = time.perf_counter()
        process()
        elapsed = (time.perf_counter() - start) * 1000
        timings.append(elapsed)
        print(f"Run {run:2d}: {elapsed:.2f} ms")

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

    # Verification: SHA-256 of output.csv
    h = hashlib.sha256()
    with open(OUTPUT_FILE, "rb") as f:
        h.update(f.read())
    print(f"Verification: SHA-256 = {h.hexdigest()}")

    os.remove(CSV_FILE)
    os.remove(OUTPUT_FILE)


if __name__ == "__main__":
    main()

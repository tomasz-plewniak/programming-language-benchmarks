using System;
using System.Collections.Generic;
using System.Diagnostics;

const int N = 5_000_000;
const int LOOKUPS = 1_000_000;
const int RUNS = 10;

double[] insertTimings = new double[RUNS];
double[] lookupTimings = new double[RUNS];

Console.WriteLine("=== BENCHMARK: M3 - Hash Map Stress Test ===");
Console.WriteLine("Language: C#");
Console.WriteLine($"Runtime:  .NET {Environment.Version}");
Console.WriteLine();

long verifySum = 0;
Stopwatch sw = new();

for (int run = 0; run < RUNS; run++)
{
    // Insert phase
    sw.Restart();
    var map = new Dictionary<string, long>(N);
    for (int i = 0; i < N; i++)
    {
        map[$"user_{i}"] = (long)i * 31 + 7;
    }
    sw.Stop();
    insertTimings[run] = sw.Elapsed.TotalMilliseconds;

    // Lookup phase
    sw.Restart();
    uint state = 42u;
    long sum = 0;
    for (int j = 0; j < LOOKUPS; j++)
    {
        state = unchecked(state * 1664525u + 1013904223u);
        int idx = (int)(state % (uint)N);
        sum += map[$"user_{idx}"];
    }
    sw.Stop();
    lookupTimings[run] = sw.Elapsed.TotalMilliseconds;

    verifySum = sum;
    Console.WriteLine($"Run {run + 1,2}: Insert: {insertTimings[run],8:F2} ms | Lookup: {lookupTimings[run],8:F2} ms | Total: {insertTimings[run] + lookupTimings[run],8:F2} ms");
}

Console.WriteLine();
PrintStats("Insert", insertTimings);
PrintStats("Lookup", lookupTimings);

double[] totalTimings = new double[RUNS];
for (int i = 0; i < RUNS; i++)
    totalTimings[i] = insertTimings[i] + lookupTimings[i];
PrintStats("Total", totalTimings);

Console.WriteLine($"Verification: sum = {verifySum}");

static void PrintStats(string label, double[] timings)
{
    double min = timings[0], max = timings[0], sum = 0;
    foreach (double t in timings)
    {
        sum += t;
        if (t < min) min = t;
        if (t > max) max = t;
    }
    double avg = sum / timings.Length;
    double variance = 0;
    foreach (double t in timings)
        variance += (t - avg) * (t - avg);
    variance /= timings.Length;
    double stddev = Math.Sqrt(variance);

    Console.WriteLine($"{label}:");
    Console.WriteLine($"  Min:    {min:F2} ms");
    Console.WriteLine($"  Avg:    {avg:F2} ms");
    Console.WriteLine($"  Max:    {max:F2} ms");
    Console.WriteLine($"  StdDev: {stddev:F2} ms");
    Console.WriteLine();
}

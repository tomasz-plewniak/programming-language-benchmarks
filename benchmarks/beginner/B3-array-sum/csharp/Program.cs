using System;
using System.Diagnostics;

const int N = 10_000_000;
const int RUNS = 10;
const long EXPECTED = 50000005000000;
double[] timings = new double[RUNS];

Console.WriteLine("=== BENCHMARK: B3 - Array Sum ===");
Console.WriteLine("Language: C#");
Console.WriteLine($"Runtime:  .NET {Environment.Version}");
Console.WriteLine();

long result = 0;
Stopwatch sw = new();

for (int i = 0; i < RUNS; i++)
{
    sw.Restart();
    long[] arr = new long[N];
    for (int j = 0; j < N; j++)
    {
        arr[j] = j + 1;
    }
    long s = 0;
    foreach (long v in arr)
    {
        s += v;
    }
    result = s;
    sw.Stop();

    timings[i] = sw.Elapsed.TotalMilliseconds;
    Console.WriteLine($"Run {i + 1,2}: {timings[i]:F2} ms");
}

Console.WriteLine();
double min = timings[0], max = timings[0], sum = 0;
foreach (double t in timings)
{
    sum += t;
    if (t < min) 
    {
        min = t;
    }

    if (t > max)
    {
       max = t; 
    }
}
double avg = sum / RUNS;
double variance = 0;
foreach (double t in timings)
{
    variance += (t - avg) * (t - avg);
}
variance /= RUNS;
double stddev = Math.Sqrt(variance);

Console.WriteLine($"Min:    {min:F2} ms");
Console.WriteLine($"Avg:    {avg:F2} ms");
Console.WriteLine($"Max:    {max:F2} ms");
Console.WriteLine($"StdDev: {stddev:F2} ms");
Console.WriteLine();
string status = result == EXPECTED ? "✓" : "✗";
Console.WriteLine($"Verification: {result} {status}");

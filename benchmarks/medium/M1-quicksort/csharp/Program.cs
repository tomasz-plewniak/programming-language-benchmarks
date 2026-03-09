using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

const int N = 5_000_000;
const int RUNS = 10;
double[] timings = new double[RUNS];
string hash = "";

Console.WriteLine("=== BENCHMARK: M1 - Custom QuickSort ===");
Console.WriteLine("Language: C#");
Console.WriteLine($"Runtime:  .NET {Environment.Version}");
Console.WriteLine();

int[] arr = new int[N];
Stopwatch sw = new();

for (int run = 0; run < RUNS; run++)
{
    // Generate array with LCG (seed=42), interpreted as int32
    uint state = 42u;
    for (int i = 0; i < N; i++)
    {
        state = unchecked(state * 1664525u + 1013904223u);
        arr[i] = (int)state;
    }

    sw.Restart();
    QuickSort(arr, 0, N - 1);
    sw.Stop();

    timings[run] = sw.Elapsed.TotalMilliseconds;
    Console.WriteLine($"Run {run + 1,2}: {timings[run]:F2} ms");

    if (run == RUNS - 1)
    {
        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        byte[] comma = [(byte)','];
        for (int i = 0; i < N; i++)
        {
            if (i > 0) hasher.AppendData(comma);
            hasher.AppendData(Encoding.ASCII.GetBytes(arr[i].ToString()));
        }
        hash = Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant();
    }
}

Console.WriteLine();
double min = timings[0], max = timings[0], sum = 0;
foreach (double t in timings)
{
    sum += t;
    if (t < min) min = t;
    if (t > max) max = t;
}
double avg = sum / RUNS;
double variance = 0;
foreach (double t in timings)
    variance += (t - avg) * (t - avg);
variance /= RUNS;
double stddev = Math.Sqrt(variance);

Console.WriteLine($"Min:    {min:F2} ms");
Console.WriteLine($"Avg:    {avg:F2} ms");
Console.WriteLine($"Max:    {max:F2} ms");
Console.WriteLine($"StdDev: {stddev:F2} ms");
Console.WriteLine();
Console.WriteLine($"Verification: SHA-256 = {hash}");

static void QuickSort(int[] arr, int lo, int hi)
{
    var stack = new Stack<(int, int)>();
    stack.Push((lo, hi));
    while (stack.Count > 0)
    {
        var (l, h) = stack.Pop();
        if (l < h)
        {
            int p = Partition(arr, l, h);
            stack.Push((l, p - 1));
            stack.Push((p + 1, h));
        }
    }
}

static int Partition(int[] arr, int lo, int hi)
{
    int pivot = arr[hi];
    int i = lo - 1;
    for (int j = lo; j < hi; j++)
    {
        if (arr[j] <= pivot)
        {
            i++;
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
    (arr[i + 1], arr[hi]) = (arr[hi], arr[i + 1]);
    return i + 1;
}

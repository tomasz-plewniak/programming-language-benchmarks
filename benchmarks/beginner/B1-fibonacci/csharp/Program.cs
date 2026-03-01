using System;
using System.Diagnostics;

const int N = 40;
const int RUNS = 10;
double[] timings = new double[RUNS];

Console.WriteLine("=== BENCHMARK: B1 - Fibonacci ===");
Console.WriteLine("Language: C#");
Console.WriteLine($"Runtime:  .NET {Environment.Version} (.NET 10)");
Console.WriteLine();

int result = 0;
for (int i = 0; i < RUNS; i++)
{
    Stopwatch sw = Stopwatch.StartNew();
    result = Fib(N);
    sw.Stop();
    double elapsed = sw.Elapsed.TotalMilliseconds;
    timings[i] = elapsed;
    Console.WriteLine($"Run {i + 1,2}: {elapsed:F2} ms");
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
    double diff = t - avg;
    variance += diff * diff;
}
double stddev = Math.Sqrt(variance / RUNS);

Console.WriteLine($"Min:    {min:F2} ms");
Console.WriteLine($"Avg:    {avg:F2} ms");
Console.WriteLine($"Max:    {max:F2} ms");
Console.WriteLine($"StdDev: {stddev:F2} ms");
Console.WriteLine();
string status = result == 102334155 ? "✓" : "✗";
Console.WriteLine($"Verification: {result} {status}");
return;

static int Fib(int n)
{
    if (n <= 1) return n;
    return Fib(n - 1) + Fib(n - 2);
}

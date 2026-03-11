using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

const int N = 1000;
const int RUNS = 10;

Console.WriteLine("=== BENCHMARK: A1 - Matrix Multiplication ===");
Console.WriteLine("Language: C#");
Console.WriteLine($"Runtime:  .NET {RuntimeInformation.FrameworkDescription.Replace(".NET ", "")}");
Console.WriteLine();

// Initialize matrices (flat row-major)
var a = new double[N * N];
var b = new double[N * N];
for (int i = 0; i < N; i++)
{
    for (int j = 0; j < N; j++)
    {
        a[i * N + j] = ((i * 1000 + j) % 97) * 0.01;
        b[i * N + j] = ((j * 1000 + i) % 89) * 0.01;
    }
}

var c = new double[N * N];
var timings = new double[RUNS];

for (int run = 0; run < RUNS; run++)
{
    Array.Clear(c);

    var sw = Stopwatch.StartNew();
    for (int i = 0; i < N; i++)
    {
        for (int k = 0; k < N; k++)
        {
            double a_ik = a[i * N + k];
            for (int j = 0; j < N; j++)
            {
                c[i * N + j] += a_ik * b[k * N + j];
            }
        }
    }
    sw.Stop();
    double elapsed = sw.Elapsed.TotalMilliseconds;
    timings[run] = elapsed;
    Console.WriteLine($"Run {run + 1,2}: {elapsed:F2} ms");
}

Console.WriteLine();
double min = double.MaxValue, max = double.MinValue, sum = 0;
foreach (var t in timings) { if (t < min) min = t; if (t > max) max = t; sum += t; }
double avg = sum / RUNS;
double variance = 0;
foreach (var t in timings) variance += (t - avg) * (t - avg);
variance /= RUNS;
double stddev = Math.Sqrt(variance);

Console.WriteLine($"Min:    {min:F2} ms");
Console.WriteLine($"Avg:    {avg:F2} ms");
Console.WriteLine($"Max:    {max:F2} ms");
Console.WriteLine($"StdDev: {stddev:F2} ms");
Console.WriteLine();
Console.WriteLine("Verification:");
Console.WriteLine($"  C[0][0]     = {c[0]:F6}");
Console.WriteLine($"  C[500][500] = {c[500 * N + 500]:F6}");
Console.WriteLine($"  C[999][999] = {c[999 * N + 999]:F6}");

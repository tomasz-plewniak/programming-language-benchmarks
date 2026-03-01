using System;
using System.Diagnostics;
using System.IO;

const int LINES = 1_000_000;
const int RUNS = 10;

string dataDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "..", "data"));
string filePath = Path.Combine(dataDir, "b2_lines.txt");

void WriteFile()
{
    Directory.CreateDirectory(dataDir);
    using StreamWriter writer = new StreamWriter(filePath);
    for (int i = 1; i <= LINES; i++)
    {
        writer.WriteLine($"Line {i}: The quick brown fox jumps over the lazy dog");
    }
}

double[] writeTimings = new double[RUNS];
double[] readTimings = new double[RUNS];

Console.WriteLine("=== BENCHMARK: B2 - File Line Counter ===");
Console.WriteLine("Language: C#");
Console.WriteLine($"Runtime:  .NET {Environment.Version} (.NET 10)");
Console.WriteLine();

int result = 0;
for (int i = 0; i < RUNS; i++)
{
    Stopwatch sw = Stopwatch.StartNew();
    WriteFile();
    sw.Stop();
    double writeElapsed = sw.Elapsed.TotalMilliseconds;

    sw = Stopwatch.StartNew();
    result = ReadAndCount();
    sw.Stop();
    double readElapsed = sw.Elapsed.TotalMilliseconds;

    writeTimings[i] = writeElapsed;
    readTimings[i] = readElapsed;
    double total = writeElapsed + readElapsed;
    Console.WriteLine($"Run {i + 1,2}: Write {writeElapsed:F2} ms | Read {readElapsed:F2} ms | Total {total:F2} ms");
}

Console.WriteLine();
foreach ((string? label, double[]? timings) in new[] { ("Write", writeTimings), ("Read", readTimings) })
{
    double min = timings[0], max = timings[0], sum = 0;
    foreach (double t in timings) { sum += t; if (t < min) min = t; if (t > max) max = t; }
    double avg = sum / RUNS;
    double variance = 0;
    foreach (double t in timings) variance += (t - avg) * (t - avg);
    double stddev = Math.Sqrt(variance / RUNS);
    Console.WriteLine($"{label} — Min: {min:F2} ms | Avg: {avg:F2} ms | Max: {max:F2} ms | StdDev: {stddev:F2} ms");
}

double[] totals = new double[RUNS];
for (int i = 0; i < RUNS; i++) totals[i] = writeTimings[i] + readTimings[i];
{
    double min = totals[0], max = totals[0], sum = 0;
    foreach (double t in totals)
    {
        sum += t; if (t < min) min = t; if (t > max) max = t;
    }
    
    double avg = sum / RUNS;
    double variance = 0;
    foreach (double t in totals) variance += (t - avg) * (t - avg);
    double stddev = Math.Sqrt(variance / RUNS);
    Console.WriteLine($"Total — Min: {min:F2} ms | Avg: {avg:F2} ms | Max: {max:F2} ms | StdDev: {stddev:F2} ms");
}

Console.WriteLine();
string status = result == LINES ? "✓" : "✗";
Console.WriteLine($"Verification: {result} {status}");

File.Delete(filePath);
return;

int ReadAndCount()
{
    int count = 0;
    using StreamReader reader = new StreamReader(filePath);
    while (reader.ReadLine() != null)
        count++;
    return count;
}

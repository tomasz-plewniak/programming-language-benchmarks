using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

const int N = 1_000_000;
const int RUNS = 10;
const string CsvFile = "input.csv";
const string OutputFile = "output.csv";

string[] firstNames = ["James", "Mary", "John", "Patricia", "Robert",
                       "Jennifer", "Michael", "Linda", "David", "Elizabeth"];
string[] lastNames = ["Smith", "Johnson", "Williams", "Brown", "Jones",
                      "Garcia", "Miller", "Davis", "Rodriguez", "Martinez"];
string[] departments = ["Engineering", "Marketing", "Sales", "HR",
                        "Finance", "Operations", "Support", "Legal"];

Console.WriteLine("=== BENCHMARK: M2 - CSV Processing ===");
Console.WriteLine("Language: C#");
Console.WriteLine($"Runtime:  .NET {Environment.Version}");
Console.WriteLine();

GenerateCsv();

double[] timings = new double[RUNS];
Stopwatch sw = new();

for (int run = 0; run < RUNS; run++)
{
    sw.Restart();
    Process();
    sw.Stop();
    timings[run] = sw.Elapsed.TotalMilliseconds;
    Console.WriteLine($"Run {run + 1,2}: {timings[run]:F2} ms");
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

// Verification: SHA-256 of output.csv
byte[] outputBytes = File.ReadAllBytes(OutputFile);
byte[] hashBytes = SHA256.HashData(outputBytes);
Console.WriteLine($"Verification: SHA-256 = {Convert.ToHexString(hashBytes).ToLowerInvariant()}");

File.Delete(CsvFile);
File.Delete(OutputFile);

void GenerateCsv()
{
    using var writer = new StreamWriter(CsvFile, false, new UTF8Encoding(false), 1 << 20);
    writer.NewLine = "\n";
    writer.WriteLine("id,first_name,last_name,email,department,salary");
    uint state = 42u;
    for (int i = 0; i < N; i++)
    {
        state = unchecked(state * 1664525u + 1013904223u);
        double salary = 30000.0 + (state / 4294967295.0) * 120000.0;
        string first = firstNames[i % 10];
        string last = lastNames[i % 10];
        string dept = departments[i % 8];
        writer.WriteLine($"{i + 1},{first},{last},{first.ToLowerInvariant()}.{last.ToLowerInvariant()}@company.com,{dept},{salary:F2}");
    }
}

void Process()
{
    var groups = new Dictionary<string, (int count, double total, double max, double min)>(8);

    using (var reader = new StreamReader(CsvFile))
    {
        reader.ReadLine(); // skip header
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            // Parse salary (last field) and department (second to last)
            int lastComma = line.LastIndexOf(',');
            double salary = double.Parse(line.AsSpan(lastComma + 1));
            if (salary > 75000.0)
            {
                int prevComma = line.LastIndexOf(',', lastComma - 1);
                string dept = line.Substring(prevComma + 1, lastComma - prevComma - 1);
                if (groups.TryGetValue(dept, out var g))
                {
                    groups[dept] = (g.count + 1, g.total + salary, Math.Max(g.max, salary), Math.Min(g.min, salary));
                }
                else
                {
                    groups[dept] = (1, salary, salary, salary);
                }
            }
        }
    }

    var results = new List<(string dept, int count, double avg, double max, double min)>(groups.Count);
    foreach (var (dept, g) in groups)
        results.Add((dept, g.count, g.total / g.count, g.max, g.min));

    results.Sort((a, b) => b.avg.CompareTo(a.avg));

    using var writer = new StreamWriter(OutputFile, false, new UTF8Encoding(false));
    writer.NewLine = "\n";
    writer.WriteLine("department,count,avg_salary,max_salary,min_salary");
    foreach (var (dept, count, avgSal, maxSal, minSal) in results)
        writer.WriteLine($"{dept},{count},{avgSal:F2},{maxSal:F2},{minSal:F2}");
}

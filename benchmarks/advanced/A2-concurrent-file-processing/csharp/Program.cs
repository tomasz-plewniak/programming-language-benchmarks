using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

const int NUM_FILES = 100;
const int LINES_PER_FILE = 100_000;
const int RUNS = 10;
const long SEED = 42;
const int TOP_N = 100;

string[] WORDS = {
    "the", "be", "to", "of", "and", "a", "in", "that", "have", "I",
    "it", "for", "not", "on", "with", "he", "as", "you", "do", "at",
    "this", "but", "his", "by", "from", "they", "we", "her", "she", "or",
    "an", "will", "my", "one", "all", "would", "there", "their", "what", "so",
    "up", "out", "if", "about", "who", "get", "which", "go", "me", "when",
    "make", "can", "like", "time", "no", "just", "him", "know", "take", "people",
    "into", "year", "your", "good", "some", "could", "them", "see", "other", "than",
    "then", "now", "look", "only", "come", "its", "over", "think", "also", "back",
    "after", "use", "two", "how", "our", "work", "first", "well", "way", "even",
    "new", "want", "because", "any", "these", "give", "day", "most", "us", "great",
    "between", "need", "large", "often", "hand", "high", "place", "old", "while", "mean",
    "keep", "let", "begin", "seem", "help", "show", "hear", "play", "run", "move",
    "live", "believe", "bring", "happen", "write", "provide", "sit", "stand", "lose", "pay",
    "meet", "include", "continue", "set", "learn", "change", "lead", "understand", "watch", "follow",
    "stop", "create", "speak", "read", "allow", "add", "spend", "grow", "open", "walk",
    "win", "teach", "offer", "remember", "love", "consider", "appear", "buy", "wait", "serve",
    "die", "send", "expect", "build", "stay", "fall", "cut", "reach", "kill", "remain",
    "suggest", "raise", "pass", "sell", "require", "report", "decide", "pull", "develop", "eat",
    "return", "hold", "cover", "point", "turn", "start", "close", "small", "number", "group",
    "always", "music", "those", "both", "mark", "call", "ask", "late", "home", "last",
    "long", "best", "still", "find", "head", "body", "water", "word", "money", "story",
    "fact", "month", "lot", "right", "study", "book", "eye", "job", "business", "issue",
    "side", "kind", "four", "room", "heart", "friend", "power", "city", "house", "party",
    "world", "area", "company", "problem", "during", "family", "government", "country", "question", "school",
    "state", "program", "information", "system", "service", "part", "idea", "table", "game", "child",
    "process", "since", "line", "result", "team", "model", "product", "market", "level", "local",
    "computer", "field", "car", "force", "food", "community", "end", "light", "real", "history",
    "political", "social", "general", "personal", "public", "national", "court", "young", "council", "war",
    "health", "age", "face", "policy", "research", "street", "law", "door", "office", "trade",
    "report", "student", "human", "data", "form", "value", "rate", "land", "project", "control",
    "action", "support", "order", "today", "figure", "class", "mother", "special", "case", "reason",
    "morning", "record", "air", "nature", "north", "sound", "effort", "fish", "plant", "true",
    "paper", "space", "event", "range", "plan", "type", "police", "road", "view", "south",
    "board", "cover", "price", "letter", "current", "future", "present", "past", "foreign", "central",
    "digital", "global", "final", "major", "natural", "popular", "cultural", "serious", "recent", "common",
    "left", "simple", "entire", "clear", "certain", "single", "source", "detail", "standard", "share",
    "modern", "potential", "key", "strong"
};

Console.WriteLine("=== BENCHMARK: A2 - Concurrent File Processing ===");
Console.WriteLine("Language: C#");
Console.WriteLine($"Runtime:  .NET {RuntimeInformation.FrameworkDescription.Replace(".NET ", "")}");
Console.WriteLine();

// Generate files
string dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data");
Directory.CreateDirectory(dataDir);
GenerateFiles(dataDir, WORDS);

var timings = new double[RUNS];

for (int run = 0; run < RUNS; run++)
{
    var sw = Stopwatch.StartNew();

    var globalCounts = new ConcurrentDictionary<string, long>();
    var tasks = new Task[NUM_FILES];
    for (int f = 0; f < NUM_FILES; f++)
    {
        string filePath = Path.Combine(dataDir, $"file_{f:D3}.txt");
        tasks[f] = Task.Run(() =>
        {
            var localCounts = new Dictionary<string, long>();
            foreach (var line in File.ReadLines(filePath))
            {
                foreach (var word in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (localCounts.TryGetValue(word, out long count))
                        localCounts[word] = count + 1;
                    else
                        localCounts[word] = 1;
                }
            }
            foreach (var kvp in localCounts)
            {
                globalCounts.AddOrUpdate(kvp.Key, kvp.Value, (_, old) => old + kvp.Value);
            }
        });
    }
    Task.WaitAll(tasks);

    sw.Stop();
    double elapsed = sw.Elapsed.TotalMilliseconds;
    timings[run] = elapsed;
    Console.WriteLine($"Run {run + 1,2}: {elapsed:F2} ms");

    if (run == RUNS - 1)
    {
        var top = globalCounts
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key)
            .Take(TOP_N)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("Verification:");
        Console.WriteLine($"  Total unique words: {globalCounts.Count}");
        Console.WriteLine($"  Top 5 words:");
        for (int i = 0; i < Math.Min(5, top.Count); i++)
            Console.WriteLine($"    {i + 1}. {top[i].Key} = {top[i].Value}");
        Console.WriteLine($"  Word #100: {top[Math.Min(99, top.Count - 1)].Key} = {top[Math.Min(99, top.Count - 1)].Value}");
    }
}

Console.WriteLine();
double min = timings.Min();
double max = timings.Max();
double avg = timings.Average();
double variance = timings.Select(t => (t - avg) * (t - avg)).Sum() / RUNS;
double stddev = Math.Sqrt(variance);

Console.WriteLine($"Min:    {min:F2} ms");
Console.WriteLine($"Avg:    {avg:F2} ms");
Console.WriteLine($"Max:    {max:F2} ms");
Console.WriteLine($"StdDev: {stddev:F2} ms");

// Cleanup
Directory.Delete(dataDir, true);

static void GenerateFiles(string dataDir, string[] words)
{
    long state = SEED;
    for (int f = 0; f < NUM_FILES; f++)
    {
        string filePath = Path.Combine(dataDir, $"file_{f:D3}.txt");
        using var writer = new StreamWriter(filePath);
        for (int line = 0; line < LINES_PER_FILE; line++)
        {
            state = (state * 1103515245 + 12345) & 0x7FFFFFFF;
            int wordCount = (int)(state % 11) + 5; // 5 to 15
            for (int w = 0; w < wordCount; w++)
            {
                if (w > 0) writer.Write(' ');
                state = (state * 1103515245 + 12345) & 0x7FFFFFFF;
                writer.Write(words[state % words.Length]);
            }
            writer.WriteLine();
        }
    }
}

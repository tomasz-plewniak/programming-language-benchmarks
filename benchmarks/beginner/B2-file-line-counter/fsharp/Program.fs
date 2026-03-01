open System
open System.Diagnostics
open System.IO

let LINES = 1_000_000
let RUNS = 10

let dataDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "..", "data"))
let filePath = Path.Combine(dataDir, "b2_lines.txt")

let writeFile () =
    Directory.CreateDirectory(dataDir) |> ignore
    use writer = new StreamWriter(filePath)
    for i in 1 .. LINES do
        writer.WriteLine($"Line {i}: The quick brown fox jumps over the lazy dog")

let readAndCount () =
    use reader = new StreamReader(filePath)
    let mutable count = 0
    while reader.ReadLine() <> null do
        count <- count + 1
    count

let writeTimings = Array.zeroCreate<float> RUNS
let readTimings = Array.zeroCreate<float> RUNS

printfn "=== BENCHMARK: B2 - File Line Counter ==="
printfn "Language: F#"
printfn $"Runtime:  .NET {Environment.Version} (.NET 10)"
printfn ""

let mutable result = 0
for i in 0 .. RUNS - 1 do
    let sw = Stopwatch.StartNew()
    writeFile ()
    sw.Stop()
    let writeElapsed = sw.Elapsed.TotalMilliseconds

    let sw2 = Stopwatch.StartNew()
    result <- readAndCount ()
    sw2.Stop()
    let readElapsed = sw2.Elapsed.TotalMilliseconds

    writeTimings[i] <- writeElapsed
    readTimings[i] <- readElapsed
    let total = writeElapsed + readElapsed
    printfn $"Run %2d{i + 1}: Write %.2f{writeElapsed} ms | Read %.2f{readElapsed} ms | Total %.2f{total} ms"

printfn ""
for label, timings in [("Write", writeTimings); ("Read", readTimings)] do
    let minT = Array.min timings
    let maxT = Array.max timings
    let avg = Array.average timings
    let variance = timings |> Array.averageBy (fun t -> (t - avg) ** 2.0)
    let stddev = sqrt variance
    printfn $"{label} — Min: %.2f{minT} ms | Avg: %.2f{avg} ms | Max: %.2f{maxT} ms | StdDev: %.2f{stddev} ms"

let totals = Array.init RUNS (fun i -> writeTimings[i] + readTimings[i])
let minT = Array.min totals
let maxT = Array.max totals
let avg = Array.average totals
let variance = totals |> Array.averageBy (fun t -> (t - avg) ** 2.0)
let stddev = sqrt variance
printfn $"Total — Min: %.2f{minT} ms | Avg: %.2f{avg} ms | Max: %.2f{maxT} ms | StdDev: %.2f{stddev} ms"

printfn ""
let status = if result = LINES then "✓" else "✗"
printfn $"Verification: {result} {status}"

File.Delete(filePath)

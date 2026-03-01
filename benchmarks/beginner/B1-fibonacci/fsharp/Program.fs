open System
open System.Diagnostics

let rec fib n =
    if n <= 1 then n
    else fib (n - 1) + fib (n - 2)

let N = 40
let RUNS = 10
let timings = Array.zeroCreate<float> RUNS

printfn "=== BENCHMARK: B1 - Fibonacci ==="
printfn "Language: F#"
printfn $"Runtime:  .NET {Environment.Version} (.NET 10)"
printfn ""

let mutable result = 0
for i in 0 .. RUNS - 1 do
    let sw = Stopwatch.StartNew()
    result <- fib N
    sw.Stop()
    let elapsed = sw.Elapsed.TotalMilliseconds
    timings[i] <- elapsed
    printfn $"Run %2d{i + 1}: %.2f{elapsed} ms"

printfn ""
let minT = Array.min timings
let maxT = Array.max timings
let avg = Array.average timings
let variance = timings |> Array.averageBy (fun t -> (t - avg) ** 2.0)
let stddev = sqrt variance

printfn $"Min:    %.2f{minT} ms"
printfn $"Avg:    %.2f{avg} ms"
printfn $"Max:    %.2f{maxT} ms"
printfn $"StdDev: %.2f{stddev} ms"
printfn ""
let status = if result = 102334155 then "✓" else "✗"
printfn $"Verification: {result} {status}"

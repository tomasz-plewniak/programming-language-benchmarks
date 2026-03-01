open System
open System.Diagnostics

[<EntryPoint>]
let main (argv: string array) =
    let n = 10_000_000
    let runs = 10
    let expected = 50000005000000L
    let timings = Array.zeroCreate<float> runs

    printfn "=== BENCHMARK: B3 - Array Sum ==="
    printfn "Language: F#"
    printfn "Runtime:  .NET %O" Environment.Version
    printfn ""

    let mutable result = 0L
    let sw = Stopwatch()

    for i in 0 .. runs - 1 do
        sw.Restart()
        let arr = Array.init n (fun j -> int64 (j + 1))
        let mutable s = 0L
        for j in 0 .. n - 1 do
            s <- s + arr.[j]
        result <- s
        sw.Stop()

        timings.[i] <- sw.Elapsed.TotalMilliseconds
        printfn "Run %2d: %.2f ms" (i + 1) timings.[i]

    printfn ""
    let minT = Array.min timings
    let maxT = Array.max timings
    let avg = Array.average timings
    let variance = timings |> Array.averageBy (fun t -> (t - avg) ** 2.0)
    let stddev = sqrt variance

    printfn "Min:    %.2f ms" minT
    printfn "Avg:    %.2f ms" avg
    printfn "Max:    %.2f ms" maxT
    printfn "StdDev: %.2f ms" stddev
    printfn ""
    let status = if result = expected then "✓" else "✗"
    printfn "Verification: %d %s" result status
    0

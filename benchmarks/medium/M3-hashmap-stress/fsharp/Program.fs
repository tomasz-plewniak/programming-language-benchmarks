open System
open System.Collections.Generic
open System.Diagnostics

[<EntryPoint>]
let main _ =
    let n = 5_000_000
    let lookups = 1_000_000
    let runs = 10
    let insertTimings = Array.zeroCreate<float> runs
    let lookupTimings = Array.zeroCreate<float> runs

    printfn "=== BENCHMARK: M3 - Hash Map Stress Test ==="
    printfn "Language: F#"
    printfn "Runtime:  .NET %O" Environment.Version
    printfn ""

    let mutable verifySum = 0L
    let sw = Stopwatch()

    for run in 0 .. runs - 1 do
        // Insert phase
        sw.Restart()
        let map = Dictionary<string, int64>(n)
        for i in 0 .. n - 1 do
            map.[sprintf "user_%d" i] <- int64 i * 31L + 7L
        sw.Stop()
        insertTimings.[run] <- sw.Elapsed.TotalMilliseconds

        // Lookup phase
        sw.Restart()
        let mutable state = 42u
        let mutable sum = 0L
        for _ in 0 .. lookups - 1 do
            state <- state * 1664525u + 1013904223u
            let idx = int (uint64 state % uint64 n)
            sum <- sum + map.[sprintf "user_%d" idx]
        sw.Stop()
        lookupTimings.[run] <- sw.Elapsed.TotalMilliseconds

        verifySum <- sum
        printfn "Run %2d: Insert: %8.2f ms | Lookup: %8.2f ms | Total: %8.2f ms"
            (run + 1) insertTimings.[run] lookupTimings.[run] (insertTimings.[run] + lookupTimings.[run])

    printfn ""

    let printStats label (timings: float[]) =
        let minT = Array.min timings
        let maxT = Array.max timings
        let avg = Array.average timings
        let variance = timings |> Array.averageBy (fun t -> (t - avg) ** 2.0)
        let stddev = sqrt variance
        printfn "%s:" label
        printfn "  Min:    %.2f ms" minT
        printfn "  Avg:    %.2f ms" avg
        printfn "  Max:    %.2f ms" maxT
        printfn "  StdDev: %.2f ms" stddev
        printfn ""

    printStats "Insert" insertTimings
    printStats "Lookup" lookupTimings

    let totalTimings = Array.init runs (fun i -> insertTimings.[i] + lookupTimings.[i])
    printStats "Total" totalTimings

    printfn "Verification: sum = %d" verifySum
    0

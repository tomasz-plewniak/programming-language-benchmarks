open System
open System.Collections.Generic
open System.Diagnostics
open System.Security.Cryptography
open System.Text

[<EntryPoint>]
let main _ =
    let n = 5_000_000
    let runs = 10
    let timings = Array.zeroCreate<float> runs

    printfn "=== BENCHMARK: M1 - Custom QuickSort ==="
    printfn "Language: F#"
    printfn "Runtime:  .NET %O" Environment.Version
    printfn ""

    let arr = Array.zeroCreate<int> n
    let sw = Stopwatch()
    let mutable hash = ""

    let partition (a: int[]) lo hi =
        let pivot = a.[hi]
        let mutable i = lo - 1
        for j in lo .. hi - 1 do
            if a.[j] <= pivot then
                i <- i + 1
                let tmp = a.[i]
                a.[i] <- a.[j]
                a.[j] <- tmp
        let tmp = a.[i + 1]
        a.[i + 1] <- a.[hi]
        a.[hi] <- tmp
        i + 1

    let quickSort (a: int[]) startLo startHi =
        let stack = Stack<struct (int * int)>()
        stack.Push(struct (startLo, startHi))
        while stack.Count > 0 do
            let struct (l, h) = stack.Pop()
            if l < h then
                let p = partition a l h
                stack.Push(struct (l, p - 1))
                stack.Push(struct (p + 1, h))

    for run in 0 .. runs - 1 do
        // Generate array with LCG (seed=42), interpreted as int32
        let mutable state = 42u
        for i in 0 .. n - 1 do
            state <- state * 1664525u + 1013904223u
            arr.[i] <- int state

        sw.Restart()
        quickSort arr 0 (n - 1)
        sw.Stop()

        timings.[run] <- sw.Elapsed.TotalMilliseconds
        printfn "Run %2d: %.2f ms" (run + 1) timings.[run]

        if run = runs - 1 then
            use hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256)
            let comma = [| byte ',' |]
            for i in 0 .. n - 1 do
                if i > 0 then hasher.AppendData(comma)
                hasher.AppendData(Encoding.ASCII.GetBytes(string arr.[i]))
            hash <- Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant()

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
    printfn "Verification: SHA-256 = %s" hash
    0

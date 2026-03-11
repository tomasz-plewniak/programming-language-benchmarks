open System
open System.Diagnostics
open System.Runtime.InteropServices

[<EntryPoint>]
let main _ =
    let n = 1000
    let runs = 10

    printfn "=== BENCHMARK: A1 - Matrix Multiplication ==="
    printfn "Language: F#"
    printfn "Runtime:  .NET %s" (RuntimeInformation.FrameworkDescription.Replace(".NET ", ""))
    printfn ""

    // Initialize matrices (flat row-major)
    let a = Array.init (n * n) (fun idx ->
        let i = idx / n
        let j = idx % n
        float ((i * 1000 + j) % 97) * 0.01)
    let b = Array.init (n * n) (fun idx ->
        let i = idx / n
        let j = idx % n
        float ((j * 1000 + i) % 89) * 0.01)

    let c = Array.zeroCreate<float> (n * n)
    let timings = Array.zeroCreate<float> runs

    for run in 0 .. runs - 1 do
        Array.Clear(c, 0, c.Length)

        let sw = Stopwatch.StartNew()
        for i in 0 .. n - 1 do
            for k in 0 .. n - 1 do
                let a_ik = a.[i * n + k]
                for j in 0 .. n - 1 do
                    c.[i * n + j] <- c.[i * n + j] + a_ik * b.[k * n + j]
        sw.Stop()
        let elapsed = sw.Elapsed.TotalMilliseconds
        timings.[run] <- elapsed
        printfn "Run %2d: %.2f ms" (run + 1) elapsed

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
    printfn "Verification:"
    printfn "  C[0][0]     = %.6f" c.[0]
    printfn "  C[500][500] = %.6f" c.[500 * n + 500]
    printfn "  C[999][999] = %.6f" c.[999 * n + 999]
    0

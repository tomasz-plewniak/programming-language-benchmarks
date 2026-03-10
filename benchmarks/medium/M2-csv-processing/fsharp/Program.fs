open System
open System.Collections.Generic
open System.Diagnostics
open System.Globalization
open System.IO
open System.Security.Cryptography
open System.Text

[<EntryPoint>]
let main _ =
    CultureInfo.CurrentCulture <- CultureInfo.InvariantCulture
    let n = 1_000_000
    let runs = 10
    let csvFile = "input.csv"
    let outputFile = "output.csv"

    let firstNames = [| "James"; "Mary"; "John"; "Patricia"; "Robert";
                        "Jennifer"; "Michael"; "Linda"; "David"; "Elizabeth" |]
    let lastNames = [| "Smith"; "Johnson"; "Williams"; "Brown"; "Jones";
                       "Garcia"; "Miller"; "Davis"; "Rodriguez"; "Martinez" |]
    let departments = [| "Engineering"; "Marketing"; "Sales"; "HR";
                         "Finance"; "Operations"; "Support"; "Legal" |]

    printfn "=== BENCHMARK: M2 - CSV Processing ==="
    printfn "Language: F#"
    printfn "Runtime:  .NET %O" Environment.Version
    printfn ""

    // Generate CSV
    do
        use writer = new StreamWriter(csvFile, false, UTF8Encoding(false), 1 <<< 20)
        writer.NewLine <- "\n"
        writer.WriteLine("id,first_name,last_name,email,department,salary")
        let mutable state = 42u
        for i in 0 .. n - 1 do
            state <- state * 1664525u + 1013904223u
            let salary = 30000.0 + (float state / 4294967295.0) * 120000.0
            let first = firstNames.[i % 10]
            let last = lastNames.[i % 10]
            let dept = departments.[i % 8]
            writer.WriteLine($"{i + 1},{first},{last},{first.ToLowerInvariant()}.{last.ToLowerInvariant()}@company.com,{dept},{salary:F2}")

    let timings = Array.zeroCreate<float> runs
    let sw = Stopwatch()

    let processFile () =
        let groups = Dictionary<string, struct (int * float * float * float)>(8)

        use reader = new StreamReader(csvFile)
        reader.ReadLine() |> ignore // skip header
        let mutable line = reader.ReadLine()
        while not (isNull line) do
            let lastComma = line.LastIndexOf(',')
            let salary = Double.Parse(line.AsSpan(lastComma + 1))
            if salary > 75000.0 then
                let prevComma = line.LastIndexOf(',', lastComma - 1)
                let dept = line.Substring(prevComma + 1, lastComma - prevComma - 1)
                match groups.TryGetValue(dept) with
                | true, struct (c, t, mx, mn) ->
                    groups.[dept] <- struct (c + 1, t + salary, max mx salary, min mn salary)
                | false, _ ->
                    groups.[dept] <- struct (1, salary, salary, salary)
            line <- reader.ReadLine()

        let results = ResizeArray(groups.Count)
        for kv in groups do
            let struct (c, t, mx, mn) = kv.Value
            results.Add(struct (kv.Key, c, t / float c, mx, mn))

        results.Sort(fun (struct (_, _, a, _, _)) (struct (_, _, b, _, _)) -> compare b a)

        use writer = new StreamWriter(outputFile, false, UTF8Encoding(false))
        writer.NewLine <- "\n"
        writer.WriteLine("department,count,avg_salary,max_salary,min_salary")
        for struct (dept, count, avgSal, maxSal, minSal) in results do
            writer.WriteLine($"{dept},{count},{avgSal:F2},{maxSal:F2},{minSal:F2}")

    for run in 0 .. runs - 1 do
        sw.Restart()
        processFile ()
        sw.Stop()
        timings.[run] <- sw.Elapsed.TotalMilliseconds
        printfn "Run %2d: %.2f ms" (run + 1) timings.[run]

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

    // Verification: SHA-256 of output.csv
    let outputBytes = File.ReadAllBytes(outputFile)
    let hashBytes = SHA256.HashData(outputBytes)
    printfn "Verification: SHA-256 = %s" (Convert.ToHexString(hashBytes).ToLowerInvariant())

    File.Delete(csvFile)
    File.Delete(outputFile)
    0

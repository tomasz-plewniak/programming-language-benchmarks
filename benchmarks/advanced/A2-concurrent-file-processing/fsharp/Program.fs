open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Runtime.InteropServices

let words = [|
    "the"; "be"; "to"; "of"; "and"; "a"; "in"; "that"; "have"; "I"
    "it"; "for"; "not"; "on"; "with"; "he"; "as"; "you"; "do"; "at"
    "this"; "but"; "his"; "by"; "from"; "they"; "we"; "her"; "she"; "or"
    "an"; "will"; "my"; "one"; "all"; "would"; "there"; "their"; "what"; "so"
    "up"; "out"; "if"; "about"; "who"; "get"; "which"; "go"; "me"; "when"
    "make"; "can"; "like"; "time"; "no"; "just"; "him"; "know"; "take"; "people"
    "into"; "year"; "your"; "good"; "some"; "could"; "them"; "see"; "other"; "than"
    "then"; "now"; "look"; "only"; "come"; "its"; "over"; "think"; "also"; "back"
    "after"; "use"; "two"; "how"; "our"; "work"; "first"; "well"; "way"; "even"
    "new"; "want"; "because"; "any"; "these"; "give"; "day"; "most"; "us"; "great"
    "between"; "need"; "large"; "often"; "hand"; "high"; "place"; "old"; "while"; "mean"
    "keep"; "let"; "begin"; "seem"; "help"; "show"; "hear"; "play"; "run"; "move"
    "live"; "believe"; "bring"; "happen"; "write"; "provide"; "sit"; "stand"; "lose"; "pay"
    "meet"; "include"; "continue"; "set"; "learn"; "change"; "lead"; "understand"; "watch"; "follow"
    "stop"; "create"; "speak"; "read"; "allow"; "add"; "spend"; "grow"; "open"; "walk"
    "win"; "teach"; "offer"; "remember"; "love"; "consider"; "appear"; "buy"; "wait"; "serve"
    "die"; "send"; "expect"; "build"; "stay"; "fall"; "cut"; "reach"; "kill"; "remain"
    "suggest"; "raise"; "pass"; "sell"; "require"; "report"; "decide"; "pull"; "develop"; "eat"
    "return"; "hold"; "cover"; "point"; "turn"; "start"; "close"; "small"; "number"; "group"
    "always"; "music"; "those"; "both"; "mark"; "call"; "ask"; "late"; "home"; "last"
    "long"; "best"; "still"; "find"; "head"; "body"; "water"; "word"; "money"; "story"
    "fact"; "month"; "lot"; "right"; "study"; "book"; "eye"; "job"; "business"; "issue"
    "side"; "kind"; "four"; "room"; "heart"; "friend"; "power"; "city"; "house"; "party"
    "world"; "area"; "company"; "problem"; "during"; "family"; "government"; "country"; "question"; "school"
    "state"; "program"; "information"; "system"; "service"; "part"; "idea"; "table"; "game"; "child"
    "process"; "since"; "line"; "result"; "team"; "model"; "product"; "market"; "level"; "local"
    "computer"; "field"; "car"; "force"; "food"; "community"; "end"; "light"; "real"; "history"
    "political"; "social"; "general"; "personal"; "public"; "national"; "court"; "young"; "council"; "war"
    "health"; "age"; "face"; "policy"; "research"; "street"; "law"; "door"; "office"; "trade"
    "report"; "student"; "human"; "data"; "form"; "value"; "rate"; "land"; "project"; "control"
    "action"; "support"; "order"; "today"; "figure"; "class"; "mother"; "special"; "case"; "reason"
    "morning"; "record"; "air"; "nature"; "north"; "sound"; "effort"; "fish"; "plant"; "true"
    "paper"; "space"; "event"; "range"; "plan"; "type"; "police"; "road"; "view"; "south"
    "board"; "cover"; "price"; "letter"; "current"; "future"; "present"; "past"; "foreign"; "central"
    "digital"; "global"; "final"; "major"; "natural"; "popular"; "cultural"; "serious"; "recent"; "common"
    "left"; "simple"; "entire"; "clear"; "certain"; "single"; "source"; "detail"; "standard"; "share"
    "modern"; "potential"; "key"; "strong"
|]

let numFiles = 100
let linesPerFile = 100_000
let runs = 10
let seed = 42L
let topN = 100

let generateFiles (dataDir: string) =
    let mutable state = seed
    for f in 0 .. numFiles - 1 do
        let filePath = Path.Combine(dataDir, sprintf "file_%03d.txt" f)
        use writer = new StreamWriter(filePath)
        for _ in 0 .. linesPerFile - 1 do
            state <- (state * 1103515245L + 12345L) &&& 0x7FFFFFFFL
            let wordCount = int (state % 11L) + 5
            for w in 0 .. wordCount - 1 do
                if w > 0 then writer.Write(' ')
                state <- (state * 1103515245L + 12345L) &&& 0x7FFFFFFFL
                writer.Write(words.[int (state % int64 words.Length)])
            writer.WriteLine()

[<EntryPoint>]
let main _ =
    printfn "=== BENCHMARK: A2 - Concurrent File Processing ==="
    printfn "Language: F#"
    printfn "Runtime:  .NET %s" (RuntimeInformation.FrameworkDescription.Replace(".NET ", ""))
    printfn ""

    let dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data")
    Directory.CreateDirectory(dataDir) |> ignore
    generateFiles dataDir

    let timings = Array.zeroCreate<float> runs

    for run in 0 .. runs - 1 do
        let sw = Stopwatch.StartNew()

        let globalCounts = ConcurrentDictionary<string, int64>()
        let tasks =
            [| for f in 0 .. numFiles - 1 ->
                let filePath = Path.Combine(dataDir, sprintf "file_%03d.txt" f)
                async {
                    let localCounts = Dictionary<string, int64>()
                    for line in File.ReadLines(filePath) do
                        for word in line.Split(' ', StringSplitOptions.RemoveEmptyEntries) do
                            match localCounts.TryGetValue(word) with
                            | true, count -> localCounts.[word] <- count + 1L
                            | false, _ -> localCounts.[word] <- 1L
                    for kvp in localCounts do
                        globalCounts.AddOrUpdate(kvp.Key, kvp.Value, fun _ old -> old + kvp.Value) |> ignore
                } |]
        tasks |> Async.Parallel |> Async.RunSynchronously |> ignore

        sw.Stop()
        let elapsed = sw.Elapsed.TotalMilliseconds
        timings.[run] <- elapsed
        printfn "Run %2d: %.2f ms" (run + 1) elapsed

        if run = runs - 1 then
            let topList =
                globalCounts
                |> Seq.sortWith (fun a b ->
                    let cmp = compare b.Value a.Value
                    if cmp <> 0 then cmp else compare a.Key b.Key)
                |> Seq.take topN
                |> Seq.toArray

            printfn ""
            printfn "Verification:"
            printfn "  Total unique words: %d" globalCounts.Count
            printfn "  Top 5 words:"
            for i in 0 .. min 4 (topList.Length - 1) do
                printfn "    %d. %s = %d" (i + 1) topList.[i].Key topList.[i].Value
            printfn "  Word #100: %s = %d" topList.[min 99 (topList.Length - 1)].Key topList.[min 99 (topList.Length - 1)].Value

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

    Directory.Delete(dataDir, true)
    0

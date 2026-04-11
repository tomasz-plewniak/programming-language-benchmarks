const fs = require("fs");
const path = require("path");
const { Worker, isMainThread, parentPort, workerData } = require("worker_threads");

const NUM_FILES = 100;
const LINES_PER_FILE = 100_000;
const RUNS = 10;
const SEED = 42;
const TOP_N = 100;

const WORDS = [
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
    "modern", "potential", "key", "strong",
];

if (!isMainThread) {
    // Worker thread: read file and count words
    const filePath = workerData.filePath;
    const content = fs.readFileSync(filePath, "utf-8");
    const counts = new Map();
    const lines = content.split("\n");
    for (const line of lines) {
        if (line.length === 0) continue;
        const words = line.split(" ");
        for (const word of words) {
            if (word.length === 0) continue;
            counts.set(word, (counts.get(word) || 0) + 1);
        }
    }
    // Convert Map to object for serialization
    const result = {};
    for (const [word, count] of counts) {
        result[word] = count;
    }
    parentPort.postMessage(result);
} else {
    function generateFiles(dataDir) {
        let state = BigInt(SEED);
        const mask = BigInt(0x7FFFFFFF);
        const a = BigInt(1103515245);
        const c = BigInt(12345);
        const wordLen = BigInt(WORDS.length);

        for (let f = 0; f < NUM_FILES; f++) {
            const filePath = path.join(dataDir, `file_${String(f).padStart(3, "0")}.txt`);
            const lines = [];
            for (let line = 0; line < LINES_PER_FILE; line++) {
                state = (state * a + c) & mask;
                const wc = Number(state % 11n) + 5;
                const wordsOnLine = [];
                for (let w = 0; w < wc; w++) {
                    state = (state * a + c) & mask;
                    wordsOnLine.push(WORDS[Number(state % wordLen)]);
                }
                lines.push(wordsOnLine.join(" "));
            }
            fs.writeFileSync(filePath, lines.join("\n") + "\n");
        }
    }

    async function processFilesWithWorkers(dataDir) {
        const promises = [];
        for (let f = 0; f < NUM_FILES; f++) {
            const filePath = path.join(dataDir, `file_${String(f).padStart(3, "0")}.txt`);
            promises.push(
                new Promise((resolve, reject) => {
                    const worker = new Worker(__filename, {
                        workerData: { filePath },
                    });
                    worker.on("message", resolve);
                    worker.on("error", reject);
                })
            );
        }
        const results = await Promise.all(promises);

        const globalCounts = new Map();
        for (const localCounts of results) {
            for (const [word, count] of Object.entries(localCounts)) {
                globalCounts.set(word, (globalCounts.get(word) || 0) + count);
            }
        }
        return globalCounts;
    }

    async function main() {
        console.log("=== BENCHMARK: A2 - Concurrent File Processing ===");
        console.log("Language: JavaScript");
        console.log(`Runtime:  Node.js ${process.version}`);
        console.log();

        const dataDir = path.join(process.cwd(), "data");
        fs.mkdirSync(dataDir, { recursive: true });
        generateFiles(dataDir);

        const timings = [];

        for (let run = 0; run < RUNS; run++) {
            const start = performance.now();
            const globalCounts = await processFilesWithWorkers(dataDir);
            const elapsed = performance.now() - start;
            timings.push(elapsed);
            console.log(`Run ${String(run + 1).padStart(2)}: ${elapsed.toFixed(2)} ms`);

            if (run === RUNS - 1) {
                const sorted = [...globalCounts.entries()].sort((a, b) => {
                    if (b[1] !== a[1]) return b[1] - a[1];
                    return a[0].localeCompare(b[0]);
                });
                const top = sorted.slice(0, TOP_N);

                console.log();
                console.log("Verification:");
                console.log(`  Total unique words: ${globalCounts.size}`);
                console.log("  Top 5 words:");
                for (let i = 0; i < Math.min(5, top.length); i++) {
                    console.log(`    ${i + 1}. ${top[i][0]} = ${top[i][1]}`);
                }
                const idx = Math.min(99, top.length - 1);
                console.log(`  Word #100: ${top[idx][0]} = ${top[idx][1]}`);
            }
        }

        console.log();
        const minT = Math.min(...timings);
        const maxT = Math.max(...timings);
        const avg = timings.reduce((a, b) => a + b, 0) / RUNS;
        const variance = timings.reduce((s, t) => s + (t - avg) ** 2, 0) / RUNS;
        const stddev = Math.sqrt(variance);

        console.log(`Min:    ${minT.toFixed(2)} ms`);
        console.log(`Avg:    ${avg.toFixed(2)} ms`);
        console.log(`Max:    ${maxT.toFixed(2)} ms`);
        console.log(`StdDev: ${stddev.toFixed(2)} ms`);

        fs.rmSync(dataDir, { recursive: true, force: true });
    }

    main();
}

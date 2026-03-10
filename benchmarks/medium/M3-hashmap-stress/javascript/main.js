function main() {
    const N = 5_000_000;
    const LOOKUPS = 1_000_000;
    const RUNS = 10;

    const insertTimings = [];
    const lookupTimings = [];

    console.log("=== BENCHMARK: M3 - Hash Map Stress Test ===");
    console.log("Language: JavaScript");
    console.log(`Runtime:  Node.js ${process.version}`);
    console.log();

    let verifySum = 0n;

    for (let run = 1; run <= RUNS; run++) {
        // Insert phase
        const startInsert = process.hrtime.bigint();
        const map = new Map();
        for (let i = 0; i < N; i++) {
            map.set(`user_${i}`, BigInt(i) * 31n + 7n);
        }
        const insertElapsed = Number(process.hrtime.bigint() - startInsert) / 1e6;
        insertTimings.push(insertElapsed);

        // Lookup phase
        const startLookup = process.hrtime.bigint();
        let state = 42;
        let sum = 0n;
        for (let j = 0; j < LOOKUPS; j++) {
            state = (Math.imul(state, 1664525) + 1013904223) >>> 0;
            const idx = state % N;
            sum += map.get(`user_${idx}`);
        }
        const lookupElapsed = Number(process.hrtime.bigint() - startLookup) / 1e6;
        lookupTimings.push(lookupElapsed);

        verifySum = sum;
        console.log(`Run ${String(run).padStart(2)}: Insert: ${insertElapsed.toFixed(2).padStart(8)} ms | Lookup: ${lookupElapsed.toFixed(2).padStart(8)} ms | Total: ${(insertElapsed + lookupElapsed).toFixed(2).padStart(8)} ms`);
    }

    console.log();
    printStats("Insert", insertTimings);
    printStats("Lookup", lookupTimings);

    const totalTimings = insertTimings.map((v, i) => v + lookupTimings[i]);
    printStats("Total", totalTimings);

    console.log(`Verification: sum = ${verifySum}`);
}

function printStats(label, timings) {
    const min = Math.min(...timings);
    const max = Math.max(...timings);
    const avg = timings.reduce((a, b) => a + b, 0) / timings.length;
    const variance = timings.reduce((a, t) => a + (t - avg) ** 2, 0) / timings.length;
    const stddev = Math.sqrt(variance);

    console.log(`${label}:`);
    console.log(`  Min:    ${min.toFixed(2)} ms`);
    console.log(`  Avg:    ${avg.toFixed(2)} ms`);
    console.log(`  Max:    ${max.toFixed(2)} ms`);
    console.log(`  StdDev: ${stddev.toFixed(2)} ms`);
    console.log();
}

main();

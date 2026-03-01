function main() {
    const N = 10_000_000;
    const RUNS = 10;
    const EXPECTED = 50000005000000;
    const timings = [];

    console.log("=== BENCHMARK: B3 - Array Sum ===");
    console.log("Language: JavaScript");
    console.log(`Runtime:  Node.js ${process.version}`);
    console.log();

    let result = 0;
    for (let i = 1; i <= RUNS; i++) {
        const start = process.hrtime.bigint();
        const arr = new Array(N);
        for (let j = 0; j < N; j++) {
            arr[j] = j + 1;
        }
        let s = 0;
        for (let j = 0; j < N; j++) {
            s += arr[j];
        }
        result = s;
        const elapsed = Number(process.hrtime.bigint() - start) / 1e6; // ms
        timings.push(elapsed);
        console.log(`Run ${String(i).padStart(2)}: ${elapsed.toFixed(2)} ms`);
    }

    console.log();
    const min = Math.min(...timings);
    const max = Math.max(...timings);
    const avg = timings.reduce((a, b) => a + b, 0) / timings.length;
    const variance = timings.reduce((a, t) => a + (t - avg) ** 2, 0) / timings.length;
    const stddev = Math.sqrt(variance);

    console.log(`Min:    ${min.toFixed(2)} ms`);
    console.log(`Avg:    ${avg.toFixed(2)} ms`);
    console.log(`Max:    ${max.toFixed(2)} ms`);
    console.log(`StdDev: ${stddev.toFixed(2)} ms`);
    console.log();
    const status = result === EXPECTED ? "✓" : "✗";
    console.log(`Verification: ${result} ${status}`);
}

main();

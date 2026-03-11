const N = 1000;
const RUNS = 10;

function main() {
    console.log("=== BENCHMARK: A1 - Matrix Multiplication ===");
    console.log("Language: JavaScript");
    console.log(`Runtime:  Node.js ${process.version}`);
    console.log();

    // Initialize matrices (flat Float64Array for row-major)
    const a = new Float64Array(N * N);
    const b = new Float64Array(N * N);
    for (let i = 0; i < N; i++) {
        for (let j = 0; j < N; j++) {
            a[i * N + j] = ((i * 1000 + j) % 97) * 0.01;
            b[i * N + j] = ((j * 1000 + i) % 89) * 0.01;
        }
    }

    const c = new Float64Array(N * N);
    const timings = [];

    for (let run = 0; run < RUNS; run++) {
        c.fill(0);

        const start = performance.now();
        for (let i = 0; i < N; i++) {
            for (let k = 0; k < N; k++) {
                const a_ik = a[i * N + k];
                for (let j = 0; j < N; j++) {
                    c[i * N + j] += a_ik * b[k * N + j];
                }
            }
        }
        const elapsed = performance.now() - start;
        timings.push(elapsed);
        console.log(`Run ${String(run + 1).padStart(2)}: ${elapsed.toFixed(2)} ms`);
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
    console.log();
    console.log("Verification:");
    console.log(`  C[0][0]     = ${c[0].toFixed(6)}`);
    console.log(`  C[500][500] = ${c[500 * N + 500].toFixed(6)}`);
    console.log(`  C[999][999] = ${c[999 * N + 999].toFixed(6)}`);
}

main();

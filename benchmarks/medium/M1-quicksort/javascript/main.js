const crypto = require('crypto');

function partition(arr, lo, hi) {
    const pivot = arr[hi];
    let i = lo - 1;
    for (let j = lo; j < hi; j++) {
        if (arr[j] <= pivot) {
            i++;
            const tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }
    }
    const tmp = arr[i + 1];
    arr[i + 1] = arr[hi];
    arr[hi] = tmp;
    return i + 1;
}

function quickSort(arr, lo, hi) {
    const stack = [[lo, hi]];
    while (stack.length > 0) {
        const [l, h] = stack.pop();
        if (l < h) {
            const p = partition(arr, l, h);
            stack.push([l, p - 1]);
            stack.push([p + 1, h]);
        }
    }
}

function main() {
    const N = 5_000_000;
    const RUNS = 10;
    const timings = [];

    console.log("=== BENCHMARK: M1 - Custom QuickSort ===");
    console.log("Language: JavaScript");
    console.log(`Runtime:  Node.js ${process.version}`);
    console.log();

    const arr = new Int32Array(N);
    let hash = '';

    for (let run = 1; run <= RUNS; run++) {
        // Generate array with LCG (seed=42), interpreted as int32
        let state = 42;
        for (let i = 0; i < N; i++) {
            state = (Math.imul(state, 1664525) + 1013904223) >>> 0;
            arr[i] = state | 0;
        }

        const start = process.hrtime.bigint();
        quickSort(arr, 0, N - 1);
        const elapsed = Number(process.hrtime.bigint() - start) / 1e6;

        timings.push(elapsed);
        console.log(`Run ${String(run).padStart(2)}: ${elapsed.toFixed(2)} ms`);

        if (run === RUNS) {
            const h = crypto.createHash('sha256');
            const comma = Buffer.from(',');
            for (let i = 0; i < N; i++) {
                if (i > 0) h.update(comma);
                h.update(arr[i].toString());
            }
            hash = h.digest('hex');
        }
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
    console.log(`Verification: SHA-256 = ${hash}`);
}

main();

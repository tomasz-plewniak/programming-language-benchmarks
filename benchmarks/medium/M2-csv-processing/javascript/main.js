const crypto = require('crypto');
const fs = require('fs');

const FIRST_NAMES = ["James", "Mary", "John", "Patricia", "Robert",
                     "Jennifer", "Michael", "Linda", "David", "Elizabeth"];
const LAST_NAMES = ["Smith", "Johnson", "Williams", "Brown", "Jones",
                    "Garcia", "Miller", "Davis", "Rodriguez", "Martinez"];
const DEPARTMENTS = ["Engineering", "Marketing", "Sales", "HR",
                     "Finance", "Operations", "Support", "Legal"];

const N = 1_000_000;
const RUNS = 10;
const CSV_FILE = "input.csv";
const OUTPUT_FILE = "output.csv";

function generateCsv() {
    const lines = ["id,first_name,last_name,email,department,salary"];
    let state = 42;
    for (let i = 0; i < N; i++) {
        state = (Math.imul(state, 1664525) + 1013904223) >>> 0;
        const salary = 30000.0 + (state / 4294967295.0) * 120000.0;
        const first = FIRST_NAMES[i % 10];
        const last = LAST_NAMES[i % 10];
        const dept = DEPARTMENTS[i % 8];
        lines.push(`${i + 1},${first},${last},${first.toLowerCase()}.${last.toLowerCase()}@company.com,${dept},${salary.toFixed(2)}`);
    }
    fs.writeFileSync(CSV_FILE, lines.join('\n') + '\n');
}

function processCsv() {
    const data = fs.readFileSync(CSV_FILE, 'utf8');
    const lines = data.split('\n');

    // Read, filter, group in one pass
    const groups = new Map();
    // Skip header (index 0) and empty trailing line
    for (let i = 1; i < lines.length; i++) {
        const line = lines[i];
        if (line.length === 0) continue;
        const parts = line.split(',');
        const salary = parseFloat(parts[5]);
        if (salary > 75000.0) {
            const dept = parts[4];
            let g = groups.get(dept);
            if (g === undefined) {
                g = { count: 0, total: 0, max: -Infinity, min: Infinity };
                groups.set(dept, g);
            }
            g.count++;
            g.total += salary;
            if (salary > g.max) g.max = salary;
            if (salary < g.min) g.min = salary;
        }
    }

    // Build and sort results
    const results = [];
    for (const [dept, g] of groups) {
        results.push({ dept, count: g.count, avg: g.total / g.count, max: g.max, min: g.min });
    }
    results.sort((a, b) => b.avg - a.avg);

    // Write output
    const out = ["department,count,avg_salary,max_salary,min_salary"];
    for (const r of results) {
        out.push(`${r.dept},${r.count},${r.avg.toFixed(2)},${r.max.toFixed(2)},${r.min.toFixed(2)}`);
    }
    fs.writeFileSync(OUTPUT_FILE, out.join('\n') + '\n');
}

function main() {
    const timings = [];

    console.log("=== BENCHMARK: M2 - CSV Processing ===");
    console.log("Language: JavaScript");
    console.log(`Runtime:  Node.js ${process.version}`);
    console.log();

    generateCsv();

    for (let run = 1; run <= RUNS; run++) {
        const start = process.hrtime.bigint();
        processCsv();
        const elapsed = Number(process.hrtime.bigint() - start) / 1e6;
        timings.push(elapsed);
        console.log(`Run ${String(run).padStart(2)}: ${elapsed.toFixed(2)} ms`);
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

    // Verification: SHA-256 of output.csv
    const hash = crypto.createHash('sha256').update(fs.readFileSync(OUTPUT_FILE)).digest('hex');
    console.log(`Verification: SHA-256 = ${hash}`);

    fs.unlinkSync(CSV_FILE);
    fs.unlinkSync(OUTPUT_FILE);
}

main();

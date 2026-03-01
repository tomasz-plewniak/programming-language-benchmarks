const fs = require("fs");
const path = require("path");

const DATA_DIR = path.join(__dirname, "..", "..", "..", "..", "data");
const FILE_PATH = path.join(DATA_DIR, "b2_lines.txt");
const LINES = 1_000_000;
const RUNS = 10;

function writeFile() {
  fs.mkdirSync(DATA_DIR, { recursive: true });
  const fd = fs.openSync(FILE_PATH, "w");
  for (let i = 1; i <= LINES; i++) {
    fs.writeSync(fd, `Line ${i}: The quick brown fox jumps over the lazy dog\n`);
  }
  fs.closeSync(fd);
}

function readAndCount() {
  const data = fs.readFileSync(FILE_PATH, "utf8");
  let count = 0;
  let idx = 0;
  while (idx < data.length) {
    const next = data.indexOf("\n", idx);
    if (next === -1) {
      if (idx < data.length) count++;
      break;
    }
    count++;
    idx = next + 1;
  }
  return count;
}

function main() {
  const writeTimings = [];
  const readTimings = [];

  console.log("=== BENCHMARK: B2 - File Line Counter ===");
  console.log("Language: JavaScript");
  console.log(`Runtime:  Node ${process.version}`);
  console.log();

  let count = 0;
  for (let i = 1; i <= RUNS; i++) {
    let start = process.hrtime.bigint();
    writeFile();
    const writeElapsed = Number(process.hrtime.bigint() - start) / 1e6;

    start = process.hrtime.bigint();
    count = readAndCount();
    const readElapsed = Number(process.hrtime.bigint() - start) / 1e6;

    writeTimings.push(writeElapsed);
    readTimings.push(readElapsed);
    const total = writeElapsed + readElapsed;
    console.log(
      `Run ${String(i).padStart(2)}: Write ${writeElapsed.toFixed(2)} ms | Read ${readElapsed.toFixed(2)} ms | Total ${total.toFixed(2)} ms`
    );
  }

  console.log();
  for (const [label, timings] of [["Write", writeTimings], ["Read", readTimings]]) {
    const min = Math.min(...timings);
    const avg = timings.reduce((a, b) => a + b, 0) / timings.length;
    const max = Math.max(...timings);
    const variance = timings.reduce((a, t) => a + (t - avg) ** 2, 0) / timings.length;
    const stddev = Math.sqrt(variance);
    console.log(`${label} — Min: ${min.toFixed(2)} ms | Avg: ${avg.toFixed(2)} ms | Max: ${max.toFixed(2)} ms | StdDev: ${stddev.toFixed(2)} ms`);
  }

  const totals = writeTimings.map((w, i) => w + readTimings[i]);
  const min = Math.min(...totals);
  const avg = totals.reduce((a, b) => a + b, 0) / totals.length;
  const max = Math.max(...totals);
  const variance = totals.reduce((a, t) => a + (t - avg) ** 2, 0) / totals.length;
  const stddev = Math.sqrt(variance);
  console.log(`Total — Min: ${min.toFixed(2)} ms | Avg: ${avg.toFixed(2)} ms | Max: ${max.toFixed(2)} ms | StdDev: ${stddev.toFixed(2)} ms`);

  console.log();
  const status = count === LINES ? "✓" : "✗";
  console.log(`Verification: ${count} ${status}`);

  fs.unlinkSync(FILE_PATH);
}

main();

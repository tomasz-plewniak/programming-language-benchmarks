function fib(n) {
  if (n <= 1) return n;
  return fib(n - 1) + fib(n - 2);
}

function main() {
  const N = 40;
  const RUNS = 10;
  const timings = [];

  console.log("=== BENCHMARK: B1 - Fibonacci ===");
  console.log("Language: JavaScript");
  console.log(`Runtime:  Node ${process.version}`);
  console.log();

  for (let i = 1; i <= RUNS; i++) {
    const start = process.hrtime.bigint();
    const result = fib(N);
    const elapsed = Number(process.hrtime.bigint() - start) / 1e6; // ms
    timings.push({ elapsed, result });
    console.log(`Run ${String(i).padStart(2)}: ${elapsed.toFixed(2)} ms`);
  }

  console.log();
  const times = timings.map((t) => t.elapsed);
  const min = Math.min(...times);
  const avg = times.reduce((a, b) => a + b, 0) / times.length;
  const max = Math.max(...times);
  const variance = times.reduce((a, t) => a + (t - avg) ** 2, 0) / times.length;
  const stddev = Math.sqrt(variance);

  console.log(`Min:    ${min.toFixed(2)} ms`);
  console.log(`Avg:    ${avg.toFixed(2)} ms`);
  console.log(`Max:    ${max.toFixed(2)} ms`);
  console.log(`StdDev: ${stddev.toFixed(2)} ms`);
  console.log();
  const result = timings[timings.length - 1].result;
  const status = result === 102334155 ? "✓" : "✗";
  console.log(`Verification: ${result} ${status}`);
}

main();

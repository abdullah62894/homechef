// Simple load generator for the HomeChef API (Stage 12).
// Usage: node loadtest.mjs <baseUrl> <path> <concurrency> <durationSeconds> [--warmup]
import http from "node:http";

const [, , base, path, concArg, durArg, warmupArg] = process.argv;
const concurrency = Number(concArg ?? 16);
const durationMs = Number(durArg ?? 10) * 1000;
const url = new URL(path, base);

function request() {
  return new Promise((resolve) => {
    const start = process.hrtime.bigint();
    const req = http.get(url, { agent: false }, (res) => {
      res.resume();
      res.on("end", () =>
        resolve({ ok: res.statusCode === 200, ms: Number(process.hrtime.bigint() - start) / 1e6 })
      );
    });
    req.on("error", () => resolve({ ok: false, ms: 0 }));
  });
}

function percentile(sorted, p) {
  if (sorted.length === 0) return 0;
  const idx = Math.min(sorted.length - 1, Math.ceil((p / 100) * sorted.length) - 1);
  return sorted[Math.max(0, idx)];
}

async function run(label) {
  const latencies = [];
  let failures = 0;
  let requests = 0;
  const deadline = Date.now() + durationMs;

  await Promise.all(
    Array.from({ length: concurrency }, async () => {
      while (Date.now() < deadline) {
        const r = await request();
        requests++;
        if (!r.ok) failures++;
        else latencies.push(r.ms);
      }
    })
  );

  latencies.sort((a, b) => a - b);
  const total = Number(process.hrtime.bigint()); // not used; wall time below
  const rps = requests / (durationMs / 1000);
  console.log(
    JSON.stringify({
      endpoint: path,
      mode: label,
      rps: Math.round(rps),
      p50: +percentile(latencies, 50).toFixed(1),
      p95: +percentile(latencies, 95).toFixed(1),
      p99: +percentile(latencies, 99).toFixed(1),
      failures,
    })
  );
}

// Cold run first (cache empty), then warm (cache populated).
if (warmupArg !== "--warm") await request(); // one cold hit populates nothing extra; keep both modes comparable
await run("cold");
await request(); // prime the in-memory cache
await run("warm");

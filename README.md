# Hacker News — Best N Stories API (ASP.NET Core)

This repository contains a minimal ASP.NET Core implementation that exposes a REST endpoint to return the best `n` stories from Hacker News, by **score**.

> **Endpoint:** `GET /api/beststories?n={n}`

---

## Design & assumptions

* We call the Hacker News API endpoints documented at [https://github.com/HackerNews/API](https://github.com/HackerNews/API).
* The Hacker News endpoint `/v0/beststories.json` returns a list of story IDs. Those IDs are not guaranteed to be ordered by `score`. Therefore the API:

  * *scans the first `MaxScan` story IDs* from `beststories.json`, requests their details in parallel (bounded concurrency), then picks the top `n` by `score`.
  * `MaxScan` is configurable (defaults to 500). This is a trade-off between accuracy (scanning more IDs) and load on HN.
* To avoid overloading Hacker News we use:

  * `IHttpClientFactory` to reuse connections.
  * An in-memory `IMemoryCache` with short TTLs for the list of IDs and longer TTL for individual story details.
  * Bounded parallelism (SemaphoreSlim / `Parallel.ForEachAsync`) when fetching story details.
  * Optional rate limiting / backoff could be added with Polly (not included but described in README).
* We cap caller `n` to a reasonable maximum (e.g. 100). Validation returns `400 Bad Request` for invalid values.

---

## How to run

Prerequisites: .NET 7 (or .NET 8), `dotnet` cli.

1. `dotnet run --project src/BestStoriesApi`
2. Open `http://localhost:5000/api/beststories?n=10`

You can also change configuration in `appsettings.json` (MaxScan, cache durations, parallelism).

---

## Possible enhancements

* Use Polly for retries, exponential backoff and bulkhead isolation.
* Add a background refresh job to keep the top stories cached and serve instantly.
* Replace `IMemoryCache` with `IDistributedCache` (Redis) for multi-instance deployments.
* Expose observability: metrics (Prometheus), traces, and logs to observe rate and failures.
* Add e2e and unit tests for `StoriesService` (mock `HackerNewsClient`).

---

## Notes

* The code above intentionally keeps configuration inline for clarity. For production, move tunables to `appsettings.json` and `IOptions<T>`.
* We intentionally limit the scan to `MaxScan` IDs to reduce requests to Hacker News. If correctness needs to be perfect, the service could gradually scan more IDs or maintain a background index built from all `beststories`.

---

# Hacker News — Best Stories API (ASP.NET Core)

This repository contains a minimal ASP.NET Core implementation that exposes a REST endpoint to return the best `n` stories from Hacker News, by **score**.

> **Endpoint:** `GET /api/stories/best?n={n}`

---

## Design & assumptions

* We call the Hacker News API endpoints documented at [https://github.com/HackerNews/API](https://github.com/HackerNews/API).
* The Hacker News endpoint `/v0/beststories.json` returns a list of story IDs. Those IDs are not guaranteed to be ordered by `score`. Therefore the API:

  * *scans the first `MaxScan` story IDs* from `beststories.json`, requests their details in parallel (bounded concurrency), then picks the top `n` by `score`.
  * `MaxScan` is configurable (defaults to 500). This is a trade-off between accuracy (scanning more IDs) and load on HackerNews.
* To avoid overloading Hacker News we use:

  * `IHttpClientFactory` to reuse connections.
  * An in-memory `IMemoryCache` with a short TTL for the list of IDs and a longer TTL for individual story details.
  * Bounded parallelism (using a SemaphoreSlim) when fetching story details.
* We cap caller `n` to a reasonable maximum (100). Validation returns `400 Bad Request` for invalid values.

---

## How to run

Prerequisites: .NET 8, `dotnet` cli.

1. `dotnet run --project src/BestStoriesApi`
2. Open `http://localhost:5000/api/beststories?n=10`

You can also change configuration in `appsettings.json` (MaxScan, cache durations, parallelism).

---

## Possible enhancements

* Use Polly for retries
  * Optional rate limiting / backoff could be added
* Add a background refresh job to keep the top stories cached and serve instantly.
* Replace `IMemoryCache` with `IDistributedCache` (Redis) for multi-instance deployments.
* Expose observability: metrics, traces, and logs to observe rate and failures.
* Add e2e for `StoriesService` (mock `HackerNewsClient`).

---

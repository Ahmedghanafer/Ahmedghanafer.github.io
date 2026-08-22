namespace Portfolio.Data;

/// <summary>
/// Facts for the Albion Profit Forge case study. Every number here is measured, not estimated.
/// Where the product's own marketing copy disagreed with the code, the code won, including
/// where that made the number less flattering.
/// </summary>
public static class ApfCaseStudy
{
    /// <summary>The end-to-end freshness chain, rendered by <c>ArchitectureFlow</c>.</summary>
    public static readonly IReadOnlyList<FlowNode> Pipeline = new[]
    {
        new FlowNode(
            Label: "AODP",
            Detail: "The Albion Online Data Project: a community market-data source. Three ingest paths feed off it: "
                  + "periodic REST sweeps, a daily full SQL dump, and a live event stream. A full EU sweep is 82 seconds "
                  + "and 153,871 rows.",
            Sub: "upstream",
            Icon: "cloud"),

        new FlowNode(
            Label: "NATS ingest",
            Detail: "A server-side subscriber folds live price events into the database in roughly one-second batches. "
                  + "This is where NATS stops. It is ingest transport only; it never reaches the browser, and nothing "
                  + "downstream is push-based.",
            Sub: "live stream",
            Icon: "bolt"),

        new FlowNode(
            Label: "PostgreSQL 16",
            Detail: "psycopg3 with a module-level connection pool, raw SQL, no ORM. There is no Redis and no Memcached: "
                  + "the price table is the cache, supplemented by in-process TTL dictionaries and HTTP validators. "
                  + "The whole database is ~825 MB, of which the results table is 487 MB across five retained builds.",
            Sub: "raw SQL, no ORM",
            Icon: "database"),

        new FlowNode(
            Label: "Precompute daemon",
            Detail: "An in-process Python daemon thread on a fixed-rate 300-second timer. No cron, no Celery, no external "
                  + "queue. Fixed-rate rather than fixed-delay, so a slow pass doesn't stretch the advertised cadence. The "
                  + "first pass is deliberately delayed by a full interval, because boot is already contending with the catalogue "
                  + "load and the NATS connect, and a cold run there would compete with the first users. Regions run "
                  + "serially on purpose so the box only ever holds one pass's CPU and memory peak.",
            Sub: "every ~5 min",
            Icon: "cog",
            Highlight: true),

        new FlowNode(
            Label: "Versioned results",
            Detail: "Rows are written under a run_id while the pass walks, and readers never see them, because every read "
                  + "path resolves the build through is_current. On success, one transaction demotes the old build, "
                  + "promotes the new one and prunes what it superseded. The invariant is written into the docstring: "
                  + "there is never a moment with zero current builds (readers would see an empty list) or two "
                  + "(paging would interleave). On failure the run closes unpromoted and its partial rows are deleted: "
                  + "a broken pass degrades to stale data, never to no data.",
            Sub: "atomic swap",
            Icon: "layers",
            Highlight: true),

        new FlowNode(
            Label: "Polling API",
            Detail: "ORDER BY rank_score DESC with explicit tiebreakers, because rank_score alone has ties, and an unstable sort "
                  + "would drop or duplicate rows across page boundaries. Columnar payload, strong ETag keyed on "
                  + "(build, mode, page, tier), 2,000 rows per request. No calculation happens in the request path at all.",
            Sub: "read path",
            Icon: "server"),

        new FlowNode(
            Label: "Browser",
            Detail: "A 60-second poll of a deliberately tiny one-row probe, comparing the envelope's computed_at against "
                  + "the last build it saw. A changed timestamp is the swap signal. This is HTTP polling; there is no "
                  + "EventSource and no WebSocket anywhere in the codebase. The poll suspends while the tab is hidden, and "
                  + "a detected swap is deferred while a detail modal is open, because a comparison mid-read losing its row "
                  + "is worse than sixty stale seconds.",
            Sub: "60 s poll",
            Icon: "monitor"),
    };

    public static readonly IReadOnlyList<EngineeringStory> Stories = new[]
    {
        new EngineeringStory(
            Kicker: "Payload",
            Title: "The field names were the payload",
            Problem:
                "A full page load returned 100k+ price rows as a list of dicts, and every row repeated all fourteen field "
                + "names. In a measured 129 MB response the largest single share was not the data but the keys, "
                + "written out a hundred thousand times.",
            Fix:
                "Move the wire format to columnar: column names once, then rows as bare tuples. The client keeps a branch "
                + "for the old shape, so it ships as a compatible change rather than a flag day. Two related fixes landed "
                + "the same day. Bulk loads became cache-only, because the daily dump is already a full snapshot and "
                + "re-asking a rate-limited API for keys it never had cost 5+ minutes of sweep for no data; and the first "
                + "load stopped pulling all five quality tiers, since a full-catalogue pull was ~480k keys. Then "
                + "encode zstd gzip at the edge, which had simply never been switched on.",
            Before: "129 MB",
            After: "13.9 MB",
            DeltaNote: "on the wire before compression, which takes another 10–20× off",
            Lesson: "Serialization shape is a performance decision. Nobody profiles the key names."),

        new EngineeringStory(
            Kicker: "Caching",
            Title: "The 304 that only ever fired on localhost",
            Problem:
                "The ~7 MB item catalogue only changes on a recompile, but every page load re-downloaded and re-parsed it. "
                + "A strong ETag plus If-None-Match fixed that, verified returning 304. The same trick then went onto the "
                + "results endpoint and, in production, never fired once; every conditional request came back 200 with a "
                + "full body.",
            Fix:
                "Caddy's encode rewrites the outgoing ETag to a weak -gzip-suffixed tag. That is correct per RFC: the "
                + "compressed representation genuinely is a different representation. But it forwards the client's "
                + "If-None-Match untouched, so browsers echoed back the suffixed tag and the origin's plain string "
                + "comparison missed every single time. The fix strips a trailing -gzip / -zstd / -br / -deflate before "
                + "comparing.",
            Before: "200, always",
            After: "304, finally",
            DeltaNote: "the origin had been right all along; the proxy was the untested half",
            Lesson: "An optimisation verified at the origin is not verified. Test it through the stack that serves it."),

        new EngineeringStory(
            Kicker: "Memory",
            Title: "5 GB resident, held for a loop that reads one bucket",
            Problem:
                "After a dump import the history table held 742 MB of hourly JSON blobs across 54k keys, 13.6 KB each on "
                + "average, 3.1 GB on disk with rewrite bloat. The only consumer of that data reads the last bucket of "
                + "each blob. A single full-catalogue stats recompute parsed roughly a gigabyte of JSON into memory at "
                + "once, and the run log showed the process peaking at 4.6–5.1 GB.",
            Fix:
                "Retention caps applied at every merge instead of at read time, a one-off truncation at import end, and "
                + "the stats path re-plumbed to iterate 200 items at a time so peak memory is one chunk regardless of "
                + "catalogue size. The same diagnosis explained a second symptom nobody had connected to it: the "
                + "multi-minute history flushes during import, where every flush was read-modify-writing an ever-growing "
                + "year-long blob.",
            Before: "5.4 GB",
            After: "1.0 GB",
            DeltaNote: "after VACUUM, locally and on the server",
            Lesson: "Store what the reader reads. Retention is a write-path concern, not a query filter."),

        new EngineeringStory(
            Kicker: "Query load",
            Title: "1,750 round trips for one page",
            Problem:
                "The bulk price read chunked its keys into IN (VALUES …) batches of 200 and issued one statement per "
                + "batch. A single page load measured ~1,750 pool checkouts and queries, minutes of pure round-trip "
                + "latency against data that was already sitting in the cache.",
            Fix:
                "Decompose the keys into their distinct dimension values and issue exactly one query per region with three "
                + "array parameters, then filter the returned grid superset down in Python. The database does one scan and "
                + "the round trips go to one.",
            Before: "~1,750 queries",
            After: "1 per region",
            DeltaNote: "same data, same correctness",
            Lesson: "Round-trip count is its own axis. A fast query run 1,750 times is a slow page."),

        new EngineeringStory(
            Kicker: "Upstream",
            Title: "A cache that couldn't cache absence",
            Problem:
                "Bulk inserts deliberately dropped rows with no real price, so they could never clobber good dump data. "
                + "The side effect went unnoticed for a long time: items with no listings anywhere were never written to "
                + "the cache at all, so every page load re-classified them as missing and re-fetched them from a "
                + "rate-limited upstream. One user reloading meant a full re-sweep; N concurrent users meant N full "
                + "sweeps: a self-inflicted denial of service against somebody else's API, and the reason a plain reload "
                + "could take minutes.",
            Fix:
                "Write an explicit api-nodata marker row so \"nothing here\" becomes a cacheable fact, with ON CONFLICT DO "
                + "NOTHING so a marker can never overwrite real data. The guard condition matters more than the marker: "
                + "markers are written only when the sweep had zero fetch errors. With errors present you cannot "
                + "distinguish \"no data exists\" from \"couldn't ask\", and a transient upstream outage must never be "
                + "cached as a permanent absence.",
            Before: "N users → N sweeps",
            After: "absence cached once",
            DeltaNote: "minutes off a cold reload",
            Lesson: "A negative result is a result. Caching only successes turns a cache into an amplifier."),

        new EngineeringStory(
            Kicker: "Correctness",
            Title: "Row counts moving while the data stood still",
            Problem:
                "Nothing was reported broken. But between two builds the precomputed Black Market rows had drifted from "
                + "1,202 to 351 purely on ranking competition, while the underlying data was verifiably unchanged at "
                + "28,883 rows. Filtering to a single sell city reached only 9–20% of craftable items; the Black Market, "
                + "5.4% (336 of 6,194). The other 94% existed in the price table and were simply never stored.",
            Fix:
                "Add per-item coverage rows so every item stays reachable whether or not it wins the global ranking, and "
                + "stream them in 1,000-row batches instead of accumulating them to the end, which removed the last "
                + "structure in the pass that grew with catalogue size. Coverage rows and top-N rows legitimately collide, "
                + "so the coverage flag is AND-ed on conflict, making the write order-independent and idempotent under retry.",
            Before: "5.4% reachable",
            After: "91.0% reachable",
            DeltaNote: "3.3× the rows, 2× the time, 35% less peak memory",
            Lesson: "Row counts moving without data moving is the signal. Watch invariants, not just errors."),
    };

    /// <summary>The precompute runner's own before/after arc, oldest first.</summary>
    public static readonly IReadOnlyList<(string Stage, string Items, string Combinations, string Rows, string Time, string Rss)> PrecomputeArc = new[]
    {
        ("Craft mode only", "7,115", "653,731", "7,809", "36.7 s", "136 MB"),
        ("All modes", "n/a", "n/a", "40,054", "32.3 s", "175 MB"),
        ("Coverage-key fix", "8,529", "1,316,707", "133,309", "68.1 s", "114 MB"),
        ("Current build", "8,438", "1,927,388", "149,180", "32.3 s", "137 MB"),
    };

    /// <summary>Calibration points the code's own comments cite for the freshness curve.</summary>
    public static readonly IReadOnlyList<(string Condition, double Factor)> FreshnessPoints = new[]
    {
        ("Fresh on both legs", 1.00),
        ("Sell price 24 h old", 0.69),
        ("Sell 24 h + materials 48 h", 0.58),
        ("Sell price 5 days old", 0.27),
        ("The floor, however stale", 0.20),
    };

    /// <summary>Trade-offs the project made deliberately, and says so out loud in its own source.</summary>
    public static readonly IReadOnlyList<FactRow> Tradeoffs = new[]
    {
        new FactRow(
            "One gunicorn worker, sixteen threads",
            "The NATS subscriber threads, the dump-import job state and the rate-limiter windows are all per-process. "
            + "N workers would run N subscribers and give each user a different view of import status. Concurrency comes "
            + "from threads instead. It is written into the config as a stated trade-off with an exit condition: workers "
            + "can scale the moment the subscriber moves into its own service."),

        new FactRow(
            "Not everything is precomputed",
            "Enchant Mode, Quality Mode and Detailed Shopping change the shape of the calculation, not just its inputs, "
            + "so the client refuses the precomputed path and falls back to the live calculator. Melding and Transmuting "
            + "are excluded because they were measured fast enough to render live, at 80 and 95 combinations."),

        new FactRow(
            "Craft city is not a stored dimension",
            "It only feeds the return rate, and the browser already holds the city-bonus table. Storing it would multiply "
            + "the row count sevenfold for no new information. Both sides of every user toggle are stored instead, so the "
            + "toggles stay exact rather than pinning users to one basis."),

        new FactRow(
            "No current build returns 200, never 404",
            "The response carries available: false and the front end falls back to the live calculator. A missing build is "
            + "a normal state, not an error, which is the same instinct as the failure path: degrade to stale, never to nothing."),
    };
}

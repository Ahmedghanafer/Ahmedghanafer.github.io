namespace Portfolio.Data;

/// <summary>
/// Prose and supporting figures for the Albion Profit Forge case study.
/// Split from <see cref="ApfCaseStudy"/> so the measured-facts tables and the
/// written narrative can be edited independently.
/// </summary>
public static class ApfNarrative
{
    public const string Kind = "Case study · market analytics platform";

    public const string Title = "Albion Profit Forge";

    public const string Standfirst =
        "A live market-analysis platform for a game economy with a few thousand items and three regional servers. "
        + "Prices arrive over NATS, land in PostgreSQL, and a background daemon evaluates every viable trade route "
        + "before anybody asks for one. I own all of it: schema, ingest, ranking model, front end, and the VPS it "
        + "runs on.";

    public const string Problem =
        "Working out what is worth crafting means comparing material costs in one city against sell listings in "
        + "another, at several quality tiers, for thousands of items, against prices that are constantly going "
        + "stale. Computing that per request is hopeless. So the request path does none of it.";

    public const string Thesis =
        "The whole architecture is one decision, applied repeatedly: move the work out of the request and put a "
        + "version number on the result. A daemon evaluates ~1.9 million combinations per pass, keeps the ~150 "
        + "thousand that survive ranking, writes them under a build id, and flips one boolean in a single "
        + "transaction. A page view is ORDER BY rank_score DESC with a LIMIT and an OFFSET. Nothing is calculated "
        + "there.";

    public const string SplitIdea =
        "The precompute stores only the market-derived half of each row: material cost, sell listing, craft yield, "
        + "volume, freshness. The per-user half (return rate, sales tax, station fee, the user's filters) stays in "
        + "the browser and is applied over the stored rows. That boundary is what lets one shared result set of "
        + "~150k rows serve every user with their own settings intact. Both sides of each user toggle are stored, so "
        + "the toggles stay exact instead of pinning everyone to one basis.";

    public const string ClaimsNote =
        "The product's own landing page says “~1,000,000 combinations”. That number is combinations evaluated per "
        + "pass, not results stored, and it is understated: the live figure is 1,927,388. Rows actually stored and "
        + "served for the current EU build is 149,180. The honest phrasing is ~1.9M evaluated every pass, ~150k "
        + "served, and that is the phrasing this page uses.";

    public const string PollingNote =
        "NATS is in the stack, so people assume there is a socket to the browser. There is not. A search for "
        + "EventSource, WebSocket and text/event-stream across the whole front end returns zero hits; every hit is "
        + "setInterval. Live updates are a 60-second poll of a deliberately tiny probe: one row, compared against "
        + "the last build the client saw.";

    public const string PollingDetail =
        "The probe asks for a single row and reads one field from the envelope. A changed computed_at is the swap "
        + "signal. The poll suspends while the tab is hidden and re-fires on visibilitychange. On a new build the "
        + "client invalidates every mode's cached rows plus the material price slice, re-fetches, re-renders. And "
        + "the swap is deferred while a detail modal is open: losing a row mid-comparison is worse than sixty stale "
        + "seconds.";

    public const string SwapNote =
        "The invariant the promote transaction protects, written in the code that enforces it: there is never a "
        + "moment where zero runs are current (readers would see an empty list) or two are, which would interleave "
        + "two builds across page boundaries.";

    public const string ConfidenceIntro =
        "Every row carries a price that was true at some point. Ranking has to care how long ago that was, or a "
        + "five-day-old phantom listing outranks a real one. The same decay curve drives both the rank score and the "
        + "user-facing confidence badge, deliberately the same, so confidence and ranking can never disagree about "
        + "what “fresh” means.";

    public const string ConfidenceHalfWeight =
        "The sell side decays twice as fast because it is the risky leg: a stale sell price means capital committed "
        + "against a maybe-phantom listing, while a stale material cost is at worst a missed buy. Half weight is "
        + "implemented as double the time constant (half the decay rate in the exponent), not a 0.5 multiplier on "
        + "the output.";

    public const string ConfidenceMultiplicative =
        "It is the product of two per-leg curves, not max() of the legs. The first version shipped max(sellAge, "
        + "matAge/2) and was replaced twenty-two minutes later, because a row with both a 24-hour sell price and "
        + "48-hour materials lands at 0.494, below either leg alone at 0.685, which a max()-of-legs shape "
        + "simply cannot express. Both comments in the source quoted 0.58 for that case until I checked it "
        + "against the running code; 0.58 is what a material time constant of 4×τ would give, not the 2×τ the "
        + "constants actually use. Nothing asserted it, so the prose drifted. There is a test now.";

    public const string ConfidenceFloor =
        "The 0.2 floor is a product decision, not a numerical one: stale rows sink but never die, because a monster "
        + "margin on a thin market can still be worth surfacing. Unknown ages decay nothing: a genuinely old row "
        + "always carries a date, so there is no escape hatch through missing data.";

    public const string ConfidenceHonesty =
        "Two things I will not dress up. The 48-hour time constant arrives fully formed in the commit that "
        + "introduced it, justified by its output points rather than fitted to data. And the 50/50 age-versus-volume "
        + "split in the confidence badge predates the current history entirely, an un-derived product judgement. "
        + "What is derived and documented is the shape of the age half, and the constraint that it must be the same "
        + "curve the ranker uses.";

    public const string Closing =
        "The parts I would show a reviewer are not the throughput numbers. They are the two bugs whose failure mode "
        + "was silence: a retention prune that matched nothing for weeks, and a caching optimisation that was "
        + "correct at the origin and inert behind the proxy. And the habit that caught the third one: watching row "
        + "counts move while the data stood still.";

    /// <summary>Current build, queried live from the database rather than estimated.</summary>
    public static readonly IReadOnlyList<FactRow> RunFacts = new[]
    {
        new FactRow("Items scanned", "8,438", null),
        new FactRow("Combinations evaluated", "1,927,388", "per pass"),
        new FactRow("Rows stored & served", "149,180", "this build"),
        new FactRow("Pass duration", "32.3 s", null),
        new FactRow("Peak resident memory", "137 MB", null),
        new FactRow("Results table on disk", "487 MB", "5 builds · 745,884 rows"),
    };

    /// <summary>Six consecutive fixed-rate passes on one box: the warm-up curve.</summary>
    public static readonly IReadOnlyList<(string Label, double Seconds, string? Rss)> WarmupRuns = new[]
    {
        ("pass 1", 118.0, "281 MB"),
        ("pass 2", 64.8, (string?)null),
        ("pass 3", 53.6, (string?)null),
        ("pass 4", 36.1, (string?)null),
        ("pass 5", 34.9, (string?)null),
        ("pass 6", 32.3, "137 MB"),
    };

    /// <summary>Liquidity brackets for the volume half of the confidence score.</summary>
    public static readonly IReadOnlyList<(int MinVolume, int Points)> VolumeBrackets = new[]
    {
        (100, 50),
        (30, 40),
        (10, 25),
        (1, 10),
    };
}

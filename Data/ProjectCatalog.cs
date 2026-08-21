namespace Portfolio.Data;

/// <summary>
/// The project index. Adding a case study is two steps: append a
/// <see cref="ProjectSummary"/> here, and add a routed page under <c>Pages/Projects/</c>
/// whose <c>@page</c> matches its <see cref="ProjectSummary.CaseStudyHref"/>.
/// Each case study owns its own layout, so a Python/Postgres write-up and a .NET
/// write-up don't have to pretend to be the same shape.
/// </summary>
public static class ProjectCatalog
{
    public const string ApfSlug = "albion-profit-forge";

    public static readonly IReadOnlyList<ProjectSummary> All = new[]
    {
        new ProjectSummary(
            Slug: ApfSlug,
            Title: "Albion Profit Forge",
            Kind: "Market analytics platform",
            Tagline: "~1.9M trade combinations evaluated every pass. ~150k survivors served from a table that does no maths in the request path.",
            Description:
                "A live market-analysis platform for Albion Online. Prices arrive over NATS, land in PostgreSQL, and a "
                + "five-minute daemon walks the whole catalogue to precompute every viable craft, flip and salvage route. "
                + "The web request does no calculation at all — it pages a versioned results table in rank order. "
                + "I own the whole thing: schema, ingest, ranking model, front end, VPS.",
            Stack: new[] { "Python 3.12", "Flask 3.1", "PostgreSQL 16", "psycopg3 · raw SQL", "NATS", "Vanilla JS", "Caddy", "Cloudflare" },
            Metrics: new[]
            {
                new Metric("1,927,388", "combinations evaluated", "per precompute pass"),
                new Metric("149,180", "rows stored & served", "current EU build"),
                new Metric("32.3 s", "full precompute pass", "137 MB peak RSS"),
                new Metric("129 → 13.9 MB", "price payload", "before edge compression"),
            },
            CaseStudyHref: "/projects/" + ApfSlug,
            ExternalHref: null,
            Status: "Live · operated by me",
            IsCentrepiece: true),

        new ProjectSummary(
            Slug: "console-certification-api",
            Title: "Console Certification API",
            Kind: "SaaS API · .NET 8",
            Tagline: "Automating the console certification process for game developers.",
            Description:
                "A .NET 8 SaaS API on a Clean Architecture split, designed around high availability and secure handling "
                + "of third-party integration data. Pre-MVP — the case study goes up when there is something honest to measure.",
            Stack: new[] { ".NET 8", "C#", "Entity Framework", "Clean Architecture", "DDD", "REST API" },
            Metrics: Array.Empty<Metric>(),
            CaseStudyHref: null,
            ExternalHref: null,
            Status: "In development",
            IsCentrepiece: false),

        new ProjectSummary(
            Slug: "this-site",
            Title: "This site",
            Kind: "Blazor WebAssembly",
            Tagline: "C# in the browser. No React, no Vue, no bundler — the .NET runtime and a component tree.",
            Description:
                "A standalone Blazor WebAssembly app compiled to static files. Routing, state, theming and every diagram "
                + "on the case-study page are C# components. The only JavaScript is a small interop module for the three "
                + "things the DOM genuinely owns: localStorage, IntersectionObserver and scroll position.",
            Stack: new[] { "Blazor WebAssembly", ".NET 8", "C#", "CSS custom properties", "GitHub Actions" },
            Metrics: Array.Empty<Metric>(),
            CaseStudyHref: null,
            ExternalHref: "https://github.com/Ahmedghanafer",
            Status: "You're looking at it",
            IsCentrepiece: false),
    };

    public static ProjectSummary Centrepiece => All.First(p => p.IsCentrepiece);

    public static IEnumerable<ProjectSummary> Supporting => All.Where(p => !p.IsCentrepiece);

    public static ProjectSummary? BySlug(string slug) => All.FirstOrDefault(p => p.Slug == slug);
}

namespace Portfolio.Data;

/// <summary>
/// Single source of truth for CV-derived content. Kept as plain data so the
/// rendering components stay dumb and reusable.
/// </summary>
public static class SiteData
{
    public static readonly Profile Me = new(
        Name: "Ahmed Ghanafer",
        Role: "Fullstack .NET Developer",
        Location: "Stockholm, Sweden",
        Pitch: "I build the unglamorous half of software well: the data layer that stays correct under load, "
             + "the API that stops re-sending 129 MB, the background job that can never hand out a half-finished result.",
        Summary: "Seven years across CRM platforms, automation tooling and product engineering, mostly in .NET. "
               + "Since 2025 I've been running my own product work, which is where I learned the most: when you own "
               + "the VPS, the schema and the front end, every shortcut comes back with interest.",
        Links: new[]
        {
            new ContactLink("Email", "ahmed.ghanafer@gmail.com", "mailto:ahmed.ghanafer@gmail.com", "mail"),
            new ContactLink("GitHub", "github.com/Ahmedghanafer", "https://github.com/Ahmedghanafer", "github"),
            new ContactLink("LinkedIn", "ahmed-ghanafer", "https://linkedin.com/in/ahmed-ghanafer-374804123", "linkedin"),
        });

    public static readonly IReadOnlyList<Role> Experience = new[]
    {
        new Role(
            Title: "Founder & Lead Developer",
            Company: "Self-employed",
            Period: "Feb 2025 – Present",
            Location: "Stockholm",
            Blurb: "Building and operating my own products end to end: architecture, database, deployment, and the "
                 + "on-call pager that is also me.",
            Highlights: new[]
            {
                "Architected and developed a SaaS API that automates the console certification process for game developers.",
                "Built the backend on .NET 8, Entity Framework and a Clean Architecture split, with high availability and secure third-party data handling as the two hard requirements.",
                "Ran the market and technical research that decided what was worth building, and what wasn't.",
                "Kept a product roadmap that balances technical debt against feature priority on the way to an MVP.",
                "Separately: designed, shipped and now operate Albion Profit Forge, a Python/PostgreSQL analytics platform on a self-managed VPS.",
            },
            Stack: new[] { ".NET 8", "Entity Framework", "Clean Architecture", "DDD", "REST API", "PostgreSQL", "Python", "Linux VPS" }),

        new Role(
            Title: "Fullstack Consultant",
            Company: "SweetSystems",
            Period: "Aug 2021 – Feb 2025",
            Location: "Stockholm",
            Blurb: "Three and a half years leading development of custom CRM platforms and automation tools, each one "
                 + "shaped around a different client's actual process rather than a template.",
            Highlights: new[]
            {
                "Designed and implemented CRM systems and automation products tailored to client requirements.",
                "Owned integration work into existing customer systems, plus the technical support that follows a live integration.",
                "Worked in Agile sprints and built the scalable pieces of core systems the rest of the platform leaned on.",
            },
            Stack: new[] { "C#", ".NET Core", "ASP.NET MVC 5", "Entity Framework", "SQL Server", "TypeScript", "Angular", "Azure DevOps", "Scrum" }),

        new Role(
            Title: "Fullstack Web Developer",
            Company: "Keystone Media Group",
            Period: "Sep 2018 – Aug 2021",
            Location: "Stockholm",
            Blurb: "First professional role. Shipped features into a live product on a small team, which meant learning "
                 + "the whole stack rather than a slice of it.",
            Highlights: new[]
            {
                "Developed and deployed features across .NET/C#, Angular, Knockout, JavaScript, SQL and AWS cloud services.",
                "Researched and introduced the technologies each new feature needed, rather than forcing them into the existing toolset.",
                "Worked requirements directly with stakeholders (scope, constraints and what was actually possible) before committing to an approach.",
            },
            Stack: new[] { "C#", ".NET", "Angular", "Knockout", "JavaScript", "SQL", "AWS", "Git" }),
    };

    public static readonly IReadOnlyList<SkillGroup> Skills = new[]
    {
        new SkillGroup("Backend", "Where I'm strongest.",
            new[] { "C#", ".NET 8", "ASP.NET MVC 5", "Entity Framework", "Clean Architecture", "DDD", "REST API", "gRPC", "TDD", "Python", "Flask" }),
        new SkillGroup("Data", "Schema first, ORM second.",
            new[] { "SQL Server", "PostgreSQL 16", "Raw SQL", "Query tuning", "Indexing", "Migrations", "Caching strategy", "NATS" }),
        new SkillGroup("Frontend", "Including the framework-free kind.",
            new[] { "Blazor WebAssembly", "TypeScript", "JavaScript", "Angular", "Knockout", "HTML & CSS", "Accessibility", "UI/UX design" }),
        new SkillGroup("Platform", "Ship it, then keep it up.",
            new[] { "Docker", "Azure DevOps", "AWS", "Linux VPS", "Caddy", "Cloudflare", "GitHub Actions", "Git", "Scrum" }),
    };

    public static readonly IReadOnlyList<LanguageSkill> Languages = new[]
    {
        new LanguageSkill("English", "Native / bilingual", 100),
        new LanguageSkill("Arabic", "Native / bilingual", 100),
        new LanguageSkill("Swedish", "Professional working", 75),
    };

    /// <summary>Named <c>Schooling</c> rather than <c>Education</c> so the field never shadows the type.</summary>
    public static readonly IReadOnlyList<Education> Schooling = new[]
    {
        new Education("ASP.NET MVC 5", "Lexicon Yrkeshögskola", "2018", null),
        new Education("SVA Grund (del 2)", "Eductus", "2017–2018", "Nacka"),
        new Education("Mathematics", "Tishreen University", "2014–2015", "Latakia, Syria"),
    };

    /// <summary>Rendered in the hero's runtime card: the "built in Blazor WebAssembly" signal.</summary>
    public static readonly IReadOnlyList<FactRow> RuntimeFacts = new[]
    {
        new FactRow("runtime", "Blazor WebAssembly"),
        new FactRow("framework", ".NET 8"),
        new FactRow("language", "C#, including this page"),
        new FactRow("js framework", "none"),
        new FactRow("build step", "dotnet publish"),
    };
}

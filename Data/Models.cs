namespace Portfolio.Data;

/* ---------------------------------------------------------------------
   Profile / CV
   --------------------------------------------------------------------- */

/// <summary>Identity and contact surface, rendered by the hero and the contact section.</summary>
public sealed record Profile(
    string Name,
    string Role,
    string Location,
    string Pitch,
    string Summary,
    IReadOnlyList<ContactLink> Links);

public sealed record ContactLink(string Label, string Value, string Href, string Kind);

public sealed record Role(
    string Title,
    string Company,
    string Period,
    string? Location,
    string Blurb,
    IReadOnlyList<string> Highlights,
    IReadOnlyList<string> Stack);

public sealed record SkillGroup(string Title, string Caption, IReadOnlyList<string> Items);

public sealed record LanguageSkill(string Name, string Level, int Strength);

public sealed record Education(string Program, string School, string Period, string? Location);

/* ---------------------------------------------------------------------
   Projects
   --------------------------------------------------------------------- */

/// <summary>
/// Card-level summary of a project. Case studies live at their own route so each
/// one can render a completely different layout for a completely different stack.
/// </summary>
public sealed record ProjectSummary(
    string Slug,
    string Title,
    string Kind,
    string Tagline,
    string Description,
    IReadOnlyList<string> Stack,
    IReadOnlyList<Metric> Metrics,
    string? CaseStudyHref,
    string? ExternalHref,
    string Status,
    bool IsCentrepiece);

public sealed record Metric(string Value, string Label, string? Note = null);

/* ---------------------------------------------------------------------
   Case-study building blocks, shared by every case study, not just APF
   --------------------------------------------------------------------- */

/// <summary>One stage in an architecture diagram rendered by <c>ArchitectureFlow</c>.</summary>
/// <param name="Label">Stage name.</param>
/// <param name="Detail">Expanded explanation, revealed on interaction.</param>
/// <param name="Sub">Short qualifier shown next to the label.</param>
/// <param name="Icon">Glyph key resolved by <c>FlowIcon</c>.</param>
/// <param name="Highlight">Marks the stages that carry the design's main idea.</param>
public sealed record FlowNode(
    string Label,
    string Detail,
    string Sub,
    string Icon,
    bool Highlight = false);

/// <summary>
/// A problem → fix → measured delta narrative. The <c>Lesson</c> is the point;
/// the numbers are the evidence that it was learned rather than asserted.
/// </summary>
public sealed record EngineeringStory(
    string Kicker,
    string Title,
    string Problem,
    string Fix,
    string Before,
    string After,
    string DeltaNote,
    string Lesson);

public sealed record FactRow(string Label, string Value, string? Note = null);

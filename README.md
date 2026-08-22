# Ahmed Ghanafer portfolio

A personal developer portfolio built as a **standalone Blazor WebAssembly app** on .NET 8,
published as static files. Routing, state, the theme system and every visualisation are C#
running in the browser. There is no JavaScript framework, no bundler, and no backend. The
whole site is a component tree compiled to WebAssembly plus about a hundred lines of interop
for the handful of things the DOM genuinely owns.

## Run it locally

```bash
dotnet run
```

Then open the URL it prints (typically `http://localhost:5xxx`). Hot reload works:

```bash
dotnet watch
```

Build and publish checks:

```bash
dotnet build                                  # must be clean
dotnet publish -c Release -o dist             # static output lands in dist/wwwroot
```

## Structure

```
Program.cs                  Service registration, two services, no HttpClient (there is no API)
App.razor                   Router, NotFound branch
_Imports.razor

Data/                       All content, as plain records. No CMS, no markdown loader.
  Models.cs                 Shared record types: Profile, Role, ProjectSummary, FlowNode, …
  SiteData.cs               CV-derived content: profile, experience, skills, languages, schooling
  ProjectCatalog.cs         The project index, one entry per project
  ApfCaseStudy.cs           Measured facts for the Albion Profit Forge case study
  ApfNarrative.cs           The written narrative for the same case study

Layout/
  MainLayout.razor          Skip link, header, footer, per-navigation scroll + reveal handling
  SiteHeader.razor          Sticky nav with in-page anchors that survive a route change
  SiteFooter.razor

Components/                 Reusable presentation. Nothing here knows any specific content.
  Section.razor             Numbered page section
  Card.razor  Callout.razor  TagList.razor  MetricGrid.razor  FactTable.razor
  Reveal.razor              Scroll-reveal wrapper
  ThemeToggle.razor  Icon.razor
  TimelineItem.razor  ExperienceTimeline.razor
  ProjectLead.razor  ProjectCard.razor  ProjectShowcase.razor
  SkillsGrid.razor  LanguageBars.razor  EducationList.razor  ContactLinks.razor
  RuntimeBadge.razor        Real boot numbers from the navigation timing API
  ArchitectureFlow.razor    Expandable pipeline diagram (real markup, not an image)
  StoryCard.razor           problem → fix → measured delta → lesson
  FreshnessLab.razor        Interactive decay curve, computed in C#
  WarmupChart.razor  PayloadBars.razor
  CaseStudySection.razor  CaseStudyToc.razor
  NotFoundView.razor

Pages/
  Home.razor                        /
  Projects/AlbionProfitForge.razor  /projects/albion-profit-forge

Services/
  JsInterop.cs              The entire JS surface, behind one guarded facade
  ThemeService.cs           Light/dark state

wwwroot/
  index.html                Pre-paint theme script, boot screen, noscript fallback
  css/app.css               The design system, in numbered layers
  js/site.js                localStorage, IntersectionObserver, scroll, timing API, nothing else
  favicon.svg               Theme-aware
  .nojekyll

deploy/Caddyfile.example    Serving the same output from a VPS
.github/workflows/deploy.yml
```

## Adding another case study

The architecture is built for this. Two steps:

1. Append a `ProjectSummary` to `Data/ProjectCatalog.cs`, with a `CaseStudyHref` of
   `/projects/<slug>`.
2. Add `Pages/Projects/<Name>.razor` with a matching `@page` directive.

The new page composes the same primitives (`CaseStudySection`, `Callout`, `FactTable`,
`MetricGrid`, `StoryCard`, `ArchitectureFlow`, `CaseStudyToc`) but owns its own layout, so a
Python/Postgres write-up and a .NET one don't have to pretend to be the same shape. `ProjectShowcase`
picks up the new entry automatically; it knows no project by name.

Only one project may set `IsCentrepiece`: that one leads the section, the rest become cards.

## Design system

Everything lives in `wwwroot/css/app.css`, in numbered layers: tokens → reset → type → layout →
components → motion → print. There are no utility classes and no framework.

- **Type**: Archivo for display, IBM Plex Sans for body, IBM Plex Mono for labels and figures.
- **Colour**: a warm neutral surface ramp with a terracotta accent
  (`#b5541d` light, `#e0864a` dark). Every colour is a custom property; dark mode redefines
  the tokens, not the rules.
- **Theme**: chosen before first paint by an inline script in `index.html`, so there is never a
  flash of the wrong surface while the runtime downloads. `ThemeService` reads that decision back
  and owns everything after it. The choice persists in `localStorage`.

## Accessibility and motion

- Semantic landmarks, one `h1` per route, no skipped heading levels.
- Skip link, visible focus rings, `aria-expanded` disclosures, a `role="switch"` theme toggle.
- Every animation is behind `prefers-reduced-motion`, and the *revealed* state is the CSS default: if `IntersectionObserver` never fires, the content is simply there. A failsafe in `site.js`
  drops the effect entirely if nothing has revealed after 2.5 s, because a missing animation is a
  rounding error and a blank page is a broken site.
- Charts carry text alternatives; measured figures are real `<table>` elements.
- Wide content (formula, tables) scrolls inside its own container; the page body never does.

## Deployment

Nothing is deployed automatically. The workflow is `workflow_dispatch` only; the `push` trigger is
commented out in `.github/workflows/deploy.yml`.

### GitHub Pages

The workflow publishes, then does the three things a Blazor SPA needs on Pages:

1. **Rewrites `<base href>`** to `/<repo>/` for a project site, or leaves it `/` for a
   `<user>.github.io` site. Blazor resolves every asset against this; get it wrong and nothing loads.
2. **Copies `index.html` to `404.html`** *after* the rewrite. Pages serves `404.html` for unknown
   paths, and because it is byte-identical the router picks the route up from the URL.
3. **Touches `.nojekyll`**, so Jekyll doesn't strip `_framework/`.

To enable it: repository *Settings → Pages → Source: GitHub Actions*, then run the workflow.

### Any static host (Caddy, nginx, S3)

`dotnet publish -c Release -o dist` and serve `dist/wwwroot`. The only requirements are an SPA
fallback to `index.html` and a correct `<base href>`. See `deploy/Caddyfile.example` for a working
config with compression and sensible cache headers.

// ---------------------------------------------------------------------------
// The entire JavaScript surface of this site.
//
// Everything here is something the DOM genuinely owns, and that re-implementing
// in C# would only make worse: persisted preference storage, IntersectionObserver,
// scroll position, and the navigation timing API. All UI state, routing and
// rendering stays in Blazor.
//
// Nothing in this file is load-bearing for content. If the module fails to load,
// the page is still complete — only the motion is missing.
// ---------------------------------------------------------------------------

const THEME_KEY = "ahg-theme";
const REDUCED = () => window.matchMedia("(prefers-reduced-motion: reduce)").matches;

/* --- Theme ---------------------------------------------------------------- */

export function readStoredTheme() {
    try {
        return localStorage.getItem(THEME_KEY);
    } catch {
        // Private mode or blocked storage. Falling back to the OS preference is fine.
        return null;
    }
}

export function prefersDark() {
    return !!(window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches);
}

export function applyTheme(theme) {
    document.documentElement.setAttribute("data-theme", theme);

    const meta = document.querySelector('meta[name="theme-color"]');
    if (meta) meta.setAttribute("content", theme === "dark" ? "#100e0c" : "#f7f4f0");

    try { localStorage.setItem(THEME_KEY, theme); } catch { /* not fatal */ }
}

/* --- Scroll reveal -------------------------------------------------------- */

let revealObserver = null;
let failsafeArmed = false;

/**
 * Content must never be permanently invisible.
 *
 * If nothing has revealed a couple of seconds after the first scan, the observer
 * is not firing in this environment — a background tab that never composites, a
 * prerender, an engine quirk. In that case drop the effect and show everything:
 * a missing animation is a rounding error, a blank page is a broken site.
 */
function armRevealFailsafe() {
    if (failsafeArmed) return;
    failsafeArmed = true;

    setTimeout(() => {
        if (document.querySelector(".reveal.is-revealed")) return;
        revealObserver = { observe: el => el.classList.add("is-revealed") };
        document.querySelectorAll(".reveal").forEach(el => el.classList.add("is-revealed"));
    }, 2500);
}

/**
 * Reveals `.reveal` elements once each, as they enter the viewport.
 * Idempotent and cheap: call it after any render that may have added nodes.
 */
export function scanReveals() {
    const targets = document.querySelectorAll(".reveal:not(.is-revealed)");
    if (!targets.length) return;

    if (REDUCED() || !("IntersectionObserver" in window)) {
        targets.forEach(el => el.classList.add("is-revealed"));
        return;
    }

    armRevealFailsafe();

    if (!revealObserver) {
        revealObserver = new IntersectionObserver((entries, obs) => {
            for (const entry of entries) {
                if (!entry.isIntersecting) continue;
                entry.target.classList.add("is-revealed");
                obs.unobserve(entry.target);
            }
        }, { rootMargin: "0px 0px -8% 0px", threshold: 0.08 });
    }

    targets.forEach(el => revealObserver.observe(el));
}

/* --- Sticky header -------------------------------------------------------- */

let headerWatched = false;

export function watchHeader() {
    if (headerWatched) return;

    const header = document.querySelector(".site-header");
    if (!header) return;

    headerWatched = true;
    const update = () => header.setAttribute("data-stuck", String(window.scrollY > 8));
    update();
    window.addEventListener("scroll", update, { passive: true });
}

/* --- Case-study scrollspy ------------------------------------------------- */
//
// Which section is "current" is view state, so Blazor owns it. This only reports
// the crossing; the component decides what to do about it.

let spyObserver = null;
let spyRef = null;
let spyActive = null;
let spyToken = 0;

/**
 * Reports which of `ids` is the current section.
 *
 * Returns a token. Two components use this — the home nav and the case-study
 * table of contents — and during a route change both are briefly alive, so a
 * stop only takes effect if it names the run it started. Without that, the
 * outgoing component's teardown silently kills the incoming one's spy.
 */
export function startScrollSpy(ids, dotNetRef) {
    forceStopScrollSpy();
    if (!ids || !ids.length || !("IntersectionObserver" in window)) return 0;

    const token = ++spyToken;
    spyRef = dotNetRef;
    const onScreen = new Set();

    const publish = () => {
        // The first id still on screen wins; otherwise the last one scrolled past.
        let next = ids.find(id => onScreen.has(id));

        if (!next) {
            const passed = ids.filter(id => {
                const el = document.getElementById(id);
                return el && el.getBoundingClientRect().top < window.innerHeight * 0.4;
            });
            next = passed.length ? passed[passed.length - 1] : ids[0];
        }

        if (next === spyActive) return;
        spyActive = next;

        try { spyRef?.invokeMethodAsync("OnSectionChanged", next); } catch { /* torn down */ }
    };

    spyObserver = new IntersectionObserver(entries => {
        for (const entry of entries) {
            if (entry.isIntersecting) onScreen.add(entry.target.id);
            else onScreen.delete(entry.target.id);
        }
        publish();
    }, { rootMargin: "-12% 0px -68% 0px", threshold: 0 });

    ids.forEach(id => {
        const el = document.getElementById(id);
        if (el) spyObserver.observe(el);
    });

    publish();
    return token;
}

export function stopScrollSpy(token) {
    if (token && token !== spyToken) return;   // a stale teardown; leave the live spy alone
    forceStopScrollSpy();
}

function forceStopScrollSpy() {
    if (spyObserver) { spyObserver.disconnect(); spyObserver = null; }
    spyRef = null;
    spyActive = null;
}

/* --- Scrolling ------------------------------------------------------------ */

const behaviour = () => (REDUCED() ? "auto" : "smooth");

export function scrollToTop() {
    window.scrollTo({ top: 0, behavior: behaviour() });
}

/** Returns false when the id isn't on the page, so the caller can fall back. */
export function scrollToId(id) {
    const el = document.getElementById(id);
    if (!el) return false;
    el.scrollIntoView({ behavior: behaviour(), block: "start" });
    return true;
}

/* --- Boot screen ---------------------------------------------------------- */

export function markReady() {
    document.body.classList.add("blazor-ready");
}

/* --- Runtime telemetry ---------------------------------------------------- */

/**
 * Real numbers, measured in this browser on this load. A site that claims to be
 * fast should be willing to print its own figures rather than assert them.
 */
export function bootStats() {
    let bootMs = 0;
    let transferKb = 0;

    try {
        const nav = performance.getEntriesByType("navigation")[0];
        if (nav) bootMs = Math.round(nav.domContentLoadedEventEnd || nav.responseEnd || 0);

        // Everything the runtime actually pulled down for this page.
        const bytes = performance.getEntriesByType("resource")
            .reduce((sum, r) => sum + (r.transferSize || 0), 0)
            + (nav ? (nav.transferSize || 0) : 0);

        transferKb = Math.round(bytes / 1024);
    } catch { /* timing API unavailable; zeros read as "not measured" */ }

    // Prefer the time to first paint when the browser reports it - it is closer
    // to what the visitor experienced than DOMContentLoaded.
    try {
        const paint = performance.getEntriesByName("first-contentful-paint")[0];
        if (paint && paint.startTime) bootMs = Math.round(paint.startTime);
    } catch { /* optional */ }

    return {
        bootMs,
        transferKb,
        cores: navigator.hardwareConcurrency || 0,
        wasm: typeof WebAssembly === "object" ? "supported" : "unavailable",
    };
}

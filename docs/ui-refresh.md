# MechanicShop interface refresh

## Scope and design contract

Refresh the Blazor interface for a portfolio-ready workshop management product. Keep routes, authorization, API contracts, event handlers, SignalR, database operations and deployment configuration intact. Existing uncommitted backend and hub changes belong to the user.

The public home explains the actual management product, not a fictional repair business. Its primary action is signing in with the existing demo accounts. Any illustrative workspace data must be explicitly labeled as a preview. Authenticated pages use a shared navigation shell, readable tables, consistent forms and clear status colors.

## Visual system

- Cool slate canvas `#E9EDF3`; surface `#F9FAFC`; navy ink `#22314B`; muted text `#56647A`; copper action accent `#AE491F`; navy navigation `#202D43` with a warm highlight `#EFB482`.
- Native humanist sans typography: Segoe UI, with system fallbacks. No new font requests or runtime packages.
- Desktop: brand, five core navigation links and account actions share one top header. Workspace content uses the full page width with consistent gutters. Team and vehicle catalog links remain in manager settings. Mobile: the five sections stay visible in an equal-width row below the brand and account controls; no permanent sidebar or horizontally scrolling navigation.
- Rounded corners reflect hierarchy: 8px controls, 12px content surfaces, 20px feature presentation. Borders are the main separator; shadows are reserved for overlays and the product preview.
- Meaningful interaction feedback, visible keyboard focus, 44px primary touch targets, reduced-motion support, and local static assets only.

## Implementation and verification

1. Replace the shared layout and public home, establish global tokens after Bootstrap and remove dark-only presentation styles.
2. Redesign sign-in, dashboard hierarchy, dialogs, schedule presentation, records, details and invoicing. Preserve bindings and business actions.
3. Build the API and client; run existing unit tests. Verify the actual running app at desktop, tablet and mobile widths, including demo sign-in, navigation, filtering, dialogs, settings, scheduling, detail and invoice pages where data permits.
4. Record observed limitations and transfer instructions for the other deployment branch. No commit, push, deployment or backend dependency upgrade is included.

## Branch portability

The UI change set is limited to `MechanicShop.Client` presentation files, `MechanicShop.Api/Components/App.razor`, `MechanicShop.Api/wwwroot` assets and this document. Apply the same UI commit to the other branch with `git cherry-pick <ui-commit>` after reviewing and committing only those paths. Do not include the pre-existing hub, identity or API changes in that UI commit.

## Verification results — 16 November 2026

- API + Blazor WebAssembly build: passed, zero errors. Seven existing NuGet vulnerability warnings were present before the UI changes. They were resolved in the subsequent [dependency security maintenance](dependency-security-maintenance.md); that verification also ran the full 688-test suite.
- Existing tests: 208 Domain unit tests and 30 Application unit tests passed (238 total).
- Lighthouse snapshot audits: public home (desktop) and services list (mobile) each scored 100 in Accessibility, Best Practices and SEO. These are snapshot checks, not performance benchmarks or a full-site conformance claim.
- Public home verified at 320, 390, 768, 1024 and 1440 CSS pixels; no document-level horizontal overflow. Records, dialogs and detail views reviewed on mobile; large tables and schedule grids scroll within their containers.
- Manager and technician demo sign-in passed. The technician navigation omits manager pages; directly visiting employee management shows the permission message without employee data.
- Dashboard verified with existing historical data using `/?date=2026-08-02`, including all order, financial and performance metrics.
- Customer search and its empty result state passed. Create-customer dialog opens, scrolls on mobile, traps keyboard focus, closes with Escape and restores focus.
- Work order status filtering returned completed orders; pagination showed records 1–10 and 11–20 correctly on pages 1 and 2.
- Settings opens and closes with keyboard support. Existing invoice and work order details loaded; invoice PDF endpoint returned HTTP 200 after using Download PDF, without browser console errors.
- No production deployment, commits, dependency changes or data-changing CRUD submissions were performed. Invoice settlement, staff password changes and a complete booking lifecycle were not exercised against stored data. Existing backend/hub edits were preserved.

The app can be previewed with `dotnet run --project Src/MechanicShop.Api --launch-profile http` at `http://localhost:5001`, using the existing local services and configuration.

## Navigation and palette revision — 17 November 2026

- Applied the user's preference for top navigation and removed the persistent sidebar, its administration section and its unused styles.
- Applied navy, slate and copper tokens consistently across the header, buttons, forms, dashboard, public home, sign-in, work order details, invoice and favicon. Existing status labels keep distinct semantic colors.
- Release API/Blazor build passed with 0 warnings and 0 errors. This revision changes presentation only; the earlier 688-test dependency verification remains recorded separately.
- Checked 320, 390, 768, 1024 and 1440px viewports: all five navigation links visible, no document-level horizontal overflow, and no administration links in the header. Navigation targets measured 48px high on desktop/tablet and 60px on small phones.
- Verified manager settings still contains Team and Vehicle Makes links; opening Team from settings navigates correctly and closes the panel.
- Dashboard Lighthouse snapshot: 100 Accessibility, Best Practices and SEO, with no failing audits. Browser console had no warnings or errors.
- Measured primary button text contrast at 5.57:1, copper text on its soft background at 4.70:1, muted text on the canvas at 5.11:1, header navigation at 8.91:1 and form control borders at 3.27:1.

## Contrast correction — 17 November 2026

- Fixed the time-slot arrow inheriting Bootstrap's white badge text on a light background. The shared component now uses navy text explicitly, with a spoken separator for screen readers; uncolored badges also default to the theme's dark text.
- Corrected warning, danger and success outline button colors and their hover/active states. Reset Password now has readable text and icons; primary/info active states retain white text on copper.
- Darkened the schedule's current-time marker and technician label, corrected an invalid service-badge class, and increased footer-divider contrast. Bindings, handlers and backend behavior are unchanged.
- Reviewed work orders, customers, services, team, vehicle makes, schedule and manager settings in the running browser. Computed text/icon contrast checks after the fixes found no low-contrast candidates on work orders (259 checks), team (57) and schedule (518). These checks complement visual review and are not a full accessibility-conformance audit.
- Release API/Blazor build passed with 0 warnings and 0 errors; work-order browser console had no warnings or errors. The previous test-suite results above were not rerun for these presentation-only corrections.

---
name: EventEase UI Enhancement Plan
overview: A structured plan to elevate the EventEase Venue Booking System UI from functional to professional, appealing, and user-friendly for booking specialists, while retaining full CRUD, Azure readiness, and POE compliance.
todos: []
isProject: false
---

# EventEase UI Enhancement Plan

## Context

- **Current state:** Bootstrap 5 default theme, basic cards/tables/forms, minimal custom styling. Functional but generic.
- **Target users:** Booking specialists (admin platform, per POE).
- **POE requirements:** User-friendly, clear view of events/bookings, easy search/filter, Azure integration (unchanged).
- **Goals:** Professional, appealing, complete look; easy to use; CRUD preserved; Azure-ready.

---

## Design direction

### Visual identity

- **Primary palette:** Replace default Bootstrap with a refined, event-management feel:
  - Primary: Deep teal/emerald (`#0d9488` or similar) for branding and CTAs
  - Accent: Warm amber or coral for highlights (Edit, availability)
  - Neutrals: Slate grays for text and backgrounds
- **Typography:** Use a distinct sans-serif (e.g. **Plus Jakarta Sans** or **DM Sans**) via Google Fonts; keep headings clear and hierarchy obvious.
- **Spacing and rhythm:** Consistent padding/margins; comfortable reading density; clear section separation.

### UX principles (booking specialist focus)

- Clear navigation: Current section highlighted; logical grouping (Manage vs Overview).
- At-a-glance info: Capacity, availability, dates visible without extra clicks.
- Form guidance: Labels, placeholders, validation messages, required-field indicators.
- Action clarity: Primary vs secondary buttons; consistent icon use where helpful.
- Empty states: Friendly messages and clear CTAs when no venues, events, or bookings exist.
- Responsiveness: Usable on tablets; graceful mobile degradation.

---

## Implementation phases

### Phase 1: Foundation (layout, theme, global styles)


| Item                     | Location                                                   | Changes                                                                                                                                                   |
| ------------------------ | ---------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Custom CSS variables** | [wwwroot/css/site.css](EventEaseApp/wwwroot/css/site.css)  | Define `--primary`, `--accent`, `--surface`, `--text`, font families. Override Bootstrap vars where needed.                                               |
| **Google Fonts**         | [_Layout.cshtml](EventEaseApp/Views/Shared/_Layout.cshtml) | Add `<link>` for chosen font family.                                                                                                                      |
| **Layout**               | [_Layout.cshtml](EventEaseApp/Views/Shared/_Layout.cshtml) | Refine navbar: active-state styling, icon hints (optional), subtle gradient or border. Improve footer: spacing, links, optional "Powered by Azure" badge. |
| **Alerts**               | [_Layout.cshtml](EventEaseApp/Views/Shared/_Layout.cshtml) | Style TempData alerts with rounded corners, icon placeholder, better placement (top of main content).                                                     |


---

### Phase 2: Home page (landing / dashboard)


| Item                       | Location                                                        | Changes                                                                                                                          |
| -------------------------- | --------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| **Hero**                   | [Views/Home/Index.cshtml](EventEaseApp/Views/Home/Index.cshtml) | Add subtle gradient background or pattern; larger typography; short tagline.                                                     |
| **Feature cards**          | Same                                                            | Upgrade cards: subtle hover elevation, border accent, icon or illustration per section, clearer CTA buttons.                     |
| **Quick stats (optional)** | Same                                                            | If feasible via ViewBag/model: show counts (e.g. Venues, Events, Bookings) to give a dashboard feel. Requires controller change. |


---

### Phase 3: Venues module


| Item              | Location                                                                    | Changes                                                                                                                                                                                                           |
| ----------------- | --------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Index**         | [Views/Venues/Index.cshtml](EventEaseApp/Views/Venues/Index.cshtml)         | Page title + breadcrumb; card grid with image placeholder for venues without images; improved badge styling; icon buttons (Edit, Details, Delete) or clearer button grouping; empty-state partial when no venues. |
| **Create / Edit** | [Views/Venues/Create.cshtml](EventEaseApp/Views/Venues/Create.cshtml), Edit | Form in a card; grouped sections (Basic Info, Image, Availability); floating labels or clearer labels; helper text for Image URL vs Upload; consistent button styling.                                            |
| **Details**       | [Views/Venues/Details.cshtml](EventEaseApp/Views/Venues/Details.cshtml)     | Card layout with image prominent; definition list in a clean grid; action buttons aligned.                                                                                                                        |
| **Delete**        | [Views/Venues/Delete.cshtml](EventEaseApp/Views/Venues/Delete.cshtml)       | Clear warning styling; summary of what will be deleted; prominent Cancel vs Confirm.                                                                                                                              |


---

### Phase 4: Events module


| Item                 | Location                                                            | Changes                                                                                           |
| -------------------- | ------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| **Index**            | [Views/Events/Index.cshtml](EventEaseApp/Views/Events/Index.cshtml) | Same pattern as Venues: cards with image placeholder, event-type badges, venue info; empty state. |
| **Create / Edit**    | Create, Edit                                                        | Form in card; grouped fields; event-type and venue dropdowns styled; date/time picker styling.    |
| **Details / Delete** | Details, Delete                                                     | Match Venues styling; clear hierarchy.                                                            |


---

### Phase 5: Bookings and Overview


| Item                 | Location                                                                      | Changes                                                                                                                                          |
| -------------------- | ----------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Bookings Index**   | [Views/Bookings/Index.cshtml](EventEaseApp/Views/Bookings/Index.cshtml)       | Table in a card; responsive table (horizontal scroll on small screens); striped rows; action buttons compact but clear; empty state.             |
| **Booking Overview** | [Views/Bookings/Overview.cshtml](EventEaseApp/Views/Bookings/Overview.cshtml) | Search/filter section in a card; filter row visually grouped; results table styled; "Total results" more prominent; empty state when no results. |
| **Create / Edit**    | Create, Edit                                                                  | Form in card; event and venue dropdowns; date picker; validation summary styling.                                                                |
| **Details / Delete** | Details, Delete                                                               | Consistent with other modules.                                                                                                                   |


---

### Phase 6: Shared components and polish


| Item                    | Location                                                                              | Changes                                                                                 |
| ----------------------- | ------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------- |
| **Empty state partial** | [Views/Shared/_EmptyState.cshtml](EventEaseApp/Views/Shared/_EmptyState.cshtml) (new) | Reusable partial: icon/illustration, message, CTA button (e.g. "Add your first venue"). |
| **Breadcrumb partial**  | [Views/Shared/_Breadcrumb.cshtml](EventEaseApp/Views/Shared/_Breadcrumb.cshtml) (new) | Optional breadcrumb for Index/Details/Edit/Delete.                                      |
| **Form card wrapper**   | [Views/Shared/_FormCard.cshtml](EventEaseApp/Views/Shared/_FormCard.cshtml) (new)     | Optional partial to wrap forms in a styled card.                                        |
| **Error page**          | [Views/Shared/Error.cshtml](EventEaseApp/Views/Shared/Error.cshtml)                   | Friendly message for users; hide dev-mode details in production.                        |
| **Privacy page**        | [Views/Home/Privacy.cshtml](EventEaseApp/Views/Home/Privacy.cshtml)                   | Simple, clean layout.                                                                   |


---

## Technical constraints and compatibility

- **Bootstrap 5:** Keep Bootstrap; override via custom CSS and CSS variables. No framework swap.
- **CRUD:** No controller or model changes; only view and CSS updates.
- **Azure:** No config changes; UI remains compatible with Azure Web App, SQL DB, and Blob Storage.
- **Accessibility:** Labels on inputs; semantic HTML; sufficient color contrast; focus states for keyboard users.
- **Mobile:** Bootstrap grid and utilities; ensure tables scroll and forms remain usable.

---

## File change summary


| Category     | Files                                                                              |
| ------------ | ---------------------------------------------------------------------------------- |
| **CSS**      | `wwwroot/css/site.css` (major)                                                     |
| **Layout**   | `Views/Shared/_Layout.cshtml`                                                      |
| **Shared**   | `_EmptyState.cshtml`, `_Breadcrumb.cshtml` (new); `Error.cshtml`, `Privacy.cshtml` |
| **Home**     | `Views/Home/Index.cshtml`                                                          |
| **Venues**   | `Index`, `Create`, `Edit`, `Details`, `Delete`                                     |
| **Events**   | `Index`, `Create`, `Edit`, `Details`, `Delete`                                     |
| **Bookings** | `Index`, `Overview`, `Create`, `Edit`, `Details`, `Delete`                         |


---

## Suggested CSS structure (site.css)

```css
/* 1. CSS variables (theme) */
:root {
  --eventease-primary: #0d9488;
  --eventease-accent: #f59e0b;
  --eventease-surface: #f8fafc;
  --eventease-text: #1e293b;
}

/* 2. Typography */
/* 3. Navbar overrides */
/* 4. Card enhancements */
/* 5. Button overrides */
/* 6. Form styling */
/* 7. Table styling */
/* 8. Badge overrides */
/* 9. Empty states */
/* 10. Utilities */
```

---

## Execution order

1. **Phase 1** — Theme, layout, global styles (enables consistent look across all pages).
2. **Phase 6 (partial)** — Create shared partials (`_EmptyState`, optionally `_Breadcrumb`).
3. **Phases 2–5** — Update each module in sequence: Home, Venues, Events, Bookings.

---

## Outcome

- Consistent, modern visual identity.
- Clear navigation and workflow for booking specialists.
- Improved forms, cards, and tables.
- Empty states and error handling.
- Responsive, accessible, and Azure-ready.
- CRUD and existing logic unchanged.


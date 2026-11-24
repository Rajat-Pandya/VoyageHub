# VoyageHub UI Guidelines & Design System

This guide documents the design tokens, layout primitives, component patterns, and conventions used across the VoyageHub Angular SPA to ensure visual consistency, maintainability, and accessibility.

---

## 1. Design Tokens

VoyageHub uses a **4px base** spacing scale defined as CSS Custom Properties on `:root` in `src/styles/_tokens.css`. They are universally available across both SSR and client runtime.

### Spacing Scale

| Token | Value | Common Usage |
| :--- | :--- | :--- |
| `--space-0` | `0px` | Resetting margins or paddings |
| `--space-1` | `4px` | Micro-spacing, inline badges, tight indicator gaps |
| `--space-2` | `8px` | Icon-to-text spacing, compact button padding, pill tags |
| `--space-3` | `12px` | Input control vertical padding, badge groups |
| `--space-4` | `16px` | Standard field gaps, card header padding, mobile container padding |
| `--space-6` | `24px` | Desktop container padding, section gaps, card interior padding |
| `--space-8` | `32px` | Hero section padding, featured metric card padding |
| `--space-12` | `48px` | Empty-state vertical spacing, major landmark separation |

### Border Radius Tokens

| Token | Value | Common Usage |
| :--- | :--- | :--- |
| `--radius-sm` | `4px` | Status chips, entity pills, micro tags |
| `--radius-md` | `8px` | Form inputs, select dropdowns, buttons, alert banners |
| `--radius-lg` | `12px` | Content cards, data tables, modal dialogs |
| `--radius-full` | `9999px` | Circular initials badges, count badges, pill tabs |

### Control & Interactive Target Heights

- **Minimum Interactive Height**: `40px` (`--control-height-md: 40px`).
- Form inputs (`.form-input`, `.form-select`, `.form-textarea`), action buttons (`.btn-dark`, `.btn-primary`, `.btn-outline`), and tab buttons conform to the minimum 40px touch and click target standard.

### Unified Status Chip Colors

Status indicators across all views (requests, approvals, expenses, audit logs) map to standard semantic classes in `src/styles/_components.css`:

| Semantic Class | Target Statuses | Styling |
| :--- | :--- | :--- |
| `.status-pending` | `Pending`, `Submitted`, `Under Review` | Amber background (`#fffbeb`), Amber border & text (`#b45309`) |
| `.status-approved` | `Approved`, `Paid`, `Active`, `Reimbursed` | Emerald background (`#ecfdf5`), Emerald border & text (`#047857`) |
| `.status-completed`| `Completed`, `Booked` | Blue background (`#eff6ff`), Blue border & text (`#1d4ed8`) |
| `.status-rejected` | `Rejected`, `Flagged`, `Inactive`, `Cancelled` | Rose background (`#fef2f2`), Rose border & text (`#b91c1c`) |
| `.status-draft`    | `Draft`, `Saved` | Slate background (`#f1f5f9`), Slate border & text (`#475569`) |

---

## 2. Layout Primitives

VoyageHub defines standard layout classes in `src/styles/_layout.css` to prevent inline spacing and duplicate layout definitions.

### `.page` & `.page-narrow`
Responsive max-width container with standard gutters.
```html
<!-- Standard full-width page (Dashboard, Requests, Approvals, Admin) -->
<div class="page">
  ...
</div>

<!-- Narrow centered page (Forms, Settings, Account) -->
<div class="page-narrow">
  ...
</div>
```
- Desktop: `max-width: 1400px`, padding `var(--space-6)`.
- Narrow: `max-width: 900px`, padding `var(--space-6)`.
- Mobile (< 768px): padding `var(--space-4)`.

### `.page-header`
Standard page banner with title on the left and actions on the right.
```html
<header class="page-header">
  <div class="header-titles">
    <h1>Page Title</h1>
    <p class="subtitle">Brief descriptive caption of page actions</p>
  </div>
  <div class="header-actions action-bar">
    <button type="button" class="btn-dark">Primary Action</button>
  </div>
</header>
```

### `.card` & `.card-grid`
Uniform container with rounded corners and elevation.
```html
<section class="card-grid">
  <div class="card">
    <div class="card-header">
      <h2>Card Title</h2>
    </div>
    <div class="card-body">
      ...
    </div>
  </div>
</section>
```

### `.form-grid`
Two-column desktop layout that collapses gracefully to one column on mobile screens.
```html
<form class="form-grid">
  <div class="form-group">
    <label class="field-label" for="firstName">First Name</label>
    <input id="firstName" class="form-input" type="text" />
  </div>
  <div class="form-group">
    <label class="field-label" for="lastName">Last Name</label>
    <input id="lastName" class="form-input" type="text" />
  </div>
  <div class="form-group full-width">
    <label class="field-label" for="notes">Notes</label>
    <textarea id="notes" class="form-textarea"></textarea>
  </div>
</form>
```

### `.table-wrap`
Encapsulates tabular data to ensure internal horizontal scrolling without horizontal page-level overflow.
```html
<div class="table-wrap">
  <table class="voyage-table">
    <thead>
      <tr>
        <th>COLUMN 1</th>
        <th>COLUMN 2</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>Value 1</td>
        <td>Value 2</td>
      </tr>
    </tbody>
  </table>
</div>
```

### `.empty-state` & `.loading-state`
Standard empty and loading indicators.
```html
<div class="empty-state">
  <svg ...></svg>
  <h3>No records found</h3>
  <p>Try refining your filters or search keywords.</p>
</div>
```

---

## 3. Shared Standalone Components

Three lightweight Angular components reside in `src/app/shared/`:
1. `<app-page-header [title]="..." [subtitle]="...">`: Uniform header with content projected action slot.
2. `<app-empty-state [title]="..." [message]="..." [icon]="...">`: Standard centered empty/placeholder display.
3. `<app-status-chip [status]="...">`: Normalizes arbitrary status strings to unified color mapping.

---

## 4. UI Do's and Don'ts

### Do's:
- **Always** use `--space-*` CSS variables for margins, paddings, and gaps (`margin-bottom: var(--space-4)`).
- **Always** wrap tables in `.table-wrap` to keep scroll within the card on narrow devices.
- **Always** provide visible focus indicators (`:focus-visible`) for keyboard navigation.
- **Always** ensure button and form input heights are at least 40px for touch ergonomics.
- **Always** test views at 360px, 768px, 1280px, and 1920px viewports.

### Don'ts:
- **Do not** write hardcoded pixel spacing like `padding: 23px` or `margin: 17px`.
- **Do not** add `!important` to spacing or typography overrides.
- **Do not** use inline `style="..."` for spacing or layout adjustments.
- **Do not** introduce new `::ng-deep` selectors in component stylesheets.
- **Do not** let pages produce horizontal window scrollbars.

---

## 5. Adding a New Page Consistently

To add a new page to VoyageHub:
1. Wrap the template in `<div class="page">` (or `.page-narrow`).
2. Add `<header class="page-header">` with `<h1>` and `.subtitle`.
3. Wrap cards in `.card` and group forms in `.form-grid`.
4. Wrap any data tables in `<div class="table-wrap">`.
5. Use `<div class="empty-state">` for empty lists or zero-match search results.
6. Only use CSS variables from `_tokens.css` for margins, paddings, and gaps.

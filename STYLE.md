# STYLE.md — Monolith Design System
> Neo-brutalist · Monotone · Hard-edged · No gradients · Asymmetric border shadows

---

## 1. Design Philosophy

This UI uses a **neo-brutalist** aesthetic:

- **Zero rounding** — `border-radius: 0` everywhere, no exceptions
- **Stark monotone** — three values only: near-black, warm off-white, single mid-tone
- **Asymmetric border shadow** — right and bottom borders are thick, top and left are thin; this creates a flat "lifted" illusion without any box-shadow, blur, or gradient
- **Monospace typography** — Courier New throughout; reinforces the utilitarian/industrial character
- **No decorative effects** — no gradients, no drop shadows, no blur, no animations beyond functional micro-interactions

---

## 2. Color Tokens

```css
:root {
  --ink:   #0a0a0a;   /* near-black — primary text, borders, fills */
  --paper: #f5f3ee;   /* warm off-white — page background, inverse text */
  --mid:   #d4d0c8;   /* warm gray — dividers, disabled states, bar tracks */
}
```

**Rules:**
- Only these three values are used for color. No blue, red, green, or accent hues.
- `--ink` on `--paper` = default state
- `--paper` on `--ink` = inverted / active / selected state
- `--mid` is used only for subtle dividers, empty bar tracks, and muted borders

---

## 3. The Asymmetric Border System

This is the defining visual feature. Every box, card, button, and input uses this pattern:

```css
/* Standard panel / card */
border-top:    1.5px solid #0a0a0a;
border-left:   1.5px solid #0a0a0a;
border-right:  4px solid #0a0a0a;
border-bottom: 4px solid #0a0a0a;
```

```css
/* Small / tight variant (checkboxes, tags, small inputs) */
border-top:    1.5px solid #0a0a0a;
border-left:   1.5px solid #0a0a0a;
border-right:  2.5px solid #0a0a0a;
border-bottom: 2.5px solid #0a0a0a;
```

**Shorthand CSS variables for convenience:**
```css
:root {
  --b-thin:   1.5px solid #0a0a0a;
  --b-thick:  4px solid #0a0a0a;
  --b-mid:    2.5px solid #0a0a0a;
}
```

**Never use:**
- `box-shadow` for depth
- `border-radius` on any element
- Equal borders on all four sides (that removes the shadow illusion)

---

## 4. Typography

```css
:root {
  --font: 'Courier New', Courier, monospace;
}

body {
  font-family: var(--font);
  color: var(--ink);
  background: var(--paper);
}
```

| Role             | Size  | Weight | Letter Spacing | Transform   |
|------------------|-------|--------|----------------|-------------|
| Page title       | 22px  | 700    | -0.02em        | none        |
| Section label    | 9px   | 400    | 0.18em         | uppercase   |
| Panel title      | 11px  | 700    | 0.12em         | uppercase   |
| Body / task text | 12px  | 400    | 0.02em         | none        |
| Button           | 11px  | 700    | 0.10em         | uppercase   |
| Stat number      | 28px  | 700    | -0.03em        | none        |
| Metadata / time  | 9–10px| 400    | 0.06–0.08em    | uppercase   |
| Table header     | 9px   | 400    | 0.12em         | uppercase   |
| Table cell       | 11px  | 400    | none           | none        |

**Rules:**
- No font other than Courier New / monospace
- No weights other than 400 and 700
- Uppercase is used for labels, buttons, nav, and metadata — never for body content
- Muted text uses `color: #666` or `color: #888` — not a CSS variable, inline is fine

---

## 5. Buttons

```css
.btn {
  font-family: 'Courier New', Courier, monospace;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  padding: 9px 18px;
  cursor: pointer;

  /* Asymmetric border shadow */
  border-top:    1.5px solid #0a0a0a;
  border-left:   1.5px solid #0a0a0a;
  border-right:  4px solid #0a0a0a;
  border-bottom: 4px solid #0a0a0a;

  background: #f5f3ee;
  color: #0a0a0a;
  border-radius: 0;
}

.btn:hover {
  background: #0a0a0a;
  color: #f5f3ee;
}

.btn:active {
  transform: translate(2px, 2px);
  border-right:  1.5px solid #0a0a0a;
  border-bottom: 1.5px solid #0a0a0a;
}
```

**Inverted button** (dark fill, light text):
```css
.btn-inv {
  background: #0a0a0a;
  color: #f5f3ee;
}

.btn-inv:hover {
  background: #f5f3ee;
  color: #0a0a0a;
}
```

**Small button** (inside tables, panel headers):
```css
padding: 3px 10px;
font-size: 9px;
/* Same border rules apply */
```

---

## 6. Inputs

```css
input, textarea, select {
  font-family: 'Courier New', Courier, monospace;
  font-size: 12px;
  letter-spacing: 0.04em;
  padding: 8px 12px;

  border-top:    1.5px solid #0a0a0a;
  border-left:   1.5px solid #0a0a0a;
  border-right:  4px solid #0a0a0a;
  border-bottom: 4px solid #0a0a0a;
  border-radius: 0;

  background: #ffffff;
  color: #0a0a0a;
  outline: none;
  width: 100%;
}

input:focus, textarea:focus {
  background: #f5f3ee;
}
```

---

## 7. Panels / Cards

```css
.panel {
  border-top:    1.5px solid #0a0a0a;
  border-left:   1.5px solid #0a0a0a;
  border-right:  4px solid #0a0a0a;
  border-bottom: 4px solid #0a0a0a;
  background: #ffffff;
  border-radius: 0;
}

.panel-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  border-bottom: 1.5px solid #0a0a0a;
}

.panel-title {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.panel-body {
  padding: 14px 16px;
}
```

---

## 8. Stat Cards

```css
.stat-card {
  border-top:    1.5px solid #0a0a0a;
  border-left:   1.5px solid #0a0a0a;
  border-right:  4px solid #0a0a0a;
  border-bottom: 4px solid #0a0a0a;
  background: #f5f3ee;
  padding: 16px;
}

.stat-label {
  font-size: 9px;
  letter-spacing: 0.15em;
  text-transform: uppercase;
  color: #666;
  margin-bottom: 8px;
}

.stat-num {
  font-size: 28px;
  font-weight: 700;
  letter-spacing: -0.03em;
  line-height: 1;
}

.stat-delta {
  font-size: 10px;
  margin-top: 6px;
  letter-spacing: 0.05em;
}
```

---

## 9. Status Pills / Tags

```css
.status-pill {
  font-size: 9px;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  padding: 2px 8px;
  border: 1.5px solid #0a0a0a;
  border-radius: 0;
  display: inline-block;
}

/* Variants */
.s-active { background: #0a0a0a; color: #f5f3ee; }
.s-hold   { background: #d4d0c8; color: #0a0a0a; }
.s-done   { border-style: dashed; color: #777;   background: transparent; }
```

---

## 10. Checkboxes

```css
.checkbox {
  width: 16px;
  height: 16px;
  border-top:    1.5px solid #0a0a0a;
  border-left:   1.5px solid #0a0a0a;
  border-right:  2.5px solid #0a0a0a;
  border-bottom: 2.5px solid #0a0a0a;
  border-radius: 0;
  background: transparent;
  cursor: pointer;
  flex-shrink: 0;
}

.checkbox.done {
  background: #0a0a0a;
}

/* Checkmark via pseudo-element */
.checkbox.done::after {
  content: '';
  display: block;
  width: 8px;
  height: 5px;
  border-left: 2px solid #f5f3ee;
  border-bottom: 2px solid #f5f3ee;
  transform: rotate(-45deg) translateY(-1px);
}
```

---

## 11. Tables

```css
.table {
  width: 100%;
  font-size: 11px;
  border-collapse: collapse;
  font-family: 'Courier New', Courier, monospace;
}

.table th {
  text-align: left;
  font-size: 9px;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  font-weight: 400;
  padding: 6px 10px;
  background: #0a0a0a;
  color: #f5f3ee;
}

.table td {
  padding: 8px 10px;
  border-bottom: 1px solid #d4d0c8;
}

.table tr:hover td {
  background: #f5f3ee;
}
```

---

## 12. Bar Charts (CSS-only)

```css
.bar-track {
  height: 20px;
  background: #d4d0c8;
  border: 1px solid #0a0a0a;
  border-radius: 0;
}

.bar-fill {
  height: 100%;
  background: #0a0a0a;
}
```

Progress bars follow the same pattern at `height: 8px`.

---

## 13. Navigation

### Top Bar
```css
.topbar {
  background: #0a0a0a;
  color: #f5f3ee;
  height: 52px;
  border-bottom: 4px solid #0a0a0a;   /* thick bottom */
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
}

.topbar-nav a {
  font-size: 11px;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  padding: 6px 14px;
  border: 1.5px solid rgba(245,243,238,0.3);
  color: #f5f3ee;
  text-decoration: none;
}

.topbar-nav a:hover,
.topbar-nav a.active {
  background: #f5f3ee;
  color: #0a0a0a;
}
```

### Sidebar
```css
.sidebar {
  width: 200px;
  background: #0a0a0a;
  color: #f5f3ee;
  border-right: 4px solid #0a0a0a;   /* thick right */
}

.sidebar-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 9px 18px;
  font-size: 12px;
  letter-spacing: 0.05em;
  border-left: 3px solid transparent;
  cursor: pointer;
}

.sidebar-item:hover {
  background: rgba(245,243,238,0.08);
}

.sidebar-item.active {
  background: #f5f3ee;
  color: #0a0a0a;
  border-left: 3px solid #0a0a0a;
}

.sidebar-section {
  font-size: 9px;
  letter-spacing: 0.18em;
  text-transform: uppercase;
  color: rgba(245,243,238,0.45);
  padding: 14px 18px 6px;
}
```

---

## 14. Layout Structure

```
┌─ .topbar (height: 52px, ink bg, thick border-bottom) ─────────────┐
│                                                                     │
├─ .layout (display: flex, flex: 1) ─────────────────────────────────┤
│  ├─ .sidebar (width: 200px, ink bg, thick border-right)            │
│  └─ .main (flex: 1, padding: 24px, paper bg)                       │
│       ├─ .page-header                                               │
│       ├─ .stats-row (grid, 4 columns, gap: 14px)                   │
│       ├─ .grid-2 (grid, 2 columns, gap: 14px)                      │
│       └─ .panel (full width)                                        │
└─────────────────────────────────────────────────────────────────────┘
```

Grid columns:
```css
.stats-row { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; }
.grid-2    { display: grid; grid-template-columns: 1fr 1fr;        gap: 14px; }
```

---

## 15. Spacing Scale

| Token    | Value  | Usage                              |
|----------|--------|------------------------------------|
| 4px      | 4px    | Internal badge / pill padding      |
| 8px      | 8px    | Tight component gaps               |
| 10px     | 10px   | Row padding, notification gaps     |
| 12px     | 12px   | Panel inner padding (horizontal)   |
| 14px     | 14px   | Grid gap, panel body padding       |
| 16px     | 16px   | Stat card padding, panel body      |
| 20px     | 20px   | Page horizontal padding            |
| 24px     | 24px   | Page main padding                  |

No `rem` units needed — everything is px at this scale.

---

## 16. Quick-Reference Cheatsheet

```css
/* The three colors */
--ink:   #0a0a0a
--paper: #f5f3ee
--mid:   #d4d0c8

/* The border rule — ALWAYS asymmetric */
border-top:    1.5px solid #0a0a0a;   /* thin */
border-left:   1.5px solid #0a0a0a;   /* thin */
border-right:  4px solid #0a0a0a;     /* THICK */
border-bottom: 4px solid #0a0a0a;     /* THICK */

/* NEVER */
border-radius: anything;
box-shadow: anything;
background: gradient / rgba / anything other than the 3 colors above;
font-family: anything other than 'Courier New', Courier, monospace;
font-weight: anything other than 400 or 700;
```

---

*This file is the single source of truth for the Monolith design system.
Any component not listed here should be derived from these same rules.*

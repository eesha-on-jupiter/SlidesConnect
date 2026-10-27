# Workflows

Component chains for the things people usually build. Components only; see each component's page
for its inputs. `─▶` is a wire; a `Merge` is the Grasshopper Merge component (slot order = request
order).

Every workflow ends the same way: Request JSON goes into **Requests Aggregator**, its
`Requests JSON` goes into **Batch Update** with the Token and Presentation ID, and Batch Update's
**Send** button is clicked. Order matters inside a batch; a slide must be created before anything
is placed on it, so wire the slide's request first.

---

## 1. First slide

```
Google Slides Auth (check the token)

Create Presentation (click Create) ──PID──────────────────────────┐
                                                                  ▼
Create Slide  ─▶ D1 ┐                                        Batch Update
Create Shape  ─▶ D2 ├─ Merge ─▶ Requests Aggregator ──RQS──▶   (Send)
Insert Text   ─▶ D3 ┘
```

- Create Slide's `Slide ID` ─▶ Create Shape's `Slide ID`
- Create Shape's `Object ID` ─▶ Insert Text's `Object ID`

See [quickstart.md](quickstart.md) for the step-by-step version.

---

## 2. Styled text box in one component

```
Create Slide ──SID──▶ Styled Text Box ─▶ Requests Aggregator ─▶ Batch Update
```

- One box per `Text` item; connect only the traits you want (Bold, Font Size, Text Color, Fill
  Color, Alignment, Autofit…).
- `Same Box = true` joins every Text item into one box, each keeping its own style; a letter list
  with a sampled `Font Size` list gives a wave, a `Gradient` into `Text Color` a colour sweep.

---

## 3. Restyle text that's already there

```
Create Shape / Styled Text Box / Create Slide (Title/Body Object ID)
      │ Object ID
      ├─▶ Bold Text        ─┐
      ├─▶ Text Color Text  ─┼─ Merge ─▶ Requests Aggregator ─▶ Batch Update
      └─▶ Aligned Text     ─┘
```

- One-trait components (Text Style tab) each emit one request per Object ID.
- Styled Text / Styled Paragraph take every trait at once; Bullet List turns the paragraphs into a
  list. All three take `Row` / `Column` to target a table cell instead of a shape.

---

## 4. Deck from a hand-designed Layout

```
List Layouts ──Layout ID──▶ Build Slides From Layout ─▶ Requests Aggregator ─▶ Batch Update
                               ▲    ▲     ▲      ▲
                           Title  Subtitle  Body  Photo URL   (flat lists, one item per slide)
```

- Design a Layout in Google Slides with real placeholders (title, body, picture), then pick its
  `Layout ID` from List Layouts by `Layout Name`.
- Finer control per slide: List Layout Placeholders ─▶ Create Slide (placeholder IDs) ─▶ Insert Text
  / Replace Image on the resolved Title/Body/Picture Object IDs.

---

## 5. One slide per CSV record

```
Read CSV ──Columns, Headers──▶ Get CSV Column ×N ─▶ (format Title / Body text)
                                                          │
Camera planes ─▶ Capture Camera View (Capture) ─▶ Get Image URL (Get URLs) ──Photo URL──┐
                                                          ▼                             ▼
                                              Build Slides From Layout ─▶ Requests Aggregator ─▶ Batch Update
```

- One row per record (e.g. one per building floor) becomes one slide, with one captured view each.
- Get Image URL needs the `drive.file` scope; see [authentication.md](authentication.md).

---

## 6. Colour-coded shapes from Rhino geometry

```
Rhino rectangles ─▶ Rhino Shape Export ──X, Y, Width, Height, Rotation, Unit──▶ Create Shape
values ─▶ Remap ─▶ Gradient ─────────────────────────────────────────Color──▶      │
                                                                          Requests Aggregator ─▶ Batch Update
```

- Rhino Shape Export flips Y to Slides' top-left origin and outputs in the model unit; wire its
  `Unit` alongside X/Y/W/H. `Fit To Page` scales the set onto the 10 × 5.625 in page.
- Restyle afterwards with Shape Fill / Shape Outline / Shape Shadow / Shape Text Fit, or all at once
  with Update Shape Style.

---

## 7. Data tree to a styled table

```
Create Slide ──SID──▶ Data Tree To Table ─▶ Requests Aggregator ─▶ Batch Update
data tree (one branch per row) ──Data──▲

Create Slide ──SID──▶ Create Table ──Object ID──▶ Table Cell Style / Table Borders /
                                                   Merge Cells / Table Column Width ─▶ Requests Aggregator
```

- Data Tree To Table creates and fills the table in one component; rows and columns come from the
  tree's shape.
- Create Table makes an empty table of a given size and outputs its `Object ID` for the table
  styling components (Data Tree To Table doesn't expose its table ID in 1.0.0; see its page).
- Cell text styling: Styled Text / Styled Paragraph with `Row` / `Column`.

---

## 8. Images

```
URL list ───────────────────────────────────────────────────────▶ Create Image ─▶ Requests Aggregator
Bitmap / File Path ─▶ Get Image URL (Get URLs) ──Image URL──────▶ Replace Image ─▶ Requests Aggregator
```

- Create Image places a new image; Replace Image swaps the content of an existing image or picture
  placeholder by Object ID.

---

## 9. Find and replace tokens

```
Find Text {{tokens}} ┐
Replace Text values  ┴─▶ Replace All Text ─▶ Requests Aggregator ─▶ Batch Update
```

- Runs across the whole presentation, handy for a template deck that carries `{{project}}`-style
  tokens in ordinary text boxes.

---

## 10. Edit an existing deck

```
Get Presentation (by ID, or by Name) ──Presentation ID──▶ Batch Update
Get Slide ─▶ raw page JSON (inspect element IDs, placeholders)
Delete Slide / Update Slide Background / Update Shape Transform ─▶ Requests Aggregator ─▶ Batch Update
```

- Get Presentation's `Name` lookup needs the `drive.readonly` scope.
- Update Shape Transform with `Relative = true` nudges an element by X/Y without touching its size.

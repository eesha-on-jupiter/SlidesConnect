# Changelog

## 1.0.0 (2026-10-26)

First public release. 62 components across 11 tabs.

- **Auth:** Google Slides Auth validates an OAuth access token and reports its granted scopes.
- **Presentation:** Create Presentation, Get Presentation (by ID, or by title via a Drive lookup) and
  Batch Update, the single write endpoint every request flows into. Batch Update takes a list or a
  tree and sends **one** `batchUpdate` per click; pressing Send again regenerates repeated
  auto-generated IDs instead of failing with "object ID already exists".
- **Slides:** Get Slide (raw page JSON: slides, Layouts and Masters), Create Slide (predefined
  layout or a custom Layout ID, with Title/Body/Picture placeholder mapping), Update Slide
  Background, Delete Slide.
- **Layouts:** List Layouts, List Layout Placeholders and Build Slides From Layout, one slide per
  record from flat Title / Subtitle / Body / Photo URL lists; Read CSV (record-based, handles quoted
  multi-line cells) and Get CSV Column to drive decks from spreadsheet data.
- **Shapes:** Create Shape (all 141 API shape types, optional fill and outline), Update Shape Style
  plus one-trait Shape Fill / Shape Outline / Shape Shadow / Shape Link / Shape Text Fit, Update
  Shape Transform (absolute or relative), and Rhino Shape Export (Rhino rectangles → Slides position,
  size and rotation, Y flipped to the page's top-left origin).
- **Text:** Insert Text, Replace All Text, and Styled Text Box, a text box with any combination of
  styles in one component; **Same Box** styles each letter or word of one box separately with no
  character indices to compute.
- **Text Style:** Styled Text, Styled Paragraph and Bullet List (shapes, placeholders or table cells
  via Row/Column), plus 15 one-trait components (Bold Text, Font Size Text, Aligned Text, …).
- **Tables:** Create Table, Data Tree To Table, Table Cell Style, Table Borders, Merge Cells, Table
  Column Width.
- **Insert:** Create Image, Replace Image, Get Image URL (uploads a Bitmap or local file to Drive and
  returns a link-public URL) and Capture Camera View (one PNG per camera plane, view restored
  afterwards).
- **Aggregate:** Requests Aggregator flattens Request JSON lists or trees, branch by branch, into one
  ordered payload.
- **Presets:** Shape Type, Predefined Layout, Alignment, Font Family, Bullet Preset, Dash Style,
  Border Position, Content Alignment.
- **Graph-driven by design:** every builder input is list-access and index-matched (length-1 lists
  broadcast, ragged lists warn instead of throwing); every colour is a native Grasshopper Colour;
  every length takes a real unit (`in` by default, `pt`, `mm`, `cm`, `m`, `ft`, or raw `emu`);
  rotation, flip and skew compose into one transform about the element's centre. Nothing asks for a
  character index.
- **Safe writes:** Create Presentation, Batch Update, Get Image URL and Capture Camera View act only
  on a button click, never on canvas recompute.

### Known limitations

- Freeform Rhino geometry has no Slides equivalent; Create Shape only places the API's predefined
  shape types. Send custom geometry as an image instead (Capture Camera View → Get Image URL →
  Create Image).
- Custom Layouts and Masters can't be created through the API; design them in Google Slides, then
  reference them by ID (List Layouts).
- Data Tree To Table doesn't output its table's Object ID; to style that table, read the ID from its
  `createTable` request or start from Create Table.
- The Rhino 8 Mac package is built but has not been tested on a Mac.
- Demo `.gh` files are not shipped; [workflows.md](workflows.md) draws the common wirings as
  component chains.

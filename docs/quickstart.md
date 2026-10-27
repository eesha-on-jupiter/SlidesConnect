# Quickstart

Seven components, one canvas: validate a token, create a presentation, add a slide with a text box,
and push it live.

## 1. Get a token

Follow [authentication.md](authentication.md) to get an OAuth 2.0 Bearer token with the
`https://www.googleapis.com/auth/presentations` scope. Wire it into a Grasshopper Panel so you can
reuse it across components.

## 2. Verify the connection

Drop **Auth > Google Slides Auth** ([SlidesConnectAuth](components/Auth/SlidesConnectAuth.md)). Wire your
token Panel into `Token`, then read `Status` / `Response`; a healthy response reports the token's
granted scopes.

## 3. Create a presentation

Drop **Presentation > Create Presentation**
([CreatePresentation](components/Presentation/CreatePresentation.md)). Wire your token in, set
`Title`, click **Create**, and read its `Presentation ID` output; everything downstream chains from this.

## 4. Add a slide and some text

Drop **Slides > Create Slide** ([CreateSlide](components/Slides/CreateSlide.md)) with `Layout` =
`BLANK` (or wire in the [Predefined Layout](components/Presets/PredefinedLayoutPreset.md)).
Its `Slide ID` output is what every downstream component's `Slide ID` input takes.

Drop **Shapes > Create Shape** ([CreateShape](components/Shapes/CreateShape.md)) with `Slide ID` from
the new slide, `Shape Type` = `TEXT_BOX` (or from the
[Shape Type](components/Presets/ShapeTypePreset.md)), and `Width`/`Height`/`X`/`Y`
in `Unit` (default `in`; the page is 10 × 5.625 in). Its `Object ID` output is the new shape's ID.

Drop **Text > Insert Text** ([InsertText](components/Text/InsertText.md)) with `Object ID` from the
shape above and `Text` = "Hello from Grasshopper".

## 5. Gather and push it live

Wire all three components' `Request JSON` outputs into **Aggregate > Requests Aggregator**
([RequestsAggregator](components/Aggregate/RequestsAggregator.md)), then into **Presentation >
Batch Update** ([BatchUpdate](components/Presentation/BatchUpdate.md)) along with your Token and the
`Presentation ID` from step 3, and click **Send**. Read `Status`; a healthy response confirms the requests were applied,
and the presentation now has your slide and text.

## What's next

- [components/](components/) has one page per component with full input/output tables.
- For a deck driven by a hand-designed Layout instead of building shapes one at a time, see
  [Build Slides From Layout](components/Layouts/BuildSlidesFromLayout.md).
- For N slides from a Grasshopper data tree or CSV in one push, feed a list of N items into any unit
  builder (Create Slide, Create Shape, Insert Text, …) instead of a single value; list-access is the
  default across almost every component, and one call then produces N requests, index-matched to
  your input.

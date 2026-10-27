<!-- pr:begin header -->
# SlidesConnect — CLAUDE.md

Generated Grasshopper REST plugin built with the **PleaseREST** pipeline (`please-rest` plugin, v1.0.0).

- **Commands:** `/rest-status` (progress tree), `/rest-resume-plugin`, `/rest-add-feature`, `/rest-build-component <id>`, `/rest-icons`, `/rest-package`, `/rest-test-api`
- **Skills:** `pr-conventions`, `pr-component`, `pr-button`, `pr-builders`, `pr-client`, `pr-preset`, `pr-bulk-data`, `pr-unit-tests`, `pr-build`, `pr-docs`, `pr-gh-package`, `pr-yak-package`, `pr-ci-template`, `pr-rhino-test`, `pr-session`, `pr-dogfood`
- **Agents:** `pr-api-scout`, `pr-aec-advisor`, `pr-spec-critic`
- **MCPs:** `pr-api-extractor`, `pr-api-tester`, `pr-icon-generator`
- **Build:** `dotnet build SlidesConnect.csproj` (net48 / net7.0-windows / net7.0) — **close Rhino before building** (it locks the `.gha`).
- **Tests:** `dotnet test SlidesConnect.Tests/SlidesConnect.Tests.csproj` — pure builder logic, net8.0, no Rhino needed.
<!-- pr:end header -->

<!-- pr:begin state -->
## Current build state

- **Version:** 1.0.0 — release candidate; version tracking starts with the first GitHub release
- **Phase:** 15 — Package + Ship. Code, unit tests (65), `docs/`, README/LICENSE/.gitignore, `manifest.yml`, CI workflows and `dist/` packages are complete. Local git repo has the initial commit; repo https://github.com/eesha-on-jupiter/SlidesConnect exists but nothing is pushed yet.
- **Last completed step:** packaging pass — docs audited against all 62 components (+ `docs/workflows.md`, 1.0.0 changelog); csproj `<Version>1.0.0</Version>` with Release stripping and manifest/logo copy; `SlidesConnectInfo.AssemblyVersion` reads the assembly; `.github/workflows/publish.yml` (tag `v*.*.*` → 3 `.yak` + 3 zips); `dist/` holds `slidesconnect-1.0.0-{rh7_0-any,rh8_0-win,rh8_0-mac}.yak` and `SlidesConnect-1.0.0-{rh7-win,rh8-win,rh8-mac}.zip`.
- **Next step:** add the remote and push `main` (Phase 14); Gate 6 live check and Gate 7 install test (Rhino 7 Win, Rhino 8 Win, Rhino 8 Mac — Mac untested); then tag `v1.0.0`, `yak push`, Food4Rhino listing, and backfill the README / `docs/index.md` placeholders. Demo `.gh` files are user-owned (`demos/` is gitignored).
- **Last active:** 2026-10-26T11:00:00
<!-- pr:end state -->

## Orientation

Orientation for a Claude Code session picking up this repo cold. For end-user-facing component docs,
see `docs/` instead — this file is for people/agents changing the plugin's code.

## Architecture

- **`Components/<Tab>/*.cs`** — thin `GH_Component` adapters, one file per component, organized into
  folders matching the 11 Grasshopper ribbon tabs (Auth, Presentation, Slides, Layouts, Shapes, Text,
  Text Style, Tables, Insert, Aggregate, Presets). The 15 one-trait Text Style components (Bold Text,
  Font Size Text, …) subclass `Utilities/TextTraitComponent.cs`. Design rule (user-set, 2026-09-15):
  every input must be something a Grasshopper graph can drive — lists, colours, geometry, CSV values.
  Nothing may ask for a character index or anything the user would have to look up or compute; ranges
  for per-letter styling are computed inside the builders (`StyleBuilders.AssembleStyledTextRuns`).
  Build Simple Slide, Get Slide Thumbnail, Capture Rhino View and Table Row Height were removed under
  that rule; their GUIDs are in `spec/guids.json`'s `_retired` block. Each adapter reads Grasshopper inputs, calls into the pure
  builders below, and sets outputs — it should not contain request-assembly logic itself.
- **`JSON Builders/RequestBuilders.cs`**, **`StyleBuilders.cs`** (text/paragraph/shape/table styling
  specs — nullable traits, null = left out of the field mask) and **`GeometryBuilders.cs`** — all pure,
  unit-testable request-building logic. No Grasshopper or RhinoCommon dependency, so
  `SlidesConnect.Tests` can compile and run them on plain net8.0 without Rhino installed.
  Anything that assembles a Google Slides API request object or does EMU/unit conversion belongs
  here, not inline in a component's `SolveInstance`.
- **`Utilities/PluginUtilities.cs`** — shared list-access helpers (`RecordCount`, `MatchLength`,
  `WarnIfRagged`) implementing the house bulk-data convention (ragged inputs → `Min` + warn, never
  throw), plus the 11 tab category-name constants (`CategoryAAAuth` through `CategoryZYPresets`).

## Source of truth

- **`spec/plugin-spec.json`** — the authoritative component spec: every component's inputs, outputs,
  types, defaults, and descriptions, plus cross-component `relations[]` and `payload_contracts[]`.
  Keep this in sync with any component change — it's what `docs/` was generated from.
- **`spec/guids.json`** — the stable `ComponentGuid` registry. GUIDs are assigned once and **never
  regenerated**, even across renames — a component's identity in saved `.gh` files depends on it.
  Retired components move to the `_retired` block with a removal note rather than being deleted
  outright.

## Icons

`icons.json` plus the `pr-icon-generator` MCP tools (`generate_icon`, `add_to_resources`,
`generate_plugin_icon`) drive the plugin's icon set. Two known quirks:

1. The MCP server caches config content by file path — editing `icons.json` and regenerating against
   the same path can return stale results. Work around it by copying to a throwaway filename first,
   generating against that, then deleting the copy.
2. `add_to_resources` isn't idempotent against resources that already existed before a given session
   started — it can append a duplicate C# property to `Resources.Designer.cs` even though the
   underlying `.resx` entry updates correctly in place. Safest is to start from a genuinely empty
   `Resources.Designer.cs` before a bulk icon regeneration; if not, dedupe by keeping the first
   occurrence of each property name afterward.

Every component's `Icon =>` property, **and** both icon references in `SlidesConnectInfo.cs`
(`GH_AssemblyInfo.Icon` and the `AddCategoryIcon` call), must point at
`Properties.Resources.GS_<Name>` for the icon to actually show up in Rhino's ribbon — a successful
build does **not** mean the icon is wired. Check both explicitly after any icon change.

## Build and test

```
dotnet build SlidesConnect.csproj -c Debug -f net48
dotnet build SlidesConnect.csproj -c Debug -f net7.0-windows
dotnet build SlidesConnect.csproj -c Debug -f net7.0
dotnet test SlidesConnect.Tests/SlidesConnect.Tests.csproj
```

**Critical deployment note:** `dotnet build` only updates this repo's own
`bin/Debug/<target>/SlidesConnect.gha` — Rhino/Grasshopper actually loads the plugin from
`%APPDATA%\Grasshopper\Libraries\SlidesConnect.gha`, a completely separate location outside
this repo. After any build, that file must be manually redeployed (copy the net48 build there for
Rhino 7, or the net7.0-windows build plus its companion `.deps.json`/`.runtimeconfig.json` files for
Rhino 8) before changes are visible in Rhino. This has caused confusion before — a clean build with
zero errors does not mean the running plugin actually changed.

## Demos

`demos/*.gh` are binary Grasshopper archives, not text/XML — they cannot be hand-edited by a coding
agent. `demos/README.md` documents the intended wiring for each demo in prose as a substitute, and is
kept in sync with the component set even when the actual `.gh` files lag behind. No Grasshopper MCP
bridge (e.g. Cordyceps) is connected in a typical coding session — see `demos/README.md`'s
"Enabling automation" section if one becomes available.

# HANDOFF — Dorval: pill radii + navigation flash
Updated: 2026-09-04

## Where we are

Two user-reported defects. **Fix 1 (corner radii) is done and measured.** Every
`CornerRadius="999"` pill was rendering as a squished oval because the Skia renderer
reduces an oversized radius *per axis* instead of clamping it to a circle. Measured on a
26px-tall amenity chip: the top row was inset **43px** from the chip's left edge (a 43x13
ellipse). All eight sites now carry an explicit `Height` and a radius of exactly half it;
the same chip now measures **12 / 7 / 5** px inset across its first three rows, which
matches a true circular r=13 arc. A comment in `Themes/Controls.xaml` records the rule.

**Fix 2 (navigation flash) is implemented but not yet proven.** Root cause was confirmed
at runtime, not guessed: a tree dump showed `Frame 'NavigationFrame' transitions=[]` (the
content swap is a hard cut) and `AppTabBar 'Root'` sitting *inside* `TodayPage` — so the
deep-green bottom bar was torn out and rebuilt on every tab switch. The bar is now
persistent chrome in `Shell.xaml`, driven by a new `ShellChrome` singleton that follows
`IRouteNotifier`. Still open: the Frame has **no entrance transition at all**, so the
content swap above the bar is still a hard cut. That is the remaining half of the fix.

**There is temporary debug scaffolding in the tree that must come out** (see Next actions).

## Last verified state

- **Build:** pass, `net10.0-desktop`, **0 warnings / 0 errors**. Also fixed a pre-existing
  `CS0108` (`AppTabBar` field named `Content` hid `ContentControl.Content` → renamed `Hug`).
- **Runtime, confirmed by screenshot:** Today / Places / Discover / Community / Saved all
  render with the persistent bar, correct active tab, and true-pill chips. Onboarding
  correctly hides the bar (`show=False` traced).
- **Runtime, confirmed by trace:** a probe navigation Today → Places fires
  `raw='Places' dest='Places' show=True`, and across every captured frame the bottom
  chrome band stayed deep green `30,75,50` — **no flash at the bar**. But the film window
  was mis-centred: the nav landed at ~800ms into a ~900ms film, so the frames that matter
  are the last few and the content band never visibly changed. **The flash measurement is
  not yet trustworthy — re-run it before believing the fix.**
- **Git:** not a git repository. No version control; changes are on disk only.

## Next actions (in order)

1. **Re-run the transition film with the nav centred.** `Probe-Transition.ps1` (in the
   scratchpad, path under Relaunch) works end-to-end now. Set `-NavAt` so the press lands
   mid-film (e.g. film lead of ~1500ms and ~80 frames), and sample the *content* band as
   well as the bar. Look for paper `245,243,236` frames where a page should be.
2. **Add the Frame entrance transition.** The Frame is named `NavigationFrame`, reachable
   from `Shell` code-behind after navigation. Either set `ContentTransitions`, or animate
   the incoming page (opacity 0→1 ~140ms + TranslateY 8→0 ~200ms, CubicEase EaseOut per
   `xaml-design-polish`). Note pages are `NavigationCacheMode="Required"`, so verify
   `Loaded` actually fires on every visit before hanging an entrance off it.
3. **Verify the bar hides on a pushed detail page.** Not yet proven — `DORVAL_START_ROUTE=Event`
   is NOT a valid test (see Open questions). Use the nav probe to press into `Event`/`Post`
   from a root and confirm `ShowTabBar` goes false.
4. **Remove all debug scaffolding:**
   - `App.xaml.cs`: `NavProbe()` call at line ~80, the `NavProbe` method, the `Trace` method.
   - `Services/ShellChrome.cs`: `DebugNavigator` property, its assignment, the file-trace
     block inside `OnRouteChanged`.
   - Delete `%TEMP%\dorval-route.log`.
5. **Re-verify** build at 0 warnings and one clean pass over all five roots.

## Open questions

- **`DORVAL_START_ROUTE=Event` is broken, pre-existing.** Navigation to a `DataViewMap`
  route as the default nested route never completes: the page renders with *no DataContext*
  (title, date and tags all empty), `App` never reaches the line that assigns the shell's
  DataContext, and stdout logging is silent for that run. Worth fixing separately — it
  makes the debug harness lie about detail pages. Same likely applies to `Post`.
- **Two `ShellViewModel` instances exist by design** — navigation builds one for the frame,
  `App` resolves another for the shell. `AddSingleton<ShellViewModel>()` does *not* win
  (navigation re-registers it). That is why chrome state lives in `ShellChrome` instead.
  Worth deciding whether to collapse this properly.
- Should the green header also stop flashing? Only the bottom bar was hoisted; the four
  root pages each still own their own header, so the top still hard-cuts.

## Relaunch

```powershell
cd C:\Users\Platform006\UnoProjects\Dorval
.\tools\Run-Desktop.ps1 -Out .\run.png            # build + launch + screenshot
.\tools\Run-Desktop.ps1 -Out .\run.png -SkipBuild # relaunch only
```

Transition film harness (written this session, keep or fold into `tools/`):
`C:\Users\PLATFO~1\AppData\Local\Temp\claude\C--Users-Platform006-UnoProjects-Dorval\a8d362e3-632b-45d5-b296-c8e16c2bb88c\scratchpad\Probe-Transition.ps1`

**MCP note:** this project had **no `.mcp.json`** — I seeded the canonical one from
`~/.claude/templates/uno/.mcp.json` (`unoannotation`, `uno`, `uno-app`). It is read at
session start, so it did **not** take effect in the session that created it. A fresh
session rooted at `C:\Users\Platform006\UnoProjects\Dorval` gets the App MCP tools
(`uno_app_start`, `uno_app_visualtree_snapshot`, `uno_app_get_screenshot`), which replace
almost all of the hand-rolled probing above. Confirm with `/mcp` that `uno-app` is listed;
if the only Uno tools are `uno_platform_docs_*`, the server never loaded and no reconnect
will fix it — start a new session in that folder.

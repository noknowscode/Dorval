# Dorval

A city guide app for Dorval: today's picks, events to discover, places on a city map, community posts, saved items, reminders, safety information and onboarding, behind a custom tab bar.

## What's inside

- **Screens:** Community, Discover, Event Detail, Onboarding, Places, Post, Reminder, Safety, Saved, Today.
- **Custom controls:** `AppTabBar`, `CityMap`, `ColumnsPanel`, `DomeHeader`, `IconGlyph`, `MockStatusBar`, `ScreenHeader`, `SlotImage`, `WrapPanel`.

## Built with

- Uno Platform single project, `Uno.Sdk` 6.7.22 (pinned in `global.json`).
- Targets: Android, iOS, WebAssembly, Desktop (net10.0).
- Material theme, Hosting and dependency injection, Uno Toolkit, MVVM (CommunityToolkit), Navigation extensions, Skia renderer.

## Run it

You need the .NET 10 SDK. Run `uno-check` first on a fresh machine.

```bash
dotnet run --project Dorval/Dorval.csproj -f net10.0-desktop
```

## License

MIT. See [LICENSE](LICENSE).

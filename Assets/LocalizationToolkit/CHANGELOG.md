# Changelog

All notable changes to the Localization Toolkit are documented in this file.

## [2.2.0] - 2026-08-04

### Added
- CLDR plural support: `GetPlural(key, count)` with per-language rules (Polish, East Slavic, Czech/Slovak, French, Romanian, Arabic, CJK) and suffixed keys `.one/.few/.many/.other`.
- `LocalizedFont` component: per-language TMP and legacy font overrides with a size multiplier — CJK and Cyrillic ready.
- Google Sheets sync: publish a sheet as CSV and sync it into the Localization Editor with one click.
- `LoadFromWebAsync`: coroutine-based remote loading (WebGL friendly); auto-load uses it in play mode.
- `On Language Changed` UnityEvent on the manager for no-code reactions.
- `LoadData` API for injecting in-memory localization data.

## [2.1.0] - 2026-08-04

### Added
- Remember Language: the player's choice is stored in PlayerPrefs and restored on startup (saved choice → system language → default).
- AI Translation Profile asset: game description, tone of voice and a do-not-translate glossary are injected into every AI prompt.
- Translate All Languages: one click translates every language sequentially, with per-language progress, an automatic retry and a summary report.
- Statistics section with per-language completion bars and a project-wide key-usage audit (missing and unused keys, one-click fix).
- Collect Keys from four sources: loaded scenes, project prefabs, Build Settings scenes or everything.
- Pseudo-localization generator for UI overflow and glyph testing.
- Demo scene now showcases LocalizedImage with generated flag sprites.

## [2.0.0] - 2026-08-04

### Added
- `LocalizedImage` component: per-language sprite overrides for `Image` and `SpriteRenderer`, with a dedicated inspector.
- Collect Scene Keys: one click gathers the keys of every `LocalizedText` in the loaded scenes into the Localization Editor.
- AI translation in the Localization Editor: translate missing entries with Claude (Anthropic) or GPT (OpenAI) using your own API key, stored in EditorPrefs only.
- Loading localization data directly from a `TextAsset` with automatic format detection — the new recommended workflow.
- CSV support alongside JSON and XML, including a RFC 4180 compliant parser and writer.
- TextMeshPro support in `LocalizedText` and the new `LanguageDropdown` component.
- Automatic system language detection (`Detect System Language` on the `LocalizationManager`).
- `{token}` placeholder replacement via `GetLocalizedValue(key, params (token, value)[])`.
- `TryGetLocalizedValue`, `ActiveLanguage`, `IsLoaded` and configurable missing-translation text.
- Assembly definitions for the runtime and editor code.
- Edit mode test suite covering the data layer, CSV parsing and token formatting.
- Edit mode preview: language switches in the inspector refresh scene texts instantly.

### Changed
- Migrated the project to Unity 6000.5.
- All package content (runtime, editor, samples, language files, tests) now ships inside `Assets/LocalizationToolkit`.
- Rebuilt the demo scene around the TextAsset workflow with six sample languages.
- All code now lives in the `UniversityOfGames.LocalizationToolkit` namespace.
- Package restructured into `Runtime`, `Editor` and `Samples` folders.
- Remote files are downloaded with `UnityWebRequest` instead of the legacy `WebClient`.
- `DropdownChange` component renamed to `LanguageDropdown`.
- The localization editor window moved to *Tools → Localization Toolkit → Localization Editor*.
- The `OnLanguageChanged` event is now `LanguageChanged`; `AvailableExtensions` is now `LocalizationFileFormat`.
- `GetAvailableLanguages` and `GetKeys` return empty arrays instead of null.

### Removed
- Unused legacy `Resources/lang.json` sample.
- The `Assets/StreamingAssets` sample files — samples now live in `LocalizationToolkit/Samples/LanguageFiles` (loading your own StreamingAssets files is still supported).

## [1.0.0] - 2022-03-08

- Initial release: JSON and XML localization with a custom editor window.

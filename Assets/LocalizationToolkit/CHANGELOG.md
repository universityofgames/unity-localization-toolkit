# Changelog

All notable changes to the Localization Toolkit are documented in this file.

## [2.0.0] - 2026-08-04

### Added
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

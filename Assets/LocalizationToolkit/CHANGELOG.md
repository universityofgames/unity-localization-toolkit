# Changelog

All notable changes to the Localization Toolkit are documented in this file.

## [2.0.0] - 2026-08-04

### Added
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
- All code now lives in the `UniversityOfGames.LocalizationToolkit` namespace.
- Package restructured into `Runtime`, `Editor` and `Samples` folders.
- Remote files are downloaded with `UnityWebRequest` instead of the legacy `WebClient`.
- `DropdownChange` component renamed to `LanguageDropdown`.
- The localization editor window moved to *Tools → Localization Toolkit → Localization Editor*.
- The `OnLanguageChanged` event is now `LanguageChanged`; `AvailableExtensions` is now `LocalizationFileFormat`.
- `GetAvailableLanguages` and `GetKeys` return empty arrays instead of null.

### Removed
- Unused legacy `Resources/lang.json` sample.

## [1.0.0] - 2022-03-08

- Initial release: JSON and XML localization with a custom editor window.

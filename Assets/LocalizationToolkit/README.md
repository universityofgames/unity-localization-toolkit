# Localization Toolkit

A lightweight localization system for Unity 6. Keep your translations in JSON, XML or CSV files, edit them in a dedicated editor window, and let your UI update itself whenever the language changes.

## Quick Start

1. Add the **Localization Manager** component to an empty GameObject (*Add Component → Localization Toolkit → Localization Manager*).
2. Enter the name and format of a localization file placed in `Assets/StreamingAssets` (see the samples `lang.json`, `lang.xml`, `lang.csv`), or paste a remote URL.
3. Add the **Localized Text** component to any `Text` or `TextMeshPro` object and assign a translation key.
4. Optionally add the **Language Dropdown** component to a `Dropdown` or `TMP_Dropdown` to let players switch languages at runtime.

## Editing Translations

Open **Tools → Localization Toolkit → Localization Editor** to create, edit and save localization files without leaving Unity.

## Scripting

```csharp
using UniversityOfGames.LocalizationToolkit;

string title = LocalizationManager.Instance.GetLocalizedValue("hello");
string welcome = LocalizationManager.Instance.GetLocalizedValue("welcome", ("name", playerName));
LocalizationManager.Instance.LoadLanguage("Polish");
LocalizationManager.LanguageChanged += OnLanguageChanged;
```

## Support

- Website: http://www.universityofgames.net
- Email: hello@universityofgames.net
- More assets: https://assetstore.unity.com/publishers/25633

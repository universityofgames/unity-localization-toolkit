# Localization Toolkit

A lightweight localization system for Unity 6. Keep your translations in JSON, XML or CSV files, edit them in a dedicated editor window, and let your UI update itself whenever the language changes.

## Quick Start

1. Add the **Localization Manager** component to an empty GameObject (*Add Component → Localization Toolkit → Localization Manager*).
2. Drag a localization file (JSON, XML or CSV) into the **File Asset** slot — samples live in `Samples/LanguageFiles`. A `StreamingAssets` file name or a remote URL works too.
3. Add the **Localized Text** component to any `Text` or `TextMeshPro` object and assign a translation key.
4. Optionally add the **Language Dropdown** component to a `Dropdown` or `TMP_Dropdown` to let players switch languages at runtime.

Open `Samples/Demo.unity` for a ready-to-play example.

## Editing Translations

Open **Tools → Localization Toolkit → Localization Editor** to create, edit and save localization files without leaving Unity. The **AI Translation** section can fill in missing translations with Claude (Anthropic) or GPT (OpenAI) — paste your API key, pick the target language and press *Translate*. The key is stored in EditorPrefs on your machine only.

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

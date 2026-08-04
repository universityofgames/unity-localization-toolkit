![cover_banner copy](https://user-images.githubusercontent.com/10097678/163688958-3e3251eb-f506-4038-b62f-8b0e0b9663c6.png)

# Localization Toolkit for Unity

A lightweight, production-ready localization system for Unity 6. Add multi-language support to your game in minutes: keep your translations in JSON, XML or CSV files, edit them in a dedicated editor window, and let your UI update itself whenever the language changes.

<img width="701" alt="Localization Toolkit inspector" src="https://user-images.githubusercontent.com/10097678/157235530-1da24364-2858-43c5-8482-0cf5b356605c.png">

## Features

- **Three file formats** — load and save translations as JSON, XML or CSV.
- **Drag & drop setup** — assign a localization `TextAsset` directly in the inspector (format detected automatically); `StreamingAssets` files and remote URLs are also supported.
- **AI translation** — translate missing entries with Claude (Anthropic) or GPT (OpenAI) straight from the Localization Editor, using your own API key.
- **Automatic language detection** — optionally match the player's system language on startup.
- **UI Text and TextMeshPro** — `LocalizedText` works with both the legacy `Text` component and `TMP_Text`.
- **Language dropdown** — a ready-made component that lists available languages and switches between them.
- **Dynamic values** — replace `{token}` placeholders in translations straight from the API.
- **Built-in editor** — create, edit and save localization files without leaving Unity.
- **Edit mode preview** — switch languages in the inspector and see your scene texts update instantly.
- **Test coverage** — the data layer ships with an edit mode test suite.

## Requirements

- Unity **6000.5** or newer
- Packages: `com.unity.ugui`, `com.unity.nuget.newtonsoft-json` (both declared in the project manifest)

## Quick Start

1. **Add the manager** — create an empty GameObject and add the `Localization Manager` component (*Add Component → Localization Toolkit → Localization Manager*).
2. **Point it at your data** — drag a localization file (JSON, XML or CSV) into the *File Asset* slot. Sample files live in `LocalizationToolkit/Samples/LanguageFiles`. Alternatively, use a file in `Assets/StreamingAssets` or a remote URL.
3. **Localize your texts** — add the `Localized Text` component to any `Text` or `TextMeshPro` object and assign a translation key.
4. **Optional** — add the `Language Dropdown` component to a `Dropdown` or `TMP_Dropdown` to let players switch languages at runtime.

Press Play — texts are resolved automatically, and every language change refreshes all localized components. Open `LocalizationToolkit/Samples/Demo.unity` to see a working setup.

## Localization Files

Every file defines a set of languages, each mapping translation keys to values. The `default` language is used as a fallback.

**JSON**

```json
{
  "languages": {
    "default": { "hello": "Hello", "bye": "Bye" },
    "Polish":  { "hello": "Cześć", "bye": "Pa!" }
  }
}
```

**XML**

```xml
<?xml version="1.0" encoding="utf-8"?>
<translations>
  <default>
    <hello>Hello</hello>
    <bye>Bye</bye>
  </default>
  <Polish>
    <hello>Cześć</hello>
    <bye>Pa!</bye>
  </Polish>
</translations>
```

**CSV**

```csv
key,default,Polish
hello,Hello,Cześć
bye,Bye,Pa!
```

## Localization Editor

Open **Tools → Localization Toolkit → Localization Editor** to create new localization data, load an existing file (local or remote), add or remove languages and keys, and save the result in any supported format.

<img width="695" alt="Localization editor window" src="https://user-images.githubusercontent.com/10097678/157235561-144d2190-4257-4b67-870a-2a4bd93be797.png">

## AI Translation

The Localization Editor includes an **AI Translation** section that fills in missing translations for the selected language using an AI model:

1. Load or create localization data and add the target language.
2. Pick a provider — **Claude (Anthropic)** or **GPT (OpenAI)** — and paste your API key.
3. Select the target language in the grid and press **Translate With AI**.

Only empty entries are translated by default (enable *Overwrite Existing* to retranslate everything), keys and `{token}` placeholders are preserved, and you can review every value in the grid before saving. The API key is stored in `EditorPrefs` on your machine only — it is never written to project files or builds.

## Scripting API

```csharp
using UniversityOfGames.LocalizationToolkit;

// Read a translation for the active language
string title = LocalizationManager.Instance.GetLocalizedValue("hello");

// Replace {token} placeholders with dynamic values
string welcome = LocalizationManager.Instance.GetLocalizedValue(
    "welcome_player", ("name", playerName), ("level", level.ToString()));

// Switch the language at runtime
LocalizationManager.Instance.LoadLanguage("Polish");

// React to language changes
LocalizationManager.LanguageChanged += () => Debug.Log("Language switched!");
```

## Project Structure

Everything ships inside a single folder:

```
Assets/LocalizationToolkit/
├── Editor/
│   ├── Translation/   AI translation (Claude / GPT) integration
│   └── ...            Custom inspectors and the Localization Editor window
├── Runtime/
│   ├── Core/          LocalizationManager, LocalizationData, file formats
│   ├── Components/    LocalizedText, LanguageDropdown
│   └── Utilities/     Web loading, CSV parsing, token formatting
├── Samples/
│   ├── Demo.unity     Ready-to-play demo scene
│   └── LanguageFiles/ Sample translations (JSON, XML, CSV)
└── Tests/             Edit mode test suite
```

* * *

### Check our other Unity packages
➡️ You can also find our other solutions on the **[Unity Asset Store](https://assetstore.unity.com/publishers/25633)**

* * *

### Need help?

University of Games is a research center - great place for indie game developers and young publishers. Thanks to more than 10+ years of experience in the industry, we provide knowledge and solutions in the area of technology research, game design, marketing, consulting and business advisory.

To learn more, you can check our blog:
- http://www.universityofgames.net

Our social media:
- https://www.facebook.com/uniwesytetgier
- http://www.twitter.com/uniwersytetgier
- http://www.instagram.com/uniwersytetgier

Contact us directly via email:
- hello@universityofgames.net

If you have any questions or issues with your Unity project(s), feel free to contact!

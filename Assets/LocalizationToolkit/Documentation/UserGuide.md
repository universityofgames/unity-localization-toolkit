# Localization Toolkit — User Guide

Version 2.0.0 · Requires Unity 6000.5 or newer · Support: hello@universityofgames.net

Localization Toolkit is a lightweight, production-ready localization system. Keep your
translations in JSON, XML or CSV files, edit and AI-translate them in a dedicated editor
window, and let your UI update itself whenever the language changes.

---

## 1. Installation & Requirements

1. Import the package into your project. All content lives in a single folder:
   `Assets/LocalizationToolkit`.
2. Make sure these packages are present (both are part of the default Unity setup):
   - `com.unity.ugui` — UI components (includes TextMeshPro),
   - `com.unity.nuget.newtonsoft-json` — JSON serialization.

The `Tests` folder is optional at runtime — it compiles only in the editor when the
Unity Test Framework is installed. You can exclude it from builds-only workflows freely.

## 2. Quick Start (60 seconds)

1. Create an empty GameObject and add **Localization Toolkit → Localization Manager**.
2. Drag a localization file into the **File Asset** slot — start with
   `Samples/LanguageFiles/lang.json`.
3. Add **Localization Toolkit → Localized Text** to any `Text` or TextMeshPro object
   and enter a translation key (for the sample file: `title`, `welcome`, `description`,
   `language_label` or `bye`).
4. Optionally add **Localization Toolkit → Language Dropdown** to a `Dropdown` or
   `TMP_Dropdown` — it fills itself with the available languages.
5. Press Play. Open `Samples/Demo.unity` at any time to see a complete working setup.

## 3. Components

### 3.1 Localization Manager

The central component; add exactly one per scene.

| Field | Description |
|---|---|
| **File Asset** | A JSON, XML or CSV `TextAsset`. The format is detected automatically. Recommended source. |
| **File URL** | Remote `.json`, `.xml` or `.csv` file downloaded on startup. Used when no file asset is set. |
| **File Name / File Format** | Name (without extension) and format of a file in `Assets/StreamingAssets`. Used when neither of the above is set. |
| **Detect System Language** | Selects the player's system language after loading (falls back to `default`). |
| **Missing Translation Text** | Text returned for unknown keys. |

The inspector also offers *Load* buttons for every source and a language selector for
previewing languages — directly in edit mode.

### 3.2 Localized Text

Keeps a `Text` or `TMP_Text` component in sync with one translation key. Refreshes
automatically on every language change; in the editor it previews language switches live.

### 3.3 Language Dropdown

Fills a `Dropdown` or `TMP_Dropdown` with all available languages, pre-selects the active
one and switches the language when the player picks another entry. No wiring required.

## 4. Localization Files

Every file defines a set of languages, each mapping translation keys to values. The
language named **`default`** is the fallback and the source of the key list in the
editor tooling — always include it.

**JSON**

```json
{
  "languages": {
    "default": { "hello": "Hello" },
    "Polish":  { "hello": "Cześć" }
  }
}
```

**XML**

```xml
<?xml version="1.0" encoding="utf-8"?>
<translations>
  <default><hello>Hello</hello></default>
  <Polish><hello>Cześć</hello></Polish>
</translations>
```

**CSV** (RFC 4180 — values with commas, quotes or line breaks are quoted)

```csv
key,default,Polish
hello,Hello,Cześć
```

Language names should match Unity's `SystemLanguage` names (`English`, `Polish`,
`German`, ...) so that automatic system-language detection can find them.

## 5. Localization Editor Window

Open **Tools → Localization Toolkit → Localization Editor**.

- **Localization Data** — load a file asset, a local file (`Open File...`) or a remote
  URL; create new data; save in any format (`Save As...` + *Save Format*).
- **Languages** — choose the edited language, add languages from the `SystemLanguage`
  list, or remove one (with confirmation).
- **AI Translation** — see section 6.
- **Entries** — the key/value table for the edited language, with a search filter,
  entry counter and per-row removal. Key renames propagate to every language.

## 6. AI Translation

Translate entries of the edited language straight from the editor window using your own
AI provider account:

1. Pick a **Provider**: *Claude (Anthropic)* or *GPT (OpenAI)*.
2. Paste your **API Key** (created in the provider's console) and optionally adjust the
   **Model** — sensible defaults are pre-filled.
3. Select the target language as the *Edited Language* and press
   **Translate '<language>' With AI**.

Behavior and guarantees:

- Only **empty** entries are translated by default; enable *Overwrite Existing* to
  retranslate everything.
- Keys and `{token}` placeholders are preserved exactly.
- Results land in the entries table for review — nothing is saved until you save.
- The API key is stored in `EditorPrefs` on your machine only. It is **never** written
  to project files, version control or builds. Requests are sent directly from the
  editor to the provider's official API endpoint.

Troubleshooting: a `401`/`403` response means an invalid or unauthorized key; `429`
means you hit your provider's rate limit; an "empty response" error usually means the
model refused — try again or switch models.

## 7. Scripting API

All types live in the `UniversityOfGames.LocalizationToolkit` namespace.

```csharp
using UniversityOfGames.LocalizationToolkit;

// Read a translation for the active language
string title = LocalizationManager.Instance.GetLocalizedValue("title");

// Safe lookup without the missing-translation placeholder
if (LocalizationManager.Instance.TryGetLocalizedValue("title", out string value)) { /* ... */ }

// Replace {token} placeholders with dynamic values
string welcome = LocalizationManager.Instance.GetLocalizedValue(
    "welcome_player", ("name", playerName), ("level", level.ToString()));

// Switch the language at runtime
LocalizationManager.Instance.LoadLanguage("Polish");

// React to language changes (unsubscribe in OnDisable)
LocalizationManager.LanguageChanged += OnLanguageChanged;

// Enumerate data
string[] languages = LocalizationManager.Instance.GetAvailableLanguages();
string[] keys = LocalizationManager.Instance.GetKeys();

// Load data from code
LocalizationManager.Instance.LoadFromTextAsset(myTextAsset);
LocalizationManager.Instance.LoadFromWeb("https://example.com/lang.json");
LocalizationManager.Instance.LoadFromFile(path, LocalizationFileFormat.Csv);
```

Working with files directly:

```csharp
LocalizationData data = LocalizationData.Parse(rawText, LocalizationFileFormat.Json);
string csv = data.ToCsv();
```

## 8. FAQ

**Texts show "Localized text not found".** The key does not exist in the active
language. Check the key spelling and make sure the `default` language contains it.
The placeholder text is configurable on the manager.

**Which text components are supported?** Legacy `UnityEngine.UI.Text`, `TMP_Text`
(TextMeshPro UGUI and 3D), `Dropdown` and `TMP_Dropdown`.

**Does remote loading work on WebGL?** No — blocking downloads are not available on
WebGL. Use a `TextAsset` (recommended) on that platform.

**Where should my own localization files live?** Anywhere in `Assets` when using the
File Asset workflow, or in `Assets/StreamingAssets` when loading by file name.

**Can I keep using my old v1.x files?** Yes — the JSON and XML formats are unchanged.

## 9. Support

- Documentation: https://github.com/universityofgames/unity-localization-toolkit#readme
- Email: hello@universityofgames.net
- More assets: https://assetstore.unity.com/publishers/25633

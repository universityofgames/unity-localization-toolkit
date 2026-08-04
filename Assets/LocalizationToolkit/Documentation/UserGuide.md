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

New to the package? Open **Tools → Localization Toolkit → Welcome** — it creates a
configured manager, opens the demo scenes and links every guide from one window.

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

### Sample scenes

| Scene | Shows |
|---|---|
| `Samples/Demo.unity` | The full overview: localized texts, flag image, language dropdown |
| `Samples/Demo_Plurals.unity` | CLDR plural forms with a live counter (`GetPlural`) |
| `Samples/Demo_Formatting.unity` | Culture-aware numbers, currency and dates (`FormatLocalized`) |
| `Samples/Demo_Objects.unity` | `LocalizedObject` switching whole layouts per language |
| `Samples/Demo_Audio.unity` | `LocalizedAudio` playing a different clip per language |

## 3. Components

### 3.1 Localization Manager

The central component; add exactly one per scene.

| Field | Description |
|---|---|
| **File Asset** | A JSON, XML or CSV `TextAsset`. The format is detected automatically. Recommended source. |
| **File URL** | Remote `.json`, `.xml` or `.csv` file downloaded on startup. Used when no file asset is set. |
| **File Name / File Format** | Name (without extension) and format of a file in `Assets/StreamingAssets`. Used when neither of the above is set. |
| **Detect System Language** | Selects the player's system language after loading (falls back to `default`). |
| **Remember Language** | Stores the player's choice in `PlayerPrefs` and restores it on startup. The saved choice wins over system-language detection. |
| **Missing Translation Text** | Text returned for unknown keys. |

The inspector also offers *Load* buttons for every source and a language selector for
previewing languages — directly in edit mode.

### 3.2 Localized Text

Keeps a `Text` or `TMP_Text` component in sync with one translation key. Refreshes
automatically on every language change; in the editor it previews language switches live.

### 3.3 Language Dropdown

Fills a `Dropdown` or `TMP_Dropdown` with all available languages, pre-selects the active
one and switches the language when the player picks another entry. No wiring required.

### 3.4 Localized Font

Swaps the font of a `TMP_Text` or legacy `Text` component per language — essential for
CJK, Cyrillic, Thai or Arabic, whose glyphs are usually missing from Latin font assets.
Add an override per language (TMP font asset and/or legacy font, plus an optional size
multiplier for dense scripts); languages without an override use the default font,
captured automatically when the default fields are left empty.

### 3.5 Localized Audio

Swaps the clip of an `AudioSource` per language — for localized voice-overs and spoken
tutorials. Assign a default clip and per-language overrides; when the language changes
during playback, the new clip restarts from the beginning.

### 3.6 Localized Object

Keeps exactly one GameObject active per language — for language-specific layouts or
decorations that go beyond a text or sprite swap. Configure one object per language
plus a default; activation runs on enable and on every language change (play mode).

### 3.7 Localized Image

Swaps the sprite of an `Image` or `SpriteRenderer` to match the active language — for
localized logos, flags, banners or any artwork containing text. Assign a **Default
Sprite** and add an override per language that needs different artwork; languages
without an override fall back to the default. The inspector offers a language popup per
override (fed by the loaded localization data), and language switches preview live in
the Scene view.

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

### Plural forms

Store plural variants as suffixed keys and read them with `GetPlural(key, count)` —
the correct CLDR category is picked per language, `{count}` is always available:

```json
"Polish": {
  "apples.one":  "{count} jabłko",
  "apples.few":  "{count} jabłka",
  "apples.many": "{count} jabłek"
}
```

```csharp
LocalizationManager.Instance.GetPlural("apples", 5); // "5 jabłek"
```

Lookup falls back from the exact category to `.other` and finally to the bare key, so
languages without plural distinctions (Japanese, Chinese, ...) only need `.other` or a
plain entry. The suffixes `.zero .one .two .few .many .other` are reserved — avoid them
in ordinary key names.

### Culture-aware formatting

`FormatLocalized` formats `{token:format}` placeholders with the active language's
culture — standard .NET format strings apply:

```json
"stats_line": "Score: {score:N0} · Reward: {reward:C} · Today: {date:d}"
```

```csharp
manager.FormatLocalized("stats_line", ("score", 987654), ("reward", 49.99), ("date", DateTime.Now));
// Polish → "Wynik: 987 654 · Nagroda: 49,99 zł · Dziś: 04.08.2026"
```

### Right-to-left languages

The manager keeps a list of right-to-left languages (Arabic and Hebrew by default).
While one is active, `LocalizedText` enables TextMeshPro's RTL mode and mirrors left/
right alignment automatically. Note: full Arabic glyph shaping is TextMeshPro's
responsibility — use a font asset with Arabic support.

### Google Sheets

Keep translations in a shared spreadsheet: in Google Sheets choose
*File → Share → Publish to web*, pick **Comma-separated values (.csv)** and copy the
link. Paste it into the **Google Sheet URL** field of the Localization Editor and press
**Sync** whenever the sheet changes. The first column must be `key`, followed by one
column per language (same layout as the CSV format above).

## 5. Localization Editor Window

Open **Tools → Localization Toolkit → Localization Editor**.

- **Localization Data** — load a file asset, a local file (`Open File...`) or a remote
  URL; create new data; save in any format (`Save As...` + *Save Format*).
- **Languages** — choose the edited language, add languages from the `SystemLanguage`
  list, or remove one (with confirmation). **Generate Pseudo Language** creates a
  `Pseudo` test language: accented characters reveal missing glyphs, ~30% padding
  reveals layouts that break on long translations, and ⟦brackets⟧ reveal truncation.
  `{token}` placeholders are preserved. Delete the language before shipping, or keep
  it — it is inert at runtime.
- **AI Translation** — see section 6.
- **Statistics** — per-language completion bars, plus **Scan Project For Key Usage**:
  compares the keys referenced by `Localized Text` components in prefabs and Build
  Settings scenes against the loaded data, listing keys that are missing from the data
  (with a one-click fix) and keys that are never used.
- **Entries** — the key/value table for the edited language, with a search filter,
  entry counter and per-row removal. Key renames propagate to every language.
  **Collect Keys** gathers `Localized Text` keys from your choice of sources — loaded
  scenes, project prefabs, Build Settings scenes or everything — and adds the missing
  ones to the table: set the keys up in your UI first, collect them with one click,
  then fill in (or AI-translate) the values.

## 6. AI Translation

Translate entries of the edited language straight from the editor window using your own
AI provider account:

1. Pick a **Provider**: *Claude (Anthropic)* or *GPT (OpenAI)*.
2. Paste your **API Key** (created in the provider's console) and optionally adjust the
   **Model** — sensible defaults are pre-filled.
3. Select the target language as the *Edited Language* and press
   **Translate '<language>' With AI**.

For production-quality output, create an **AI Translation Profile**
(*Assets → Create → Localization Toolkit → AI Translation Profile*) and assign it in
the AI section. The profile feeds every prompt with your game's description, the tone
of voice and a glossary of terms that must never be translated (proper names, stats
like *XP* or *Mana*). Use **Translate All Languages** to fill every language in one
run — languages are translated sequentially with a progress bar, a failed language is
retried once automatically, and a summary reports what happened per language.

Behavior and guarantees:

- Only **empty** entries are translated by default; enable *Overwrite Existing* to
  retranslate everything.
- Keys and `{token}` placeholders are preserved exactly; glossary terms are kept verbatim.
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

// Plurals (CLDR rules per language, {count} token built in)
string apples = LocalizationManager.Instance.GetPlural("apples", 5);

// Load data from code
LocalizationManager.Instance.LoadFromTextAsset(myTextAsset);
LocalizationManager.Instance.LoadFromWeb("https://example.com/lang.json");          // blocking (editor tooling)
LocalizationManager.Instance.LoadFromWebAsync("https://example.com/lang.json",      // coroutine, WebGL friendly
    success => Debug.Log("Loaded: " + success));
LocalizationManager.Instance.LoadFromFile(path, LocalizationFileFormat.Csv);
LocalizationManager.Instance.LoadData(myLocalizationData);
```

Designers can also react to language switches without code through the manager's
**On Language Changed** UnityEvent in the inspector.

Working with files directly:

```csharp
LocalizationData data = LocalizationData.Parse(rawText, LocalizationFileFormat.Json);
string csv = data.ToCsv();
```

## 8. FAQ

**Texts show "Localized text not found".** The key does not exist in the active
language. Check the key spelling and make sure the `default` language contains it.
The placeholder text is configurable on the manager.

**Which components are supported?** Legacy `UnityEngine.UI.Text`, `TMP_Text`
(TextMeshPro UGUI and 3D), `Dropdown`, `TMP_Dropdown`, and — for localized artwork —
`Image` and `SpriteRenderer` via `Localized Image`.

**Does remote loading work on WebGL?** Yes — use `LoadFromWebAsync` (the automatic
startup load already uses it in play mode). Only the blocking `LoadFromWeb` is
unavailable on WebGL.

**Where should my own localization files live?** Anywhere in `Assets` when using the
File Asset workflow, or in `Assets/StreamingAssets` when loading by file name.

**Can I keep using my old v1.x files?** Yes — the JSON and XML formats are unchanged.

## 9. Support

- Documentation: https://github.com/universityofgames/unity-localization-toolkit#readme
- Email: hello@universityofgames.net
- More assets: https://assetstore.unity.com/publishers/25633

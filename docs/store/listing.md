# Asset Store Listing — Localization Toolkit

Working copy for the Unity Asset Store product page. Everything below is ready to
paste into the publisher portal.

## Title

**Localization Toolkit — AI Translation, Plurals & More**

## Tagline (short description)

Localize your game in minutes. JSON/XML/CSV, one-click AI translation (Claude/GPT),
CLDR plurals, per-language fonts and sprites, Google Sheets sync and a full editor.

## Long description

**Ship your game in 10 languages, not 1.** Localization Toolkit is a complete,
lightweight localization system with the fastest workflow on the store: drop a
language file on a component, add Localized Text to your UI and press play.

**Translate everything with AI.** Connect your own Anthropic (Claude) or OpenAI (GPT)
API key and translate all languages with one click. A project profile feeds the AI
your game's description, tone of voice and a do-not-translate glossary — so "Mana"
stays "Mana" and the tone fits your game. Only missing entries are translated, every
result lands in a review table first, and your key never leaves your machine.

**Grammar done right.** CLDR plural rules give every language its correct forms —
1 apple / 2 apples, 1 jabłko / 2 jabłka / 5 jabłek — with a one-line API:
`GetPlural("apples", count)`. Numbers, dates and currency format themselves in the
active language's culture: `{price:C}`, `{date:d}`.

**Localize more than text.** Swap sprites (logos, flags), fonts (CJK, Cyrillic),
audio clips (voice-overs) and whole GameObjects per language — each with its own
drag-and-drop component and live edit-mode preview.

**Stay in control.** The Localization Editor window shows per-language completion
bars, audits which keys your scenes actually use, collects keys from your whole
project with one click, generates a pseudo-locale to catch UI overflow, and syncs
with a published Google Sheet.

### Feature list

- JSON, XML and CSV localization files (format auto-detected)
- AI translation: Claude (Anthropic) & GPT (OpenAI), batch mode, glossary, tone control
- CLDR plural rules: Polish, Russian, Czech, Arabic, French, Romanian, CJK and more
- Culture-aware number/date/currency formatting per language
- Components: Localized Text (UI Text + TextMeshPro), Localized Image, Localized Font,
  Localized Audio, Localized Object, Language Dropdown
- Right-to-left support for TextMeshPro (Arabic, Hebrew)
- Automatic system-language detection + remembered player choice (PlayerPrefs)
- Localization Editor: statistics, key audit, key collection, pseudo-localization,
  Google Sheets sync, search and filters
- Remote loading: blocking or async (WebGL friendly)
- Full XML-documented API, edit mode test suite, single-folder package
- Demo scenes covering every feature

### Technical details

- Unity 6000.5+, built-in render pipeline agnostic (pure UI/C#)
- Dependencies: com.unity.ugui, com.unity.nuget.newtonsoft-json (default in Unity 6)
- AI translation is editor-only; API keys are stored in EditorPrefs, never in assets
  or builds; requests go directly to the provider's official endpoint
- Full source code included

## Keywords

localization, localisation, translation, i18n, language, multi-language, AI translation,
ChatGPT, Claude, plurals, CSV, Google Sheets, TextMeshPro, RTL, font, translate

## Screenshot shot list

1. Localization Editor with data loaded — statistics bars visible (hero shot).
2. AI Translation section with profile assigned and the batch summary dialog.
3. Demo scene in Polish with the flag, texts and open dropdown.
4. Localized Image / Localized Font inspectors with language popups.
5. Pseudo-locale in the demo scene (visible overflow markers).
6. Google Sheets side by side with the synced editor table.

## GIF storyboard (main media)

1. (0-3 s) Demo scene running in English.
2. (3-8 s) Dropdown opens; language switches to Polish, then German — texts and flag
   change live.
3. (8-14 s) Cut to the Localization Editor: "Translate All Languages" pressed,
   progress bar runs, summary dialog appears.
4. (14-18 s) Statistics bars fill to 100%; back to the game, switching two more
   languages. End card with the logo and tagline.

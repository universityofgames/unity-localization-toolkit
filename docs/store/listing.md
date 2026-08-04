# Asset Store Listing — Localization Toolkit

Ready-to-paste copy for the Unity Asset Store publisher portal, structured exactly
like the portal form fields.

## Summary (max 180 characters — currently 173)

Complete localization for Unity 6: one-click AI translation (Claude/GPT), CLDR plurals, localized sprites, fonts and audio, Google Sheets sync and a full in-editor workflow.

## Description

**Ship your game in 10 languages, not 1.** Localization Toolkit is a complete,
lightweight localization system with the fastest workflow on the store: drop a
language file (JSON, XML or CSV) on the Localization Manager, add Localized Text to
your UI and press Play. The player's system language is detected automatically and
their choice is remembered between sessions.

**Translate everything with AI.** Connect your own Anthropic (Claude) or OpenAI (GPT)
API key and translate every language with one click. A project profile feeds the AI
your game's description, tone of voice and a do-not-translate glossary — so "Mana"
stays "Mana" and the tone fits your game. Only missing entries are translated, every
result lands in a review table before you save, and your key never leaves your
machine.

**Grammar done right.** CLDR plural rules give every language its correct forms —
1 apple / 2 apples, 1 jabłko / 2 jabłka / 5 jabłek — with a one-line API:
GetPlural("apples", count). Numbers, dates and currency format themselves in the
active language's culture: {price:C}, {date:d}.

**Localize more than text.** Swap sprites (logos, flags), fonts (CJK, Cyrillic),
audio clips (voice-overs) and whole GameObjects per language — each with its own
drag-and-drop component and live edit-mode preview. TextMeshPro and legacy UI Text
are both supported, including right-to-left flipping for Arabic and Hebrew.

**Stay in control.** The Localization Editor window shows per-language completion
bars, audits which keys your scenes actually use, collects keys from your whole
project with one click, generates a pseudo-locale that catches UI overflow before
your players do, and syncs with a published Google Sheet.

Works with any genre and any render pipeline — it is pure UI and C#. Five sample
scenes, a full user guide and the complete, XML-documented source code are included.

-- -- --

University of Games is a small research center for indie game developers — a place
dedicated to sharing passion, knowledge and adventures in the game industry. We want
it to be a home of in-depth explanations and ready-to-use solutions that help you
solve problems and grow. Your support lets us create even more content for
developers, students and teachers all over the world.

## Technical details

- Three file formats: JSON, XML and CSV, with automatic format detection
- AI translation (editor-only): Claude (Anthropic) and GPT (OpenAI), batch mode over all languages, glossary and tone control via profile assets; API keys stay in EditorPrefs, never in assets or builds
- CLDR plural rules: English-like, Polish, Russian/Ukrainian, Czech/Slovak, French, Romanian, Arabic (all six forms) and CJK
- Culture-aware {token:format} formatting: numbers, currency and dates per language
- Components: Localized Text, Localized Image, Localized Font, Localized Audio, Localized Object, Language Dropdown — all with custom inspectors and live edit-mode preview
- TextMeshPro and legacy UI Text support, including RTL direction and mirrored alignment
- Automatic system-language detection plus remembered player choice (PlayerPrefs)
- Localization Editor: completion statistics, key-usage audit, project-wide key collection, pseudo-localization generator, search and empty-value filters
- Google Sheets sync: publish a sheet as CSV and update your data with one click
- Remote loading: blocking or coroutine-based async (WebGL friendly)
- Loads from TextAsset (recommended), StreamingAssets or any URL; runtime data injection via LoadData
- Five sample scenes: overview, plurals, formatting, per-language objects and audio
- 119 edit mode tests, fully XML-documented public API, single-folder package
- Requires Unity 6000.5+; dependencies: com.unity.ugui, com.unity.nuget.newtonsoft-json (default in Unity 6)

## Keywords

localization, localisation, translation, i18n, language, multi-language, AI translation,
ChatGPT, Claude, plurals, CSV, Google Sheets, TextMeshPro, RTL, font, translate

## Screenshots

Rendered promo shots live in `docs/store/screenshots/` (1920×1080 PNG):

| File | Content |
|---|---|
| `00_hero.png` | Hero banner: title, tagline, feature list, flag row |
| `01_demo_english.png` / `02_demo_polish.png` / `03_demo_german.png` | Demo scene in three languages (texts + flag switch) |
| `04_pseudo_locale.png` | Pseudo-locale revealing UI overflow |
| `05_plurals_polish.png` | CLDR plurals: "5 jabłek" with the counter |
| `06_formatting_polish.png` | Culture-aware score/currency/date formatting |
| `07_objects_polish.png` | LocalizedObject switching a whole layout |
| `08_audio_french.png` | LocalizedAudio voice-over scene |

Still to capture manually in the editor (GUI-only):

1. Localization Editor with data loaded — statistics bars visible.
2. AI Translation section with a profile assigned and the batch summary dialog.
3. Localized Image / Localized Font inspectors with language popups.
4. Google Sheets side by side with the synced editor table.

## GIF storyboard (main media)

1. (0-3 s) Demo scene running in English.
2. (3-8 s) Dropdown opens; language switches to Polish, then German — texts and flag
   change live.
3. (8-14 s) Cut to the Localization Editor: "Translate All Languages" pressed,
   progress bar runs, summary dialog appears.
4. (14-18 s) Statistics bars fill to 100%; back to the game, switching two more
   languages. End card with the logo and tagline.

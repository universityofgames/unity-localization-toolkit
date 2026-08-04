# Localization Toolkit — Development Roadmap

Working plan for the 2.1 → 2.3 releases. Each phase ships as a separate set of commits
with tests and documentation; the package version in `LocalizationToolkitInfo` and the
changelog are bumped once per release.

Legend: ⏱ rough effort · ✅ acceptance criteria

---

## Release 2.1 — "Localize in minutes" (Tier 1)

### Phase 1 — Language persistence ⏱ 0.5 d

Runtime, `LocalizationManager`:
- New serialized toggle `_rememberLanguage` (default ON) + `RememberLanguage` property.
- `LoadLanguage` stores the chosen key in `PlayerPrefs` (`UoG.LocalizationToolkit.Language`)
  — only in play mode, never from edit-mode previews.
- Startup order in `ApplyData`: saved language (if enabled and present in data)
  → system language (if detection enabled) → `default`.
- Resolution order extracted to an internal static
  `ResolveStartupLanguage(saved, system, detectSystem, available)` so it is unit-testable
  without touching `PlayerPrefs`.

✅ Tests for every branch of the resolution order; inspector shows the toggle under
*Behaviour*; user guide section "Remembering the player's choice".

### Phase 2 — AI 2.0: game context, glossary, batch translate ⏱ 1 d

Editor:
- New `LocalizationAiProfile` ScriptableObject (`Create → Localization Toolkit → AI Profile`):
  game description (`TextArea`), tone (e.g. "casual, playful"), `DoNotTranslate` term list,
  free-form extra instructions.
- `AiTranslator.BuildPrompt` gains an optional profile parameter: context block +
  "never translate these terms" list injected into the prompt.
- Editor window: object field for the profile (remembered per project in `EditorPrefs`)
  in the AI Translation section.
- **Translate All Languages** button: sequential queue over every language except the
  key source; per-language progress ("Polish · 2/7 languages · 14 entries"); cancel
  stops after the current language; one retry with backoff on HTTP 429; summary dialog
  (translated/skipped/failed per language).

✅ Prompt-builder tests (profile block present, glossary terms listed, no profile = old
prompt); manual smoke test with a real key; guide chapter "Production-quality AI
translation" (profile + batch + troubleshooting).

### Phase 3 — Audit tools: statistics, full key collection, pseudo-locale ⏱ 1 d

Editor window:
- **Statistics** section (collapsible box): per-language completion — filled/total,
  percentage, `EditorGUI.ProgressBar`; click a language to select it as edited language
  with the search filter set to empty values.
- **Key audit**: scan build-settings scenes + all prefabs for `LocalizedText` keys;
  report "used in content but missing from data" (button: add them) and "in data but
  never used" (informational — removal stays manual).
- **Collect Scene Keys 2.0**: sources = loaded scenes / all build-settings scenes /
  all prefabs (dropdown or checkboxes). Scene scanning opens scenes one by one and
  restores the previous scene setup via `SceneManagerSetup`; asks for confirmation and
  for saving dirty scenes first.
- **Pseudo-localization**: "Generate Pseudo Language" creates/updates a `Pseudo`
  language from the key-source values — accented substitution (a→á, e→é, ...), ~30%
  padding, `⟦...⟧` wrapping. Pure static `PseudoLocalizer.Generate(string)`.

✅ Tests: `PseudoLocalizer` (substitution, padding ratio, brackets, token `{...}`
preservation), audit set logic extracted to a pure helper and covered; guide chapter
"Auditing your localization" + pseudo-locale how-to (incl. note to delete the language
before shipping, or keep — it is inert).

### Phase 4 — Demo upgrade ⏱ 0.5 d

- Generate six simple flag sprites programmatically (color stripes, no licensing risk)
  into `Samples/Sprites/`, wire a `LocalizedImage` flag into the demo scene next to the
  dropdown (builder-script pattern as before).
- Optional second sample: a small script showing `GetLocalizedValue` with tokens.

✅ Demo shows text + image localization; screenshots refreshed for the listing.

**Release 2.1 exit checklist:** version bump, changelog, full test pass, demo rebuild,
user guide updated, commits per phase (emoji style, no attribution).

---

## Release 2.2 — "Serious localization" (Tier 2)

### Phase 5 — CLDR plurals ⏱ 1–1.5 d

Runtime:
- `PluralCategory { Zero, One, Two, Few, Many, Other }` +
  `PluralRules.Resolve(languageKey, count)` — data-driven rule table covering the
  shipped rule families: English-like (en/de/es/it/pt/nl/sv/...), Polish,
  Russian/Ukrainian, Czech/Slovak, French, Romanian, Arabic (full six), CJK (Other
  only). Unknown language → English-like; always falls back to `Other`.
- File format: suffixed keys `apples.one`, `apples.few`, `apples.many`, `apples.other`.
  Lookup chain: exact category → `.other` → bare key → missing-text placeholder.
- API: `GetPlural(key, count)` and `GetPlural(key, count, params tokens)` with an
  implicit `{count}` token.

✅ Rule tests per family (PL: 1/2/5/12/22/25; RU; AR incl. 0/1/2/11/100; fallbacks);
guide chapter with a Polish example table; sample entries in `lang.json`.

### Phase 6 — LocalizedFont ⏱ 0.5–1 d

- Component: per-language entries { language, `TMP_FontAsset`, legacy `Font`, optional
  size multiplier }; captures the original font as default; targets `TMP_Text` or
  `Text`; refreshes on `LanguageChanged`, ExecuteAlways preview; custom inspector with
  the language-popup pattern from `LocalizedImageEditor`.

✅ Edit-mode tests for font resolution; guide section "Fonts for CJK and Cyrillic".

### Phase 7 — Google Sheets sync ⏱ 0.5 d

- Editor window: per-project remembered Sheet URL + **Sync from Google Sheets**
  (download published-CSV → replace data) and **Open Sheet** buttons.
- Guide: step-by-step "File → Share → Publish to web → CSV" with screenshots.

✅ Works against a real published sheet; URL survives editor restarts.

### Phase 8 — Async remote loading + designer hooks ⏱ 0.5–1 d

- `LoadFromWebAsync(url, Action<bool> onCompleted = null)` — coroutine-based, works on
  WebGL; blocking `LoadFromWeb` stays for editor tooling. Auto-load uses the async path
  in play mode.
- Instance-level `UnityEvent onLanguageChanged` on the manager (static C# event stays).

✅ Play-mode test or manual WebGL check; docs updated (WebGL limitation removed).

---

## Release 2.3 — Ecosystem & store push (Tier 3)

### Phase 9 — Culture-aware formatting + RTL groundwork ⏱ 1 d
- `SystemLanguage → CultureInfo` map; token formatting `{price:C}`, `{date:d}` applied
  with the active language's culture inside `ApplyTokens`.
- RTL: per-language "right-to-left" flag; `LocalizedText` sets TMP `isRightToLeftText`
  and mirrors alignment. Full Arabic shaping is explicitly out of scope — documented.

### Phase 10 — LocalizedAudio & LocalizedObject ⏱ 0.5 d
- `LocalizedAudio`: `AudioClip` per language for an `AudioSource` (+ `GetClipForLanguage`).
- `LocalizedObject`: activates configured GameObjects only for selected languages.

### Phase 11 — Setup Wizard ⏱ 0.5 d
- One-time welcome window (`EditorPrefs` flag): create a configured manager in the
  scene, open the demo, open the guide, open the Localization Editor.

### Phase 12 — CI ⏱ 0.5 d
- GitHub Actions with game-ci `unity-test-runner` (EditMode) on push/PR; README badge;
  activation steps for the Unity license secret documented for the org.

### Phase 13 — Store listing package ⏱ 0.5 d
- `docs/store/` (outside `Assets/`): listing description, tagline, keyword set,
  screenshot shot-list, GIF storyboard ("dropdown switches six languages live",
  "Translate All Languages in action").

---

## Cross-cutting rules

- Every phase: edit-mode tests + user-guide update + changelog entry; batch-mode
  verification (`-runTests`) must be green with zero compiler warnings before commit.
- Public API only grows — no breaking renames within the 2.x line.
- Commit style: emoji + English title + bullet body; one phase = 1–3 commits.
- Version bumps: 2.1.0 / 2.2.0 / 2.3.0 in `LocalizationToolkitInfo.Version` + changelog.

## Known risks & decisions taken

| Risk | Mitigation |
|---|---|
| Batch AI hits provider rate limits | Sequential queue, single retry with backoff on 429, per-language failure reporting |
| Scene scanning is destructive | `SceneManagerSetup` save/restore + explicit confirmation dialog |
| Pseudo language shipped to players | Documented; harmless at runtime (regular language entry) |
| Plural suffixes vs dots in user keys | `.one/.few/.many/.other/.zero/.two` documented as reserved suffixes |
| `PlayerPrefs` key collisions | Fixed documented key `UoG.LocalizationToolkit.Language` |
| RTL expectations | Scope limited to TMP RTL flag + alignment; shaping documented as out of scope |

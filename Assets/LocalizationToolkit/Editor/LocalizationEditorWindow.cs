using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>
	/// Editor window for creating, editing, AI-translating and saving localization
	/// data in any of the supported file formats (JSON, XML, CSV).
	/// </summary>
	public class LocalizationEditorWindow : EditorWindow
	{
		private const string DefaultKeyName = "NEW_KEY";
		private const float RemoveButtonWidth = 50f;
		private const float MinTextFieldWidth = 300f;
		private const float FromRightOffset = 50f;
		private const float EnumWidth = 350f;
		private const float SideButtonWidth = 80f;
		private const int ColumnCount = 2;

		private const string ApiKeyPrefsPrefix = "UniversityOfGames.LocalizationToolkit.ApiKey.";
		private const string ModelPrefsPrefix = "UniversityOfGames.LocalizationToolkit.Model.";
		private const string ProfilePrefsPrefix = "UniversityOfGames.LocalizationToolkit.AiProfile.";
		private const string SheetsPrefsPrefix = "UniversityOfGames.LocalizationToolkit.SheetUrl.";

		private LocalizationData _data = new LocalizationData();
		private LocalizationFileFormat _fileFormat;
		private TextAsset _fileAsset;
		private string _fileUrl = string.Empty;
		private string _sheetsUrl = string.Empty;

		private string[] _languageNames = Array.Empty<string>();
		private string[] _languagesAvailableToAdd = Array.Empty<string>();
		private int _selectedLanguageIndex;
		private int _languageToAddIndex;
		private string _searchFilter = string.Empty;
		private bool _showOnlyEmpty;

		private bool _showAiTranslation = true;
		private bool _showStatistics;
		private List<string> _auditMissingKeys;
		private List<string> _auditUnusedKeys;
		private AiTranslationProvider _aiProvider;
		private string _aiModel = string.Empty;
		private string _aiApiKey = string.Empty;
		private bool _aiOverwriteExisting;
		private LocalizationAiProfile _aiProfile;

		private Vector2 _scrollPosition;
		private string _lastEditedKey;
		private bool _needsRefocus;
		private TextEditor _textEditor;
		private int _lastCursorPosition;
		private int _lastSelectCursorPosition;

		private bool HasData => _data?.Languages != null && _data.Languages.Count > 0;

		private string KeySourceLanguage =>
			_data.Languages.ContainsKey(LocalizationManager.DefaultLanguageKey)
				? LocalizationManager.DefaultLanguageKey
				: _data.Languages.Keys.First();

		[MenuItem("Tools/Localization Toolkit/Localization Editor")]
		private static void Open()
		{
			var window = GetWindow<LocalizationEditorWindow>("Localization Editor");
			window.minSize = new Vector2(700f, 560f);
			window.Show();
		}

		private void OnEnable()
		{
			LoadAiPreferences();
			_sheetsUrl = EditorPrefs.GetString(SheetsPrefsPrefix + PlayerSettings.productGUID, string.Empty);
		}

		private void OnGUI()
		{
			GUILayout.Space(6);
			DrawDataSection();
			DrawLanguagesSection();
			DrawAiTranslationSection();
			DrawStatisticsSection();
			DrawEntriesSection();
			GUILayout.Space(6);
		}

		// --- Data source -------------------------------------------------------

		private void DrawDataSection()
		{
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField("Localization Data", EditorStyles.boldLabel);

				using (new EditorGUILayout.HorizontalScope())
				{
					_fileAsset = (TextAsset)EditorGUILayout.ObjectField(
						new GUIContent("File Asset", "A JSON, XML or CSV localization file. The format is detected automatically."),
						_fileAsset, typeof(TextAsset), false);
					using (new EditorGUI.DisabledScope(_fileAsset == null))
					{
						if (GUILayout.Button("Load", GUILayout.Width(SideButtonWidth)))
							LoadFromAsset();
					}
				}

				using (new EditorGUILayout.HorizontalScope())
				{
					_fileUrl = EditorGUILayout.TextField(
						new GUIContent("File URL", "Remote .json, .xml or .csv file to download."), _fileUrl);
					using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_fileUrl)))
					{
						if (GUILayout.Button("Load", GUILayout.Width(SideButtonWidth)))
							LoadFromWeb();
					}
				}

				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUI.BeginChangeCheck();
					_sheetsUrl = EditorGUILayout.TextField(
						new GUIContent("Google Sheet URL", "URL of a sheet published as CSV (File → Share → Publish to web → CSV)."),
						_sheetsUrl);
					if (EditorGUI.EndChangeCheck())
						EditorPrefs.SetString(SheetsPrefsPrefix + PlayerSettings.productGUID, _sheetsUrl.Trim());

					using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_sheetsUrl)))
					{
						if (GUILayout.Button("Sync", GUILayout.Width(SideButtonWidth)))
							SyncFromGoogleSheets();
						if (GUILayout.Button("Open", GUILayout.Width(50f)))
							Application.OpenURL(_sheetsUrl.Trim());
					}
				}

				GUILayout.Space(4);

				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("Open File...", GUILayout.Height(22f)))
						LoadFromFile();
					if (GUILayout.Button("Create New", GUILayout.Height(22f)))
						CreateNewData();
					using (new EditorGUI.DisabledScope(!HasData))
					{
						if (GUILayout.Button("Save As...", GUILayout.Height(22f)))
							SaveToFile();
					}
				}

				_fileFormat = (LocalizationFileFormat)EditorGUILayout.EnumPopup(
					new GUIContent("Save Format", "Format used by 'Save As...'."), _fileFormat);
			}
		}

		private void LoadFromAsset()
		{
			try
			{
				ResetSelection();
				_data = LocalizationData.Parse(_fileAsset.text, LocalizationFileFormatUtility.DetectFormat(_fileAsset.text));
			}
			catch (Exception exception)
			{
				EditorUtility.DisplayDialog("Localization Toolkit",
					$"Could not parse '{_fileAsset.name}':\n{exception.Message}", "OK");
			}
		}

		private void LoadFromWeb()
		{
			if (!RemoteFileLoader.TryGetFileFormatFromUrl(_fileUrl, out LocalizationFileFormat format))
			{
				EditorUtility.DisplayDialog("Localization Toolkit",
					"The URL must point to a .json, .xml or .csv file.", "OK");
				return;
			}

			string rawData = RemoteFileLoader.DownloadText(_fileUrl);
			if (string.IsNullOrEmpty(rawData))
				return;

			try
			{
				ResetSelection();
				_data = LocalizationData.Parse(rawData, format);
			}
			catch (Exception exception)
			{
				EditorUtility.DisplayDialog("Localization Toolkit",
					$"Could not parse the downloaded file:\n{exception.Message}", "OK");
			}
		}

		private void SyncFromGoogleSheets()
		{
			string rawData = RemoteFileLoader.DownloadText(_sheetsUrl.Trim());
			if (string.IsNullOrEmpty(rawData))
			{
				EditorUtility.DisplayDialog("Google Sheets",
					"The sheet could not be downloaded. Make sure it is published to the web (File → Share → Publish to web → CSV) and the URL is correct.",
					"OK");
				return;
			}

			try
			{
				ResetSelection();
				_data = LocalizationData.Parse(rawData, LocalizationFileFormatUtility.DetectFormat(rawData));
				EditorUtility.DisplayDialog("Google Sheets",
					$"Synced {_data.Languages.Count} language(s) with {_data.Languages[KeySourceLanguage].Count} key(s) from the sheet.",
					"OK");
			}
			catch (Exception exception)
			{
				EditorUtility.DisplayDialog("Google Sheets",
					$"Could not parse the downloaded sheet:\n{exception.Message}", "OK");
			}
		}

		private void LoadFromFile()
		{
			string filePath = EditorUtility.OpenFilePanelWithFilters(
				"Open localization file", Application.dataPath,
				new[] { "Localization files", "json,xml,csv", "JSON", "json", "XML", "xml", "CSV", "csv" });
			if (string.IsNullOrEmpty(filePath))
				return;

			if (!LocalizationFileFormatUtility.TryParseExtension(Path.GetExtension(filePath), out LocalizationFileFormat format))
			{
				EditorUtility.DisplayDialog("Localization Toolkit",
					"Unsupported file type. Choose a .json, .xml or .csv file.", "OK");
				return;
			}

			try
			{
				ResetSelection();
				_data = LocalizationData.Parse(File.ReadAllText(filePath), format);
				_fileFormat = format;
			}
			catch (Exception exception)
			{
				EditorUtility.DisplayDialog("Localization Toolkit",
					$"Could not parse '{Path.GetFileName(filePath)}':\n{exception.Message}", "OK");
			}
		}

		private void SaveToFile()
		{
			string filePath = EditorUtility.SaveFilePanel(
				"Save localization data file", Application.dataPath, "lang", _fileFormat.ToExtension());
			if (string.IsNullOrEmpty(filePath))
				return;

			File.WriteAllText(filePath, _data.Serialize(_fileFormat));
			AssetDatabase.Refresh();
		}

		private void CreateNewData()
		{
			ResetSelection();
			_data = new LocalizationData(LocalizationManager.DefaultLanguageKey, DefaultKeyName);
		}

		private void ResetSelection()
		{
			_selectedLanguageIndex = 0;
			_languageToAddIndex = 0;
			_searchFilter = string.Empty;
		}

		// --- Languages ---------------------------------------------------------

		private void DrawLanguagesSection()
		{
			if (!HasData)
				return;

			_languageNames = _data.Languages.Keys.ToArray();
			_selectedLanguageIndex = Mathf.Clamp(_selectedLanguageIndex, 0, _languageNames.Length - 1);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField($"Languages ({_languageNames.Length})", EditorStyles.boldLabel);

				using (new EditorGUILayout.HorizontalScope())
				{
					_selectedLanguageIndex = EditorGUILayout.Popup(
						new GUIContent("Edited Language", "Language shown in the value column of the table below."),
						_selectedLanguageIndex, _languageNames);

					bool isKeySource = _languageNames[_selectedLanguageIndex] == LocalizationManager.DefaultLanguageKey;
					using (new EditorGUI.DisabledScope(isKeySource))
					{
						if (GUILayout.Button("Remove", GUILayout.Width(SideButtonWidth)))
							RemoveSelectedLanguage();
					}
				}

				using (new EditorGUILayout.HorizontalScope())
				{
					_languagesAvailableToAdd = GetLanguagesAvailableToAdd();
					if (_languageToAddIndex >= _languagesAvailableToAdd.Length && _languagesAvailableToAdd.Length > 0)
						_languageToAddIndex = 0;

					_languageToAddIndex = EditorGUILayout.Popup("Add Language", _languageToAddIndex, _languagesAvailableToAdd);
					using (new EditorGUI.DisabledScope(_languagesAvailableToAdd.Length == 0))
					{
						if (GUILayout.Button("Add", GUILayout.Width(SideButtonWidth)))
							AddNewLanguage();
					}
				}

				if (GUILayout.Button(new GUIContent("Generate Pseudo Language",
					"Create a 'Pseudo' language with accented, padded text to test UI overflow and missing glyphs.")))
					GeneratePseudoLanguage();
			}
		}

		private void GeneratePseudoLanguage()
		{
			Dictionary<string, string> keySource = _data.Languages[KeySourceLanguage];
			var pseudo = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> entry in keySource)
				pseudo[entry.Key] = PseudoLocalizer.Generate(entry.Value);

			bool existed = _data.Languages.ContainsKey(PseudoLocalizer.LanguageKey);
			_data.Languages[PseudoLocalizer.LanguageKey] = pseudo;

			EditorUtility.DisplayDialog("Pseudo Localization",
				$"{(existed ? "Updated" : "Created")} the '{PseudoLocalizer.LanguageKey}' language with {pseudo.Count} entries. " +
				"Select it as the edited language and preview it in your scenes to spot overflowing layouts.", "OK");
			Repaint();
		}

		private string[] GetLanguagesAvailableToAdd()
		{
			return Enum.GetNames(typeof(SystemLanguage))
				.Where(language => !_data.Languages.ContainsKey(language))
				.ToArray();
		}

		private void AddNewLanguage()
		{
			string language = _languagesAvailableToAdd[_languageToAddIndex];
			var table = new Dictionary<string, string>();
			foreach (string key in _data.Languages[KeySourceLanguage].Keys)
				table[key] = string.Empty;

			_data.Languages[language] = table;
		}

		private void RemoveSelectedLanguage()
		{
			string language = _languageNames[_selectedLanguageIndex];
			if (language == LocalizationManager.DefaultLanguageKey)
				return;

			if (!EditorUtility.DisplayDialog("Remove Language",
				$"Remove '{language}' and all of its translations?", "Remove", "Cancel"))
				return;

			_data.Languages.Remove(language);
			_selectedLanguageIndex = 0;
		}

		// --- AI translation ----------------------------------------------------

		private void DrawAiTranslationSection()
		{
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showAiTranslation = EditorGUILayout.Foldout(_showAiTranslation, "AI Translation", true, EditorStyles.foldoutHeader);
				if (!_showAiTranslation)
					return;

				EditorGUI.BeginChangeCheck();
				_aiProvider = (AiTranslationProvider)EditorGUILayout.Popup(
					new GUIContent("Provider"), (int)_aiProvider,
					new[] { AiTranslationProvider.Claude.GetDisplayName(), AiTranslationProvider.OpenAi.GetDisplayName() });
				if (EditorGUI.EndChangeCheck())
					LoadAiPreferences();

				_aiModel = EditorGUILayout.TextField(
					new GUIContent("Model", "Model identifier sent to the provider."), _aiModel);
				_aiApiKey = EditorGUILayout.PasswordField(
					new GUIContent("API Key", "Stored in EditorPrefs on this machine only."), _aiApiKey);
				_aiOverwriteExisting = EditorGUILayout.Toggle(
					new GUIContent("Overwrite Existing", "Retranslate entries that already have a value."),
					_aiOverwriteExisting);
				_aiProfile = (LocalizationAiProfile)EditorGUILayout.ObjectField(
					new GUIContent("AI Profile", "Optional: game context, tone and do-not-translate glossary. Create via Assets → Create → Localization Toolkit → AI Translation Profile."),
					_aiProfile, typeof(LocalizationAiProfile), false);

				EditorGUILayout.HelpBox(
					"Translates entries of the edited language from the key source language. " +
					"The API key is stored in EditorPrefs on this machine only - it is never written to project files or builds.",
					MessageType.Info);

				if (!HasData)
				{
					EditorGUILayout.HelpBox("Load or create localization data to enable AI translation.", MessageType.Warning);
					return;
				}

				string sourceLanguage = KeySourceLanguage;
				string targetLanguage = _languageNames.Length > 0
					? _languageNames[Mathf.Clamp(_selectedLanguageIndex, 0, _languageNames.Length - 1)]
					: string.Empty;

				if (targetLanguage == sourceLanguage)
				{
					EditorGUILayout.HelpBox(
						$"'{sourceLanguage}' is the key source language. Select a different edited language above to translate into it.",
						MessageType.Warning);
				}

				using (new EditorGUILayout.HorizontalScope())
				{
					using (new EditorGUI.DisabledScope(
						string.IsNullOrWhiteSpace(_aiApiKey) || targetLanguage == sourceLanguage || string.IsNullOrEmpty(targetLanguage)))
					{
						if (GUILayout.Button($"Translate '{targetLanguage}'", GUILayout.Height(26f)))
							TranslateSelectedLanguage(sourceLanguage, targetLanguage);
					}

					using (new EditorGUI.DisabledScope(
						string.IsNullOrWhiteSpace(_aiApiKey) || _languageNames.Length < 2))
					{
						if (GUILayout.Button("Translate All Languages", GUILayout.Height(26f)))
							TranslateAllLanguages(sourceLanguage);
					}
				}

				if (string.IsNullOrWhiteSpace(_aiApiKey))
					EditorGUILayout.HelpBox("Enter your API key to enable translation.", MessageType.None);
			}
		}

		private void TranslateSelectedLanguage(string sourceLanguage, string targetLanguage)
		{
			SaveAiPreferences();

			Dictionary<string, string> entries = BuildEntriesToTranslate(sourceLanguage, targetLanguage);
			if (entries.Count == 0)
			{
				EditorUtility.DisplayDialog("AI Translation",
					$"There is nothing to translate: every '{targetLanguage}' entry already has a value. " +
					"Enable 'Overwrite Existing' to retranslate them.", "OK");
				return;
			}

			AiTranslationStatus status = AiTranslator.TranslateEntries(
				_aiProvider, _aiApiKey.Trim(), ResolveModel(), sourceLanguage, targetLanguage, entries,
				out Dictionary<string, string> translations, _aiProfile);

			if (status != AiTranslationStatus.Success)
				return;

			int applied = ApplyTranslations(sourceLanguage, targetLanguage, translations);
			Repaint();
			Debug.Log($"[LocalizationToolkit] Applied {applied} AI translations to '{targetLanguage}'. Remember to save the data.");
		}

		private void TranslateAllLanguages(string sourceLanguage)
		{
			SaveAiPreferences();

			string model = ResolveModel();
			var summary = new System.Text.StringBuilder();
			bool cancelled = false;

			foreach (string targetLanguage in _languageNames)
			{
				if (targetLanguage == sourceLanguage)
					continue;

				Dictionary<string, string> entries = BuildEntriesToTranslate(sourceLanguage, targetLanguage);
				if (entries.Count == 0)
				{
					summary.AppendLine($"{targetLanguage}: already complete");
					continue;
				}

				AiTranslationStatus status = AiTranslator.TranslateEntries(
					_aiProvider, _aiApiKey.Trim(), model, sourceLanguage, targetLanguage, entries,
					out Dictionary<string, string> translations, _aiProfile);

				if (status == AiTranslationStatus.Failed)
				{
					// One retry with a short backoff, e.g. after a rate limit response.
					System.Threading.Thread.Sleep(2000);
					status = AiTranslator.TranslateEntries(
						_aiProvider, _aiApiKey.Trim(), model, sourceLanguage, targetLanguage, entries,
						out translations, _aiProfile);
				}

				if (status == AiTranslationStatus.Cancelled)
				{
					cancelled = true;
					summary.AppendLine($"{targetLanguage}: cancelled");
					break;
				}

				if (status == AiTranslationStatus.Failed)
				{
					summary.AppendLine($"{targetLanguage}: FAILED (see the Console for details)");
					continue;
				}

				int applied = ApplyTranslations(sourceLanguage, targetLanguage, translations);
				summary.AppendLine($"{targetLanguage}: {applied} entries translated");
			}

			Repaint();
			EditorUtility.DisplayDialog("AI Translation",
				(cancelled ? "Batch translation cancelled.\n\n" : "Batch translation finished.\n\n") + summary +
				"\nRemember to save the data.", "OK");
		}

		private Dictionary<string, string> BuildEntriesToTranslate(string sourceLanguage, string targetLanguage)
		{
			Dictionary<string, string> sourceTable = _data.Languages[sourceLanguage];
			Dictionary<string, string> targetTable = _data.Languages[targetLanguage];

			var entries = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> entry in sourceTable)
			{
				if (string.IsNullOrWhiteSpace(entry.Value))
					continue;

				if (!_aiOverwriteExisting && targetTable.TryGetValue(entry.Key, out string existing) && !string.IsNullOrWhiteSpace(existing))
					continue;

				entries[entry.Key] = entry.Value;
			}

			return entries;
		}

		private int ApplyTranslations(string sourceLanguage, string targetLanguage, Dictionary<string, string> translations)
		{
			Dictionary<string, string> sourceTable = _data.Languages[sourceLanguage];
			Dictionary<string, string> targetTable = _data.Languages[targetLanguage];

			int applied = 0;
			foreach (KeyValuePair<string, string> translation in translations)
			{
				if (!sourceTable.ContainsKey(translation.Key))
					continue;

				targetTable[translation.Key] = translation.Value;
				applied++;
			}

			return applied;
		}

		private string ResolveModel()
		{
			return string.IsNullOrWhiteSpace(_aiModel) ? _aiProvider.GetDefaultModel() : _aiModel.Trim();
		}

		private void LoadAiPreferences()
		{
			_aiApiKey = EditorPrefs.GetString(ApiKeyPrefsPrefix + _aiProvider, string.Empty);
			_aiModel = EditorPrefs.GetString(ModelPrefsPrefix + _aiProvider, _aiProvider.GetDefaultModel());

			if (_aiProfile == null)
			{
				string guid = EditorPrefs.GetString(ProfilePrefsPrefix + PlayerSettings.productGUID, string.Empty);
				if (!string.IsNullOrEmpty(guid))
					_aiProfile = AssetDatabase.LoadAssetAtPath<LocalizationAiProfile>(AssetDatabase.GUIDToAssetPath(guid));
			}
		}

		private void SaveAiPreferences()
		{
			EditorPrefs.SetString(ApiKeyPrefsPrefix + _aiProvider, _aiApiKey.Trim());
			EditorPrefs.SetString(ModelPrefsPrefix + _aiProvider, string.IsNullOrWhiteSpace(_aiModel) ? _aiProvider.GetDefaultModel() : _aiModel.Trim());

			string profilePath = _aiProfile != null ? AssetDatabase.GetAssetPath(_aiProfile) : string.Empty;
			EditorPrefs.SetString(ProfilePrefsPrefix + PlayerSettings.productGUID,
				string.IsNullOrEmpty(profilePath) ? string.Empty : AssetDatabase.AssetPathToGUID(profilePath));
		}

		// --- Statistics & audit --------------------------------------------------

		private void DrawStatisticsSection()
		{
			if (!HasData)
				return;

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_showStatistics = EditorGUILayout.Foldout(_showStatistics, "Statistics", true, EditorStyles.foldoutHeader);
				if (!_showStatistics)
					return;

				foreach (LocalizationAudit.LanguageStatistics statistics in
					LocalizationAudit.GetStatistics(_data, KeySourceLanguage))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField(statistics.Language, GUILayout.Width(150f));
						Rect barRect = GUILayoutUtility.GetRect(120f, 16f, GUILayout.ExpandWidth(true));
						EditorGUI.ProgressBar(barRect, statistics.Completion,
							$"{statistics.Filled}/{statistics.Total}  ({statistics.Completion:P0})");

						if (GUILayout.Button(new GUIContent("Edit",
							"Select this language in the table, filtered to its empty entries."), GUILayout.Width(44f)))
						{
							_selectedLanguageIndex = System.Array.IndexOf(_languageNames, statistics.Language);
							_searchFilter = string.Empty;
							_showOnlyEmpty = statistics.Filled < statistics.Total;
							GUI.FocusControl(null);
						}
					}
				}

				GUILayout.Space(6);

				if (GUILayout.Button(new GUIContent("Scan Project For Key Usage",
					"Compare the keys used by LocalizedText components in prefabs and Build Settings scenes with the loaded data.")))
					ScanProjectForKeyUsage();

				if (_auditMissingKeys != null && _auditUnusedKeys != null)
				{
					EditorGUILayout.LabelField(
						$"Used in content but missing from data: {_auditMissingKeys.Count}   ·   In data but never used: {_auditUnusedKeys.Count}",
						EditorStyles.miniBoldLabel);

					if (_auditMissingKeys.Count > 0)
					{
						EditorGUILayout.HelpBox("Missing: " + Summarize(_auditMissingKeys), MessageType.Warning);
						if (GUILayout.Button("Add Missing Keys To Table"))
						{
							AddKeysToAllLanguages(_auditMissingKeys);
							_auditMissingKeys = new List<string>();
							Repaint();
						}
					}

					if (_auditUnusedKeys.Count > 0)
						EditorGUILayout.HelpBox("Unused (informational): " + Summarize(_auditUnusedKeys), MessageType.None);
				}
			}
		}

		private void ScanProjectForKeyUsage()
		{
			if (!EditorUtility.DisplayDialog("Scan Project",
				"This scans every prefab and opens every enabled scene from Build Settings, then restores your current scene setup. Continue?",
				"Scan", "Cancel"))
				return;

			HashSet<string> contentKeys = LocalizedTextKeyScanner.CollectFromPrefabs();
			HashSet<string> sceneKeys = LocalizedTextKeyScanner.CollectFromBuildScenes();
			if (sceneKeys == null)
				return;

			contentKeys.UnionWith(sceneKeys);
			contentKeys.UnionWith(LocalizedTextKeyScanner.CollectFromLoadedScenes());

			_auditMissingKeys = LocalizationAudit.GetKeysMissingFromData(contentKeys, _data, KeySourceLanguage);
			_auditUnusedKeys = LocalizationAudit.GetUnusedKeys(contentKeys, _data, KeySourceLanguage);
		}

		private static string Summarize(List<string> keys)
		{
			const int limit = 40;
			string joined = string.Join(", ", keys.Take(limit));
			return keys.Count > limit ? joined + $", … and {keys.Count - limit} more" : joined;
		}

		private void AddKeysToAllLanguages(IEnumerable<string> keys)
		{
			foreach (string key in keys)
			{
				foreach (Dictionary<string, string> table in _data.Languages.Values)
				{
					if (!table.ContainsKey(key))
						table[key] = string.Empty;
				}
			}
		}

		// --- Entries -----------------------------------------------------------

		private void DrawEntriesSection()
		{
			if (!HasData)
			{
				EditorGUILayout.HelpBox(
					"No localization data is loaded. Load a file asset, open a file, download one from a URL or create new data to get started.",
					MessageType.Info);
				return;
			}

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				int totalKeys = _data.Languages[KeySourceLanguage].Count;

				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField($"Entries ({totalKeys})", EditorStyles.boldLabel, GUILayout.Width(150f));
					GUILayout.FlexibleSpace();
					_showOnlyEmpty = GUILayout.Toggle(_showOnlyEmpty,
						new GUIContent("Only Empty", "Show only keys whose value is empty in the edited language."),
						EditorStyles.miniButton, GUILayout.Width(80f));
					_searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(200f));
					if (GUILayout.Button(new GUIContent("Collect Keys ▾",
						"Scan LocalizedText components and add their keys to the table."), GUILayout.Width(110f)))
						ShowCollectKeysMenu();
					if (GUILayout.Button("Add Entry", GUILayout.Width(80f)))
						AddNewEntry();
				}

				GUILayout.Space(4);

				float spacePerColumn = (position.width - RemoveButtonWidth - FromRightOffset) / ColumnCount;
				DrawColumnLabels(spacePerColumn);
				DrawLocalizationGrid(spacePerColumn);
			}
		}

		private void DrawColumnLabels(float spacePerColumn)
		{
			string valueLanguage = _languageNames.Length > 0
				? _languageNames[Mathf.Clamp(_selectedLanguageIndex, 0, _languageNames.Length - 1)]
				: string.Empty;

			GUILayout.BeginHorizontal();
			GUILayout.Label("Key", EditorStyles.miniBoldLabel, GUILayout.MinWidth(MinTextFieldWidth), GUILayout.MaxWidth(spacePerColumn));
			GUILayout.Label($"Value ({valueLanguage})", EditorStyles.miniBoldLabel, GUILayout.MinWidth(MinTextFieldWidth), GUILayout.MaxWidth(spacePerColumn));
			GUILayout.EndHorizontal();
		}

		private void DrawLocalizationGrid(float spacePerColumn)
		{
			var keysToReplace = new Dictionary<string, string>();
			var keysToRemove = new List<string>();

			string keySourceLanguage = KeySourceLanguage;
			int totalKeys = _data.Languages[keySourceLanguage].Count;

			string filterLanguage = _languageNames[Mathf.Clamp(_selectedLanguageIndex, 0, _languageNames.Length - 1)];
			var localizationKeys = _data.Languages[keySourceLanguage].Keys.ToList();
			localizationKeys.Sort();
			if (!string.IsNullOrEmpty(_searchFilter))
			{
				localizationKeys = localizationKeys
					.Where(key => key.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
					.ToList();
			}

			if (_showOnlyEmpty)
			{
				Dictionary<string, string> filterTable = _data.Languages[filterLanguage];
				localizationKeys = localizationKeys
					.Where(key => !filterTable.TryGetValue(key, out string value) || string.IsNullOrWhiteSpace(value))
					.ToList();
			}

			// Full copy so keys hidden by the search filter are never lost on reassignment.
			var editedLanguages = new Dictionary<string, Dictionary<string, string>>();
			foreach (KeyValuePair<string, Dictionary<string, string>> language in _data.Languages)
				editedLanguages[language.Key] = new Dictionary<string, string>(language.Value);

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

			string selectedLanguage = _languageNames[Mathf.Clamp(_selectedLanguageIndex, 0, _languageNames.Length - 1)];
			int elementId = 0;
			foreach (string key in localizationKeys)
			{
				GUILayout.BeginHorizontal();
				GUI.SetNextControlName(elementId.ToString());
				string newKey = GUILayout.TextField(
					key, GUILayout.MinWidth(MinTextFieldWidth), GUILayout.MaxWidth(spacePerColumn));

				if (key != newKey)
				{
					_lastEditedKey = newKey;
					_needsRefocus = true;
					keysToReplace[key] = newKey;
					_textEditor = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
				}

				_data.Languages[selectedLanguage].TryGetValue(key, out string currentValue);
				editedLanguages[selectedLanguage][key] = GUILayout.TextField(
					currentValue ?? string.Empty,
					GUILayout.MinWidth(MinTextFieldWidth), GUILayout.MaxWidth(spacePerColumn));

				if (GUILayout.Button("-", GUILayout.Width(RemoveButtonWidth)))
				{
					if (totalKeys > 1)
						keysToRemove.Add(key);
					else
						Debug.LogError("[LocalizationToolkit] Translation data needs at least one key.");
				}

				GUILayout.EndHorizontal();
				elementId++;
			}

			EditorGUILayout.EndScrollView();

			if (GUI.changed)
			{
				_data.Languages = editedLanguages;
				ReplaceKeys(keysToReplace);
				RemoveKeys(keysToRemove);
			}
			else
			{
				RestoreFocusIfNeeded(localizationKeys);
			}

			if (_textEditor != null)
			{
				_lastCursorPosition = _textEditor.cursorIndex;
				_lastSelectCursorPosition = _textEditor.selectIndex;
			}
		}

		private void ReplaceKeys(Dictionary<string, string> keysToReplace)
		{
			foreach (KeyValuePair<string, string> replacement in keysToReplace)
			{
				foreach (Dictionary<string, string> table in _data.Languages.Values)
				{
					if (table.ContainsKey(replacement.Value) || !table.TryGetValue(replacement.Key, out string value))
						continue;

					table.Remove(replacement.Key);
					table[replacement.Value] = value;
				}
			}
		}

		private void RemoveKeys(List<string> keysToRemove)
		{
			foreach (string key in keysToRemove)
			{
				foreach (Dictionary<string, string> table in _data.Languages.Values)
					table.Remove(key);
			}
		}

		private void ShowCollectKeysMenu()
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("From Loaded Scenes"), false,
				() => CollectKeys(LocalizedTextKeyScanner.CollectFromLoadedScenes(), "the loaded scenes"));
			menu.AddItem(new GUIContent("From Project Prefabs"), false,
				() => CollectKeys(LocalizedTextKeyScanner.CollectFromPrefabs(), "the project prefabs"));
			menu.AddItem(new GUIContent("From Build Settings Scenes"), false,
				() => CollectKeysFromBuildScenes(false));
			menu.AddItem(new GUIContent("From Everything"), false,
				() => CollectKeysFromBuildScenes(true));
			menu.ShowAsContext();
		}

		private void CollectKeysFromBuildScenes(bool includePrefabs)
		{
			if (!EditorUtility.DisplayDialog("Collect Keys",
				"This opens every enabled scene from Build Settings and then restores your current scene setup. Continue?",
				"Collect", "Cancel"))
				return;

			HashSet<string> keys = LocalizedTextKeyScanner.CollectFromBuildScenes();
			if (keys == null)
				return;

			if (includePrefabs)
				keys.UnionWith(LocalizedTextKeyScanner.CollectFromPrefabs());
			keys.UnionWith(LocalizedTextKeyScanner.CollectFromLoadedScenes());

			CollectKeys(keys, includePrefabs ? "the whole project" : "the Build Settings scenes");
		}

		private void CollectKeys(HashSet<string> contentKeys, string sourceLabel)
		{
			List<string> missing = LocalizationAudit.GetKeysMissingFromData(contentKeys, _data, KeySourceLanguage);
			AddKeysToAllLanguages(missing);

			EditorUtility.DisplayDialog("Collect Keys",
				missing.Count > 0
					? $"Added {missing.Count} new key(s) out of {contentKeys.Count} found in {sourceLabel}."
					: $"No new keys found. {contentKeys.Count} key(s) from {sourceLabel} are already in the table.",
				"OK");
			Repaint();
		}

		private void AddNewEntry()
		{
			string key = GetUniqueKeyName();
			foreach (Dictionary<string, string> table in _data.Languages.Values)
				table[key] = string.Empty;
		}

		private string GetUniqueKeyName()
		{
			Dictionary<string, string> keySource = _data.Languages[KeySourceLanguage];
			string key = DefaultKeyName;
			int iteration = 0;
			while (keySource.ContainsKey(key))
			{
				key = DefaultKeyName + "_" + iteration;
				iteration++;
			}

			return key;
		}

		private void RestoreFocusIfNeeded(List<string> localizationKeys)
		{
			if (!_needsRefocus)
				return;

			_needsRefocus = false;
			int id = localizationKeys.IndexOf(_lastEditedKey);
			GUI.FocusControl(id.ToString());
			_textEditor = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;

			if (_textEditor != null)
			{
				_textEditor.OnFocus();
				_textEditor.cursorIndex = _lastCursorPosition;
				_textEditor.selectIndex = _lastSelectCursorPosition;
			}
		}
	}
}

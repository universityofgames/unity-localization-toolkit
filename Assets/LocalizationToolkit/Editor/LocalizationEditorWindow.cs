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

		private LocalizationData _data = new LocalizationData();
		private LocalizationFileFormat _fileFormat;
		private TextAsset _fileAsset;
		private string _fileUrl = string.Empty;

		private string[] _languageNames = Array.Empty<string>();
		private string[] _languagesAvailableToAdd = Array.Empty<string>();
		private int _selectedLanguageIndex;
		private int _languageToAddIndex;
		private string _searchFilter = string.Empty;

		private bool _showAiTranslation = true;
		private AiTranslationProvider _aiProvider;
		private string _aiModel = string.Empty;
		private string _aiApiKey = string.Empty;
		private bool _aiOverwriteExisting;

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
		}

		private void OnGUI()
		{
			GUILayout.Space(6);
			DrawDataSection();
			DrawLanguagesSection();
			DrawAiTranslationSection();
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
			}
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

				using (new EditorGUI.DisabledScope(
					string.IsNullOrWhiteSpace(_aiApiKey) || targetLanguage == sourceLanguage || string.IsNullOrEmpty(targetLanguage)))
				{
					if (GUILayout.Button($"Translate '{targetLanguage}' With AI", GUILayout.Height(26f)))
						TranslateSelectedLanguage(sourceLanguage, targetLanguage);
				}

				if (string.IsNullOrWhiteSpace(_aiApiKey))
					EditorGUILayout.HelpBox("Enter your API key to enable translation.", MessageType.None);
			}
		}

		private void TranslateSelectedLanguage(string sourceLanguage, string targetLanguage)
		{
			SaveAiPreferences();

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

			if (entries.Count == 0)
			{
				EditorUtility.DisplayDialog("AI Translation",
					$"There is nothing to translate: every '{targetLanguage}' entry already has a value. " +
					"Enable 'Overwrite Existing' to retranslate them.", "OK");
				return;
			}

			string model = string.IsNullOrWhiteSpace(_aiModel) ? _aiProvider.GetDefaultModel() : _aiModel.Trim();
			Dictionary<string, string> translations = AiTranslator.TranslateEntries(
				_aiProvider, _aiApiKey.Trim(), model, sourceLanguage, targetLanguage, entries);

			if (translations == null)
				return;

			int applied = 0;
			foreach (KeyValuePair<string, string> translation in translations)
			{
				if (!sourceTable.ContainsKey(translation.Key))
					continue;

				targetTable[translation.Key] = translation.Value;
				applied++;
			}

			Repaint();
			Debug.Log($"[LocalizationToolkit] Applied {applied} AI translations to '{targetLanguage}'. Remember to save the data.");
		}

		private void LoadAiPreferences()
		{
			_aiApiKey = EditorPrefs.GetString(ApiKeyPrefsPrefix + _aiProvider, string.Empty);
			_aiModel = EditorPrefs.GetString(ModelPrefsPrefix + _aiProvider, _aiProvider.GetDefaultModel());
		}

		private void SaveAiPreferences()
		{
			EditorPrefs.SetString(ApiKeyPrefsPrefix + _aiProvider, _aiApiKey.Trim());
			EditorPrefs.SetString(ModelPrefsPrefix + _aiProvider, string.IsNullOrWhiteSpace(_aiModel) ? _aiProvider.GetDefaultModel() : _aiModel.Trim());
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
					_searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(200f));
					if (GUILayout.Button(new GUIContent("Collect Scene Keys",
						"Scan every LocalizedText in the loaded scenes and add its key to the table."), GUILayout.Width(130f)))
						CollectKeysFromLoadedScenes();
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

			var localizationKeys = _data.Languages[keySourceLanguage].Keys.ToList();
			localizationKeys.Sort();
			if (!string.IsNullOrEmpty(_searchFilter))
			{
				localizationKeys = localizationKeys
					.Where(key => key.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
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

		private void CollectKeysFromLoadedScenes()
		{
			LocalizedText[] texts = UnityEngine.Object.FindObjectsByType<LocalizedText>(FindObjectsInactive.Include);

			Dictionary<string, string> keySource = _data.Languages[KeySourceLanguage];
			var newKeys = new HashSet<string>();
			foreach (LocalizedText text in texts)
			{
				string key = text.Key?.Trim();
				if (!string.IsNullOrEmpty(key) && !keySource.ContainsKey(key))
					newKeys.Add(key);
			}

			foreach (string key in newKeys)
			{
				foreach (Dictionary<string, string> table in _data.Languages.Values)
				{
					if (!table.ContainsKey(key))
						table[key] = string.Empty;
				}
			}

			EditorUtility.DisplayDialog("Collect Scene Keys",
				newKeys.Count > 0
					? $"Added {newKeys.Count} new key(s) collected from {texts.Length} LocalizedText component(s) in the loaded scenes."
					: $"No new keys found. Scanned {texts.Length} LocalizedText component(s) in the loaded scenes.",
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

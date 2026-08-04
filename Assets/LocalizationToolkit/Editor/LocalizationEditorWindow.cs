using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>
	/// Editor window for creating, editing and saving localization data
	/// in any of the supported file formats (JSON, XML, CSV).
	/// </summary>
	public class LocalizationEditorWindow : EditorWindow
	{
		private const string DefaultKeyName = "NEW_KEY";
		private const float RemoveButtonWidth = 50f;
		private const float MinTextFieldWidth = 300f;
		private const float FromRightOffset = 50f;
		private const float EnumWidth = 350f;
		private const float ButtonWidth = 200f;
		private const int ColumnCount = 2;

		private LocalizationData _data = new LocalizationData();
		private LocalizationFileFormat _fileFormat;
		private string _fileUrl = string.Empty;

		private const string ApiKeyPrefsPrefix = "UniversityOfGames.LocalizationToolkit.ApiKey.";
		private const string ModelPrefsPrefix = "UniversityOfGames.LocalizationToolkit.Model.";

		private string[] _languageNames = Array.Empty<string>();
		private string[] _languagesAvailableToAdd = Array.Empty<string>();
		private int _selectedLanguageIndex;
		private int _languageToAddIndex;

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

		private string KeySourceLanguage =>
			_data.Languages.ContainsKey(LocalizationManager.DefaultLanguageKey)
				? LocalizationManager.DefaultLanguageKey
				: _data.Languages.Keys.First();

		[MenuItem("Tools/Localization Toolkit/Localization Editor")]
		private static void Open()
		{
			GetWindow<LocalizationEditorWindow>("Localization Editor").Show();
		}

		private void OnEnable()
		{
			LoadAiPreferences();
		}

		private void OnGUI()
		{
			GUILayout.Space(10);
			EditorGUILayout.LabelField("Load from web:", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			{
				_fileUrl = EditorGUILayout.TextField("File URL", _fileUrl);
				if (GUILayout.Button("Load From Web", GUILayout.Width(ButtonWidth)))
					LoadFromWeb();
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10);
			EditorGUILayout.LabelField("Local file:", EditorStyles.boldLabel);
			_fileFormat = (LocalizationFileFormat)EditorGUILayout.EnumPopup("File Format", _fileFormat);
			if (GUILayout.Button("Load File"))
				LoadFromFile();

			if (GUILayout.Button("Create New Data"))
				CreateNewData();

			if (_data.Languages.Count > 0)
			{
				if (GUILayout.Button("Save Data"))
					SaveToFile();

				DrawAiTranslationSection();

				GUILayout.Space(25);

				float spacePerColumn = (position.width - RemoveButtonWidth - FromRightOffset) / ColumnCount;
				_languageNames = _data.Languages.Keys.ToArray();

				GUILayout.BeginHorizontal();
				{
					GUILayout.BeginVertical(GUILayout.Width(spacePerColumn));
					{
						DrawLanguageSelection();
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical(GUILayout.Width(spacePerColumn));
					{
						GUILayout.BeginHorizontal();
						{
							DrawAddNewLanguage();
						}
						GUILayout.EndHorizontal();

						DrawRemoveLanguage();
						DrawAddNewEntry();
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();

				GUILayout.Space(15);

				DrawColumnLabels(spacePerColumn);
				DrawLocalizationGrid(spacePerColumn);
			}

			GUILayout.Space(10);
		}

		private void LoadFromWeb()
		{
			if (!RemoteFileLoader.TryGetFileFormatFromUrl(_fileUrl, out LocalizationFileFormat format))
			{
				Debug.LogError("[LocalizationToolkit] The URL must point to a .json, .xml or .csv file.");
				return;
			}

			string rawData = RemoteFileLoader.DownloadText(_fileUrl);
			if (string.IsNullOrEmpty(rawData))
				return;

			ResetSelection();
			_data = LocalizationData.Parse(rawData, format);
		}

		private void LoadFromFile()
		{
			string filePath = EditorUtility.OpenFilePanel(
				"Select localization data file", Application.streamingAssetsPath, _fileFormat.ToExtension());
			if (string.IsNullOrEmpty(filePath))
				return;

			ResetSelection();
			_data = LocalizationData.Parse(File.ReadAllText(filePath), _fileFormat);
		}

		private void SaveToFile()
		{
			string filePath = EditorUtility.SaveFilePanel(
				"Save localization data file", Application.streamingAssetsPath, "lang", _fileFormat.ToExtension());
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
		}

		private void DrawLanguageSelection()
		{
			_selectedLanguageIndex = EditorGUILayout.Popup(
				"Select Language", _selectedLanguageIndex, _languageNames, GUILayout.MaxWidth(EnumWidth));
		}

		private void DrawAddNewLanguage()
		{
			_languagesAvailableToAdd = GetLanguagesAvailableToAdd();

			if (GUILayout.Button("Add New Language", GUILayout.Width(ButtonWidth)))
				AddNewLanguage();

			if (_languageToAddIndex >= _languagesAvailableToAdd.Length && _languagesAvailableToAdd.Length > 0)
				_languageToAddIndex = 0;

			_languageToAddIndex = EditorGUILayout.Popup(
				"Language", _languageToAddIndex, _languagesAvailableToAdd, GUILayout.MaxWidth(EnumWidth));
		}

		private string[] GetLanguagesAvailableToAdd()
		{
			return Enum.GetNames(typeof(SystemLanguage))
				.Where(language => !_data.Languages.ContainsKey(language))
				.ToArray();
		}

		private void DrawRemoveLanguage()
		{
			if (GUILayout.Button("Remove Selected Language", GUILayout.Width(ButtonWidth)))
				RemoveSelectedLanguage();
		}

		private void DrawAddNewEntry()
		{
			if (GUILayout.Button("Add New Entry", GUILayout.Width(ButtonWidth)))
				AddNewEntry();
		}

		private void DrawColumnLabels(float spacePerColumn)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Key", GUILayout.MinWidth(MinTextFieldWidth), GUILayout.MaxWidth(spacePerColumn));
			GUILayout.Label("Value", GUILayout.MinWidth(MinTextFieldWidth), GUILayout.MaxWidth(spacePerColumn));
			GUILayout.EndHorizontal();
		}

		private void DrawLocalizationGrid(float spacePerColumn)
		{
			var editedLanguages = new Dictionary<string, Dictionary<string, string>>();
			var keysToReplace = new Dictionary<string, string>();
			var keysToRemove = new List<string>();

			string keySourceLanguage = KeySourceLanguage;
			var localizationKeys = _data.Languages[keySourceLanguage].Keys.ToList();
			localizationKeys.Sort();

			editedLanguages[keySourceLanguage] = new Dictionary<string, string>();
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

				foreach (string language in _data.Languages.Keys)
				{
					if (!editedLanguages.ContainsKey(language))
						editedLanguages[language] = new Dictionary<string, string>();

					if (language == selectedLanguage)
					{
						editedLanguages[language][key] = GUILayout.TextField(
							_data.Languages[language][key],
							GUILayout.MinWidth(MinTextFieldWidth), GUILayout.MaxWidth(spacePerColumn));
					}
					else
					{
						editedLanguages[language][key] = _data.Languages[language][key];
					}
				}

				if (GUILayout.Button("-", GUILayout.Width(RemoveButtonWidth)))
				{
					if (localizationKeys.Count > 1)
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

		private void AddNewLanguage()
		{
			if (_languagesAvailableToAdd.Length == 0)
			{
				Debug.LogError("[LocalizationToolkit] There is no language left to add.");
				return;
			}

			string language = _languagesAvailableToAdd[_languageToAddIndex];
			var table = new Dictionary<string, string>();
			foreach (string key in _data.Languages[KeySourceLanguage].Keys)
				table[key] = string.Empty;

			_data.Languages[language] = table;
		}

		private void RemoveSelectedLanguage()
		{
			string language = _languageNames[Mathf.Clamp(_selectedLanguageIndex, 0, _languageNames.Length - 1)];
			if (language == LocalizationManager.DefaultLanguageKey)
			{
				Debug.LogError("[LocalizationToolkit] The default language cannot be removed.");
				return;
			}

			_data.Languages.Remove(language);
			_selectedLanguageIndex = 0;
		}

		private void DrawAiTranslationSection()
		{
			GUILayout.Space(10);
			_showAiTranslation = EditorGUILayout.Foldout(_showAiTranslation, "AI Translation", true, EditorStyles.foldoutHeader);
			if (!_showAiTranslation)
				return;

			EditorGUI.indentLevel++;

			EditorGUI.BeginChangeCheck();
			_aiProvider = (AiTranslationProvider)EditorGUILayout.Popup("Provider",
				(int)_aiProvider, new[] { AiTranslationProvider.Claude.GetDisplayName(), AiTranslationProvider.OpenAi.GetDisplayName() });
			if (EditorGUI.EndChangeCheck())
				LoadAiPreferences();

			_aiModel = EditorGUILayout.TextField("Model", _aiModel);
			_aiApiKey = EditorGUILayout.PasswordField("API Key", _aiApiKey);
			_aiOverwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", _aiOverwriteExisting);

			EditorGUILayout.HelpBox(
				"The API key is stored in EditorPrefs on this machine only. It is never written to project files or builds.",
				MessageType.Info);

			string sourceLanguage = KeySourceLanguage;
			string targetLanguage = _languageNames.Length > 0
				? _languageNames[Mathf.Clamp(_selectedLanguageIndex, 0, _languageNames.Length - 1)]
				: string.Empty;

			if (targetLanguage == sourceLanguage)
			{
				EditorGUILayout.HelpBox(
					$"Select a target language other than '{sourceLanguage}' (the key source language) to translate.",
					MessageType.Warning);
			}

			using (new EditorGUI.DisabledScope(
				string.IsNullOrWhiteSpace(_aiApiKey) || targetLanguage == sourceLanguage || string.IsNullOrEmpty(targetLanguage)))
			{
				if (GUILayout.Button($"Translate '{targetLanguage}' With AI"))
					TranslateSelectedLanguage(sourceLanguage, targetLanguage);
			}

			EditorGUI.indentLevel--;
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

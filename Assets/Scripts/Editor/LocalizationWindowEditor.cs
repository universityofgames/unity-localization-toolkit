using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

public class LocalizationWindowEditor : EditorWindow {
	public LocalizationData localizationData;
	private string defaultLangName = "default";
	private string defaultKeyName = "NEW_KEY";
	private float removeButtonWidth = 50;
	private float minTextFieldWidth = 300;
	private float fromRightOffset = 50;
	private int lastCursorPos = 0;
	private int lastSelectCursorPos = 0;
	private int labelsCount = 2;
	private int enumWidth = 350;
	private int buttonWidth = 200;

	private Vector2 scrollPos;
	private string lastEditedElement;
	private bool needsRefocus;
	private TextEditor textEditor;

	private string[] languageNamesToFilter;
	private string[] availableLanguagesToAdd;
	private int filterKeyIndex = 0;
	private int selectedLanguageIndex = 0;
	private AvailableExtensions extension;

	[MenuItem("Window/Localization Editor")]
	private static void Init() {
		GetWindow<LocalizationWindowEditor>("Localization editor").Show();
	}

	private void OnGUI() {
		GUILayout.Space(10);
		extension = (AvailableExtensions)EditorGUILayout.EnumPopup("File extension", extension);

		if (localizationData != null)
		{
			float spacePerLabel = (position.width - removeButtonWidth - fromRightOffset) / labelsCount;
			languageNamesToFilter = new List<string>(localizationData.languages.Keys).ToArray();

			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical(GUILayout.Width(spacePerLabel));
				{
					DrawLanguageSelection();
				}
				GUILayout.EndVertical();

				GUILayout.BeginVertical(GUILayout.Width(spacePerLabel));
				{
					GUILayout.BeginHorizontal();
					{
						DrawAddNewLanguage();
					}
					GUILayout.EndHorizontal();

					DrawRemoveLanguage();
					DrawAddNewValue();
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(15);

			DrawLabels(spacePerLabel);
			DrawLocalizationGrid(spacePerLabel);

			GUILayout.Space(15);

			if (GUILayout.Button("Save data"))
			{
				SaveGameData();
			}
		}
		else
		{
			GUILayout.Space(15);
		}

		if (GUILayout.Button("Load data"))
		{
			LoadGameData();
		}

		if (GUILayout.Button("Create new data"))
		{
			CreateNewData();
		}
		GUILayout.Space(10);
	}

	private void DrawLanguageSelection() {
		filterKeyIndex = EditorGUILayout.Popup("Select Language", filterKeyIndex, languageNamesToFilter, GUILayout.MaxWidth(enumWidth));
	}

	private void DrawAddNewLanguage() {
		availableLanguagesToAdd = ExcludeAlreadyImplementedLanguagesFromDefaultLanguagesList();

		if (GUILayout.Button("Add new language", GUILayout.Width(buttonWidth)))
		{
			AddNewLanguage();
		}

		if (selectedLanguageIndex >= availableLanguagesToAdd.Length && availableLanguagesToAdd.Length > 0)
		{
			selectedLanguageIndex = 0;
		}
		selectedLanguageIndex = EditorGUILayout.Popup("Language", selectedLanguageIndex, availableLanguagesToAdd, GUILayout.MaxWidth(enumWidth));
	}

	private string[] ExcludeAlreadyImplementedLanguagesFromDefaultLanguagesList() {
		List<string> languages = Enum.GetNames(typeof(SystemLanguage)).ToList();
		for (int i = 0; i < languageNamesToFilter.Length; i++)
		{
			languages.Remove(languageNamesToFilter[i]);
		}
		return languages.ToArray();
	}

	private void DrawRemoveLanguage() {
		if (GUILayout.Button("Remove selected language", GUILayout.Width(buttonWidth)))
		{
			RemoveSelectedLanguage();
		}
	}

	private void DrawAddNewValue() {
		if (GUILayout.Button("Add new entry", GUILayout.Width(buttonWidth)))
		{
			AddNewEntry();
		}
	}

	private void DrawLabels(float spacePerLabel) {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Key", GUILayout.MinWidth(minTextFieldWidth), GUILayout.MaxWidth(spacePerLabel));
		GUILayout.Label("Value", GUILayout.MinWidth(minTextFieldWidth), GUILayout.MaxWidth(spacePerLabel));
		GUILayout.EndHorizontal();
	}

	private void DrawLocalizationGrid(float spacePerLabel) {
		Dictionary<string, Dictionary<string, string>> tempSyncDict = new Dictionary<string, Dictionary<string, string>>();
		Dictionary<string, string> keysToReplace = new Dictionary<string, string>();
		List<string> keysToRemove = new List<string>();

		tempSyncDict.Add(defaultLangName, new Dictionary<string, string>());
		List<string> localizationKeys = new List<string>(localizationData.languages[defaultLangName].Keys);
		localizationKeys.Sort();
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		int elementID = 0;
		foreach (string key in localizationKeys)
		{
			GUILayout.BeginHorizontal();
			GUI.SetNextControlName(elementID.ToString());
			string newKey = GUILayout.TextField(key, new GUILayoutOption[] { GUILayout.MinWidth(minTextFieldWidth), GUILayout.MaxWidth(spacePerLabel) });

			if (key != newKey)
			{
				lastEditedElement = newKey;
				needsRefocus = true;
				keysToReplace.Add(key, newKey);
				textEditor = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
			}

			List<string> keys = new List<string>(localizationData.languages.Keys);
			string lang = languageNamesToFilter[filterKeyIndex];
			for (int i = 0; i < keys.Count; i++)
			{
				if (tempSyncDict.ContainsKey(keys[i]) == false)
					tempSyncDict.Add(keys[i], new Dictionary<string, string>());
				if (keys[i] == lang)
				{
					tempSyncDict[keys[i]][key] = GUILayout.TextField(localizationData.languages[keys[i]][key], GUILayout.MinWidth(minTextFieldWidth), GUILayout.MaxWidth(spacePerLabel));
				}
				else
				{
					tempSyncDict[keys[i]][key] = localizationData.languages[keys[i]][key];
				}
			}

			if (GUILayout.Button("-", GUILayout.Width(removeButtonWidth)))
			{
				keysToRemove.Add(key);
			}
			GUILayout.EndHorizontal();
			elementID++;
		}
		EditorGUILayout.EndScrollView();

		if (GUI.changed)
		{
			localizationData.languages = tempSyncDict;
			UpdateKeys(keysToReplace);
			RemoveKeys(keysToRemove);
		}
		else
		{
			CheckIfNeedRefocus(localizationKeys);
		}

		if (textEditor != null)
		{
			lastCursorPos = textEditor.cursorIndex;
			lastSelectCursorPos = textEditor.selectIndex;
		}
	}

	private void UpdateKeys(Dictionary<string, string> keysToReplace) {
		List<string> keys = new List<string>(localizationData.languages.Keys);
		foreach (var pair in keysToReplace)
		{
			for (int i = 0; i < keys.Count; i++)
			{
				if (localizationData.languages[keys[i]].ContainsKey(pair.Value))
				{
					continue;
				}
				localizationData.languages[keys[i]].Add(pair.Value, localizationData.languages[keys[i]][pair.Key]);
				localizationData.languages[keys[i]].Remove(pair.Key);
			}
		}
	}

	private void RemoveKeys(List<string> keysToRemove) {
		List<string> keys = new List<string>(localizationData.languages.Keys);

		for (int i = 0; i < keysToRemove.Count; i++)
		{
			for (int j = 0; j < keys.Count; j++)
			{
				localizationData.languages[keys[j]].Remove(keysToRemove[i]);
			}
		}
	}

	private void AddNewEntry() {
		string key = GetNewKeyName();
		List<string> keys = new List<string>(localizationData.languages.Keys);
		for (int i = 0; i < keys.Count; i++)
		{
			localizationData.languages[keys[i]].Add(key, "");
		}
	}

	private string GetNewKeyName() {
		string key = defaultKeyName;
		int iterations = 0;
		while (localizationData.languages[defaultLangName].ContainsKey(key))
		{
			key = defaultKeyName + "_" + iterations;
			iterations++;
		}

		return key;
	}

	private void AddNewLanguage() {
		if (availableLanguagesToAdd.Length <= 0)
		{
			Debug.LogError("There is no any language to add.");
			return;
		}

		string languageCode = availableLanguagesToAdd[selectedLanguageIndex];
		if (localizationData.languages.ContainsKey(languageCode))
		{
			Debug.LogError("Translation already has this language");
		}
		else
		{
			localizationData.languages.Add(languageCode, new Dictionary<string, string>());
			List<string> localizationKeys = new List<string>(localizationData.languages[defaultLangName].Keys);
			for (int i = 0; i < localizationKeys.Count; i++)
			{
				localizationData.languages[languageCode].Add(localizationKeys[i], "");
			}
		}
	}

	private void RemoveSelectedLanguage() {
		string lang = languageNamesToFilter[filterKeyIndex];
		if (lang == defaultLangName)
		{
			Debug.LogError("Can't remove default language");
		}
		else
		{
			localizationData.languages.Remove(lang);
			filterKeyIndex = 0;
		}
	}

	private void CheckIfNeedRefocus(List<string> localizationKeys) {
		if (needsRefocus)
		{
			needsRefocus = false;
			int id = localizationKeys.IndexOf(lastEditedElement);
			GUI.FocusControl(id.ToString());
			textEditor = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;

			if (textEditor != null)
			{
				textEditor.OnFocus();
				textEditor.cursorIndex = lastCursorPos;
				textEditor.selectIndex = lastSelectCursorPos;
			}
		}
	}

	private void LoadGameData() {
		string filePath = EditorUtility.OpenFilePanel("Select localization data file", Application.streamingAssetsPath, extension.ToString().ToLower());

		if (!string.IsNullOrEmpty(filePath))
		{
			ResetIndexes();
			string data = File.ReadAllText(filePath);
			if (extension == AvailableExtensions.json)
			{
				LoadJSONFile(data);
			}
			else if (extension == AvailableExtensions.xml)
			{
				LoadXMLFile(data);
			}
		}
	}

	private void LoadJSONFile(string data) {
		JSONObject jsonData = new JSONObject(data);
		localizationData = new LocalizationData(jsonData);
	}

	private void LoadXMLFile(string data) {
		XDocument xmlDocument = XDocument.Parse(data);
		localizationData = new LocalizationData(xmlDocument);
	}

	private void SaveGameData() {
		string filePath = EditorUtility.SaveFilePanel("Save localization data file", Application.streamingAssetsPath, "", extension.ToString().ToLower());
		if (!string.IsNullOrEmpty(filePath))
		{
			if (extension == AvailableExtensions.json)
			{
				SaveJSONFile(filePath);
			}
			else if (extension == AvailableExtensions.xml)
			{
				SaveXMLFile(filePath);
			}
		}
	}

	private void SaveJSONFile(string filePath) {
		string data = localizationData.SaveLocalizationDataToJSON().ToString();
		File.WriteAllText(filePath, data);
	}

	private void SaveXMLFile(string filePath) {
		XDocument xml = localizationData.SaveLocalizationDataToXML();
		XmlWriterSettings settings = new XmlWriterSettings
		{
			Encoding = Encoding.UTF8,
			ConformanceLevel = ConformanceLevel.Document,
			OmitXmlDeclaration = false,
			CloseOutput = true,
			Indent = true,
			IndentChars = "  ",
			NewLineHandling = NewLineHandling.Replace
		};

		using (StreamWriter sw = File.CreateText(filePath))
		using (XmlWriter writer = XmlWriter.Create(sw, settings))
		{
			xml.WriteTo(writer);
			writer.Close();
		}
	}

	private void CreateNewData() {
		ResetIndexes();
		localizationData = new LocalizationData(defaultLangName, defaultKeyName);
	}

	private void ResetIndexes() {
		filterKeyIndex = 0;
		selectedLanguageIndex = 0;
	}
}
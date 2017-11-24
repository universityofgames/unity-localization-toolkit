using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LocalizedTextEditor : EditorWindow {
	public LocalizationData localizationData;
	private string defaultKeyName = "NEW_KEY";
	private float removeButtonWidth = 50;
	private float minTextFieldWidth = 300;
	private float fromRightOffset = 50;
	private Vector2 maxWindowSize = new Vector2(1024, 600);
	private Vector2 scrollPos;

	[MenuItem("Window/Localized Text Editor")]
	private static void Init() {
		GetWindow<LocalizedTextEditor>("Localization editor").Show();
	}

	private void OnGUI() {
		if (localizationData != null)
		{
			SerializedObject serializedObject = new SerializedObject(this);
			float spacePerLabel = (position.width - removeButtonWidth - fromRightOffset) / (localizationData.languages.Count + 1);
			//scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(maxWindowSize.x), GUILayout.Height(maxWindowSize.y));

			DrawLabels(spacePerLabel);
			DrawLocalizationGrid(spacePerLabel);

			if (GUILayout.Button("Add new entry", GUILayout.Width(spacePerLabel)))
			{
				AddNewEntry();
			}
			//	EditorGUILayout.EndScrollView();

			GUILayout.Space(50);

			if (GUILayout.Button("Save data"))
			{
				SaveGameData();
			}
		}

		if (GUILayout.Button("Load data"))
		{
			LoadGameData();
		}

		if (GUILayout.Button("Create new data"))
		{
			CreateNewData();
		}
	}

	private void DrawLabels(float spacePerLabel) {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Keys", GUILayout.MinWidth(minTextFieldWidth), GUILayout.MaxWidth(spacePerLabel));
		foreach (string key in localizationData.languages.Keys)
		{
			GUILayout.Label(key, GUILayout.MinWidth(minTextFieldWidth), GUILayout.MaxWidth(spacePerLabel));
		}
		GUILayout.EndHorizontal();
	}

	private string lastEditedElement;
	private bool needsRefocus;
	private TextEditor te;
	private int lastCursorPos = 0;
	private int lastSelectCursorPos = 0;

	private void DrawLocalizationGrid(float spacePerLabel) {
		Dictionary<string, Dictionary<string, string>> tempSyncDict = new Dictionary<string, Dictionary<string, string>>();
		Dictionary<string, string> keysToReplace = new Dictionary<string, string>();
		List<string> keysToRemove = new List<string>();

		tempSyncDict.Add("default", new Dictionary<string, string>());
		List<string> localizationKeys = new List<string>(localizationData.languages["default"].Keys);
		localizationKeys.Sort();

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
				te = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
			}

			List<string> keys = new List<string>(localizationData.languages.Keys);
			for (int i = 0; i < keys.Count; i++)
			{
				if (tempSyncDict.ContainsKey(keys[i]) == false)
					tempSyncDict.Add(keys[i], new Dictionary<string, string>());
				tempSyncDict[keys[i]][key] = GUILayout.TextField(localizationData.languages[keys[i]][key], GUILayout.MinWidth(minTextFieldWidth), GUILayout.MaxWidth(spacePerLabel));
			}

			if (GUILayout.Button("-", GUILayout.Width(removeButtonWidth)))
			{
				keysToRemove.Add(key);
			}
			GUILayout.EndHorizontal();
			elementID++;
		}

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

		if (te != null)
		{
			lastCursorPos = te.cursorIndex;
			lastSelectCursorPos = te.selectIndex;
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
		while (localizationData.languages["default"].ContainsKey(key))
		{
			key = defaultKeyName + "_" + iterations;
			iterations++;
		}

		return key;
	}

	private void CheckIfNeedRefocus(List<string> localizationKeys) {
		if (needsRefocus)
		{
			needsRefocus = false;
			int id = localizationKeys.IndexOf(lastEditedElement);
			GUI.FocusControl(id.ToString());
			te = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;

			if (te != null)
			{
				te.OnFocus();
				te.cursorIndex = lastCursorPos;
				te.selectIndex = lastSelectCursorPos;
			}
		}
	}

	private void LoadGameData() {
		string filePath = EditorUtility.OpenFilePanel("Select localization data file", Application.streamingAssetsPath, "json");

		if (!string.IsNullOrEmpty(filePath))
		{
			string data = File.ReadAllText(filePath);
			JSONObject jsonData = new JSONObject(data);

			localizationData = new LocalizationData(jsonData);
		}
	}

	private void SaveGameData() {
		string filePath = EditorUtility.SaveFilePanel("Save localization data file", Application.streamingAssetsPath, "", "json");
		if (!string.IsNullOrEmpty(filePath))
		{
			string dataAsJson = localizationData.SaveLocalizationDataToJSON().ToString();
			File.WriteAllText(filePath, dataAsJson);
		}
	}

	private void CreateNewData() {
		localizationData = new LocalizationData();
	}
}
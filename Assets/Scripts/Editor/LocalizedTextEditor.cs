using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LocalizedTextEditor : EditorWindow {
	public LocalizationData localizationData;

	[MenuItem("Window/Localized Text Editor")]
	private static void Init() {
		GetWindow<LocalizedTextEditor>("Localization editor");
	}

	private void OnGUI() {
		if (localizationData != null)
		{
			SerializedObject serializedObject = new SerializedObject(this);
			//SerializedProperty serializedProperty = serializedObject.FindProperty("localizationData");
			//EditorGUILayout.PropertyField(serializedProperty, true);
			float spacePerLabel = position.width / (localizationData.languages.Count + 1);

			DrawLabels(spacePerLabel);
			DrawLocalizationGrid(spacePerLabel);

			serializedObject.ApplyModifiedProperties();

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
		GUILayout.Label("Keys", GUILayout.Width(spacePerLabel));
		foreach (string key in localizationData.languages.Keys)
		{
			GUILayout.Label(key, GUILayout.Width(spacePerLabel));
		}
		GUILayout.EndHorizontal();
	}

	private void DrawLocalizationGrid(float spacePerLabel) {
		Dictionary<string, Dictionary<string, string>> tempSyncDict = new Dictionary<string, Dictionary<string, string>>();
		Dictionary<string, string> keysToReplace = new Dictionary<string, string>();

		tempSyncDict.Add("default", new Dictionary<string, string>());
		List<string> localizationKeys = new List<string>(localizationData.languages["default"].Keys);
		localizationKeys.Sort();
		foreach (string key in localizationKeys)
		{
			GUILayout.BeginHorizontal();
			string newKey = GUILayout.TextField(key, GUILayout.Width(spacePerLabel));

			if (key != newKey)
				keysToReplace.Add(key, newKey);

			List<string> keys = new List<string>(localizationData.languages.Keys);
			for (int i = 0; i < keys.Count; i++)
			{
				if (tempSyncDict.ContainsKey(keys[i]) == false)
					tempSyncDict.Add(keys[i], new Dictionary<string, string>());
				tempSyncDict[keys[i]][key] = GUILayout.TextField(localizationData.languages[keys[i]][key], GUILayout.Width(spacePerLabel));
			}

			GUILayout.EndHorizontal();
		}

		if (GUI.changed)
		{
			localizationData.languages = tempSyncDict;
			UpdateKeys(keysToReplace);
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LocalizedTextEditor : EditorWindow {
	public LocalizationData localizationData;

	[MenuItem("Window/Localized Text Editor")]
	private static void Init() {
		EditorWindow.GetWindow(typeof(LocalizedTextEditor)).Show();
	}

	private void OnGUI() {
		if (localizationData != null)
		{
			SerializedObject serializedObject = new SerializedObject(this);
			SerializedProperty serializedProperty = serializedObject.FindProperty("localizationData");
			EditorGUILayout.PropertyField(serializedProperty, true);
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

		/*if (GUILayout.Button("Create new data"))
		{
			CreateNewData();
		}*/
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
		//localizationData = new LocalizationData();
	}
}
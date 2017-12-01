using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizationManager))]
public class LocalizationManagerEditor : Editor {
	private int selectedIndex = 0;

	public override void OnInspectorGUI() {
		LocalizationManager localizationManager = (LocalizationManager)target;
		localizationManager.fileURL = EditorGUILayout.TextField("File URL: ", localizationManager.fileURL);
		if (GUILayout.Button("Load from web"))
		{
			localizationManager.LoadFromWeb();
		}

		GUILayout.BeginHorizontal();
		localizationManager.fileName = EditorGUILayout.TextField("File name", localizationManager.fileName);
		localizationManager.extension = (AvailableExtensions)EditorGUILayout.EnumPopup(localizationManager.extension);
		GUILayout.EndHorizontal();

		if (GUILayout.Button("Load local file"))
			localizationManager.InitLocalizationData();

		string[] languagesToShow = localizationManager.GetAvailableLanguages();
		if (languagesToShow != null)
		{
			GUILayout.Label("Select language");
			selectedIndex = EditorGUILayout.Popup(selectedIndex, languagesToShow);
			if (GUILayout.Button("Load language"))
				localizationManager.LoadLanguage(languagesToShow[selectedIndex]);
		}
	}
}
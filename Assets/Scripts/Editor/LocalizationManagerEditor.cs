using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizationManager))]
public class LocalizationManagerEditor : Editor {
	private int selectedIndex = 0;

	public override void OnInspectorGUI() {
		LocalizationManager localizationManager = (LocalizationManager)target;

		localizationManager.fileName = EditorGUILayout.TextField("File name", localizationManager.fileName);
		if (GUILayout.Button("Load file"))
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
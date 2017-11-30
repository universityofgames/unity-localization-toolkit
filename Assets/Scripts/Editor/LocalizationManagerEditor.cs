using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizationManager))]
public class LocalizationManagerEditor : Editor {
	private int selectedIndex = 0;
	private AvailableExtensions extension;

	public override void OnInspectorGUI() {
		LocalizationManager localizationManager = (LocalizationManager)target;

		GUILayout.BeginHorizontal();
		localizationManager.fileName = EditorGUILayout.TextField("File name", localizationManager.fileName);
		extension = (AvailableExtensions)EditorGUILayout.EnumPopup(extension);
		localizationManager.extension = extension.ToString();
		GUILayout.EndHorizontal();

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
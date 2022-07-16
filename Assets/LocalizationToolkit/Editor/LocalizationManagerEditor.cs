using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizationManager))]
public class LocalizationManagerEditor : Editor {
	private int _selectedIndex = 0;

	public override void OnInspectorGUI() {
		LocalizationManager localizationManager = (LocalizationManager)target;
		localizationManager.fileURL = EditorGUILayout.TextField("File URL: ", localizationManager.fileURL);
		if (GUILayout.Button("Load from web"))
		{
			localizationManager.LoadFromWeb(localizationManager.fileURL);
		}

		GUILayout.Space(20);	
		
		GUILayout.BeginHorizontal();
		localizationManager.fileName = EditorGUILayout.TextField("File name", localizationManager.fileName);
		localizationManager.extension = (AvailableExtensions)EditorGUILayout.EnumPopup(localizationManager.extension);
		GUILayout.EndHorizontal();

		if (GUILayout.Button("Load local file"))
		{
			localizationManager.LoadFromFile(localizationManager.fileName, localizationManager.extension);
		}

		GUILayout.Space(20);	
		
		string[] languagesToShow = localizationManager.GetAvailableLanguages();
		if (languagesToShow != null)
		{
			GUILayout.Label("Select language");
			_selectedIndex = EditorGUILayout.Popup(_selectedIndex, languagesToShow);
			if (GUILayout.Button("Load language"))
				localizationManager.LoadLanguage(languagesToShow[_selectedIndex]);
		}
	}
}
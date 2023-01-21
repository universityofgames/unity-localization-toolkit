/*
 * This script is a custom editor for the LocalizationManager class in Unity. 
 * The script is used to create a custom inspector in the Unity editor that allows 
 * the user to load localization data from either a file on the web or a local file, 
 * as well as select and load a specific language.
 */

using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizationManager))]
public class LocalizationManagerEditor : Editor {
	private int _selectedIndex = 0;

	public override void OnInspectorGUI() {
		GUILayout.BeginVertical();
		WebSection();
		GUILayout.EndVertical();

		GUILayout.Space(20);	
		
		GUILayout.BeginVertical();
		FileSection();
		GUILayout.EndVertical();

		GUILayout.Space(20);	
		
		GUILayout.BeginVertical();
		LanguageSection();
		GUILayout.EndVertical();
	}

	// Draw GUI for fetch file from WWW
	private void WebSection()
	{
		LocalizationManager localizationManager = (LocalizationManager)target;
		localizationManager.fileURL = EditorGUILayout.TextField("File URL: ", localizationManager.fileURL);
		if (GUILayout.Button("Load from web"))
		{
			localizationManager.LoadFromWeb(localizationManager.fileURL);
		}
	}

	// Draw GUI for fetch file from local storage
	private void FileSection()
	{
		LocalizationManager localizationManager = (LocalizationManager)target;
		localizationManager.fileName = EditorGUILayout.TextField("File name", localizationManager.fileName);
		localizationManager.extension = (AvailableExtensions)EditorGUILayout.EnumPopup(localizationManager.extension);
		if (GUILayout.Button("Load local file"))
		{
			string filepath = Path.Combine(Application.streamingAssetsPath, localizationManager.fileName + "." + localizationManager.extension.ToString().ToLower());
			localizationManager.LoadFromFile(filepath, localizationManager.extension);
		}
	}

	// Draw GUI for load language
	private void LanguageSection()
	{
		LocalizationManager localizationManager = (LocalizationManager)target;
		string[] languagesToShow = localizationManager.GetAvailableLanguages();
		if (languagesToShow != null && languagesToShow.Length > 0)
		{
			GUILayout.Label("Select language");
			_selectedIndex = EditorGUILayout.Popup(_selectedIndex, languagesToShow);
			if (GUILayout.Button("Load language"))
				localizationManager.LoadLanguage(languagesToShow[_selectedIndex]);
		}
	}
}
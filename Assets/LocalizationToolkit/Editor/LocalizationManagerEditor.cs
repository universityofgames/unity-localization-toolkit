using UnityEditor;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>
	/// Custom inspector for <see cref="LocalizationManager"/> that allows loading
	/// localization data from a remote URL or a local file and switching languages.
	/// </summary>
	[CustomEditor(typeof(LocalizationManager))]
	public class LocalizationManagerEditor : UnityEditor.Editor
	{
		private SerializedProperty _remoteUrl;
		private SerializedProperty _localFileName;
		private SerializedProperty _fileFormat;
		private SerializedProperty _detectSystemLanguage;
		private SerializedProperty _missingTranslationText;
		private int _selectedLanguageIndex;

		private void OnEnable()
		{
			_remoteUrl = serializedObject.FindProperty("_remoteUrl");
			_localFileName = serializedObject.FindProperty("_localFileName");
			_fileFormat = serializedObject.FindProperty("_fileFormat");
			_detectSystemLanguage = serializedObject.FindProperty("_detectSystemLanguage");
			_missingTranslationText = serializedObject.FindProperty("_missingTranslationText");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			var manager = (LocalizationManager)target;

			EditorGUILayout.LabelField("Remote Source", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_remoteUrl, new GUIContent("File URL"));
			bool loadFromWeb;
			using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_remoteUrl.stringValue)))
				loadFromWeb = GUILayout.Button("Load From Web");

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Local Source (StreamingAssets)", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_localFileName, new GUIContent("File Name"));
			EditorGUILayout.PropertyField(_fileFormat, new GUIContent("File Format"));
			bool loadFromFile;
			using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_localFileName.stringValue)))
				loadFromFile = GUILayout.Button("Load Local File");

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Behaviour", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_detectSystemLanguage, new GUIContent("Detect System Language"));
			EditorGUILayout.PropertyField(_missingTranslationText, new GUIContent("Missing Translation Text"));

			bool loadLanguage = false;
			string[] languages = manager.GetAvailableLanguages();
			if (languages.Length > 0)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Languages", EditorStyles.boldLabel);
				_selectedLanguageIndex = Mathf.Clamp(_selectedLanguageIndex, 0, languages.Length - 1);
				_selectedLanguageIndex = EditorGUILayout.Popup("Language", _selectedLanguageIndex, languages);
				loadLanguage = GUILayout.Button("Load Language");
				EditorGUILayout.LabelField("Active Language",
					string.IsNullOrEmpty(manager.ActiveLanguage) ? "-" : manager.ActiveLanguage);
			}

			serializedObject.ApplyModifiedProperties();

			if (loadFromWeb)
				manager.LoadFromWeb(manager.RemoteUrl);
			if (loadFromFile)
				manager.LoadFromFile(manager.GetLocalFilePath(), manager.FileFormat);
			if (loadLanguage)
				manager.LoadLanguage(languages[_selectedLanguageIndex]);
		}
	}
}

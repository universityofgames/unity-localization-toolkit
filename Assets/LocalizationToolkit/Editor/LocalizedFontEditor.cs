using System;
using UnityEditor;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>
	/// Custom inspector for <see cref="LocalizedFont"/> that offers a language popup
	/// per font override, fed by the data loaded in the <see cref="LocalizationManager"/>.
	/// </summary>
	[CustomEditor(typeof(LocalizedFont))]
	public class LocalizedFontEditor : UnityEditor.Editor
	{
		private SerializedProperty _defaultTmpFont;
		private SerializedProperty _defaultLegacyFont;
		private SerializedProperty _overrides;

		private void OnEnable()
		{
			_defaultTmpFont = serializedObject.FindProperty("_defaultTmpFont");
			_defaultLegacyFont = serializedObject.FindProperty("_defaultLegacyFont");
			_overrides = serializedObject.FindProperty("_overrides");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.LabelField("Default Fonts", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_defaultTmpFont, new GUIContent("TMP Font"));
			EditorGUILayout.PropertyField(_defaultLegacyFont, new GUIContent("Legacy Font"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Language Overrides", EditorStyles.boldLabel);

			LocalizationManager manager = LocalizationManager.Instance;
			string[] languages = manager != null ? manager.GetAvailableLanguages() : Array.Empty<string>();

			for (int i = 0; i < _overrides.arraySize; i++)
			{
				SerializedProperty element = _overrides.GetArrayElementAtIndex(i);
				SerializedProperty language = element.FindPropertyRelative("Language");

				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						if (languages.Length > 0)
						{
							int index = Mathf.Max(0, Array.IndexOf(languages, language.stringValue));
							index = EditorGUILayout.Popup("Language", index, languages);
							language.stringValue = languages[index];
						}
						else
						{
							language.stringValue = EditorGUILayout.TextField("Language", language.stringValue);
						}

						if (GUILayout.Button("-", GUILayout.Width(24f)))
						{
							_overrides.DeleteArrayElementAtIndex(i);
							break;
						}
					}

					EditorGUILayout.PropertyField(element.FindPropertyRelative("TmpFont"), new GUIContent("TMP Font"));
					EditorGUILayout.PropertyField(element.FindPropertyRelative("LegacyFont"), new GUIContent("Legacy Font"));
					EditorGUILayout.PropertyField(element.FindPropertyRelative("SizeMultiplier"), new GUIContent("Size Multiplier"));
				}
			}

			if (GUILayout.Button("Add Override"))
				_overrides.arraySize++;

			if (languages.Length == 0)
			{
				EditorGUILayout.HelpBox(
					"Load localization data in a LocalizationManager to pick languages from a list.",
					MessageType.Info);
			}

			if (serializedObject.ApplyModifiedProperties())
				((LocalizedFont)target).Refresh();
		}
	}
}

using System;
using UnityEditor;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>
	/// Custom inspector for <see cref="LocalizedImage"/> that offers a language popup
	/// per sprite override, fed by the data loaded in the <see cref="LocalizationManager"/>.
	/// </summary>
	[CustomEditor(typeof(LocalizedImage))]
	public class LocalizedImageEditor : UnityEditor.Editor
	{
		private SerializedProperty _defaultSprite;
		private SerializedProperty _overrides;

		private void OnEnable()
		{
			_defaultSprite = serializedObject.FindProperty("_defaultSprite");
			_overrides = serializedObject.FindProperty("_overrides");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_defaultSprite, new GUIContent("Default Sprite"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Language Overrides", EditorStyles.boldLabel);

			LocalizationManager manager = LocalizationManager.Instance;
			string[] languages = manager != null ? manager.GetAvailableLanguages() : Array.Empty<string>();

			for (int i = 0; i < _overrides.arraySize; i++)
			{
				SerializedProperty element = _overrides.GetArrayElementAtIndex(i);
				SerializedProperty language = element.FindPropertyRelative("Language");
				SerializedProperty sprite = element.FindPropertyRelative("Sprite");

				using (new EditorGUILayout.HorizontalScope())
				{
					if (languages.Length > 0)
					{
						int index = Mathf.Max(0, Array.IndexOf(languages, language.stringValue));
						index = EditorGUILayout.Popup(index, languages, GUILayout.Width(160f));
						language.stringValue = languages[index];
					}
					else
					{
						language.stringValue = EditorGUILayout.TextField(language.stringValue, GUILayout.Width(160f));
					}

					EditorGUILayout.PropertyField(sprite, GUIContent.none);

					if (GUILayout.Button("-", GUILayout.Width(24f)))
					{
						_overrides.DeleteArrayElementAtIndex(i);
						break;
					}
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
				((LocalizedImage)target).Refresh();
		}
	}
}

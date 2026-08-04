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

			string[] languages = LocalizationEditorGui.GetAvailableLanguages();

			for (int i = 0; i < _overrides.arraySize; i++)
			{
				SerializedProperty element = _overrides.GetArrayElementAtIndex(i);

				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						LocalizationEditorGui.DrawLanguageField(
							"Language", element.FindPropertyRelative("Language"), languages);

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

			LocalizationEditorGui.DrawNoLanguagesHint(languages);

			if (serializedObject.ApplyModifiedProperties())
				((LocalizedFont)target).Refresh();
		}
	}
}

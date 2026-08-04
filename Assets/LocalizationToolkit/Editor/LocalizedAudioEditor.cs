using UnityEditor;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>
	/// Custom inspector for <see cref="LocalizedAudio"/> that offers a language popup
	/// per clip override, fed by the data loaded in the <see cref="LocalizationManager"/>.
	/// </summary>
	[CustomEditor(typeof(LocalizedAudio))]
	public class LocalizedAudioEditor : UnityEditor.Editor
	{
		private SerializedProperty _defaultClip;
		private SerializedProperty _overrides;

		private void OnEnable()
		{
			_defaultClip = serializedObject.FindProperty("_defaultClip");
			_overrides = serializedObject.FindProperty("_overrides");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_defaultClip, new GUIContent("Default Clip"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Language Overrides", EditorStyles.boldLabel);

			string[] languages = LocalizationEditorGui.GetAvailableLanguages();

			for (int i = 0; i < _overrides.arraySize; i++)
			{
				SerializedProperty element = _overrides.GetArrayElementAtIndex(i);

				using (new EditorGUILayout.HorizontalScope())
				{
					LocalizationEditorGui.DrawLanguageField(
						element.FindPropertyRelative("Language"), languages, GUILayout.Width(160f));
					EditorGUILayout.PropertyField(element.FindPropertyRelative("Clip"), GUIContent.none);

					if (GUILayout.Button("-", GUILayout.Width(24f)))
					{
						_overrides.DeleteArrayElementAtIndex(i);
						break;
					}
				}
			}

			if (GUILayout.Button("Add Override"))
				_overrides.arraySize++;

			LocalizationEditorGui.DrawNoLanguagesHint(languages);

			if (serializedObject.ApplyModifiedProperties())
				((LocalizedAudio)target).Refresh();
		}
	}
}

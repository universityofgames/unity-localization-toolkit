using UnityEditor;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>
	/// Custom inspector for <see cref="LocalizedObject"/> that offers a language popup
	/// per entry, fed by the data loaded in the <see cref="LocalizationManager"/>.
	/// </summary>
	[CustomEditor(typeof(LocalizedObject))]
	public class LocalizedObjectEditor : UnityEditor.Editor
	{
		private SerializedProperty _defaultTarget;
		private SerializedProperty _entries;

		private void OnEnable()
		{
			_defaultTarget = serializedObject.FindProperty("_defaultTarget");
			_entries = serializedObject.FindProperty("_entries");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_defaultTarget, new GUIContent("Default Object"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Language Objects", EditorStyles.boldLabel);

			string[] languages = LocalizationEditorGui.GetAvailableLanguages();

			for (int i = 0; i < _entries.arraySize; i++)
			{
				SerializedProperty element = _entries.GetArrayElementAtIndex(i);

				using (new EditorGUILayout.HorizontalScope())
				{
					LocalizationEditorGui.DrawLanguageField(
						element.FindPropertyRelative("Language"), languages, GUILayout.Width(160f));
					EditorGUILayout.PropertyField(element.FindPropertyRelative("Target"), GUIContent.none);

					if (GUILayout.Button("-", GUILayout.Width(24f)))
					{
						_entries.DeleteArrayElementAtIndex(i);
						break;
					}
				}
			}

			if (GUILayout.Button("Add Entry"))
				_entries.arraySize++;

			LocalizationEditorGui.DrawNoLanguagesHint(languages);

			if (serializedObject.ApplyModifiedProperties())
				((LocalizedObject)target).Refresh();
		}
	}
}

using System;
using UnityEditor;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>Shared GUI building blocks for the toolkit's custom inspectors.</summary>
	public static class LocalizationEditorGui
	{
		/// <summary>Language keys of the loaded data, or an empty array when nothing is loaded.</summary>
		/// <returns>The available language keys.</returns>
		public static string[] GetAvailableLanguages()
		{
			LocalizationManager manager = LocalizationManager.Instance;
			return manager != null ? manager.GetAvailableLanguages() : Array.Empty<string>();
		}

		/// <summary>
		/// Draws a language selector: a popup fed by the loaded localization data, or a
		/// plain text field when no data is available.
		/// </summary>
		/// <param name="languageProperty">String property holding the language key.</param>
		/// <param name="languages">Available language keys, from <see cref="GetAvailableLanguages"/>.</param>
		/// <param name="options">Layout options for the control.</param>
		public static void DrawLanguageField(SerializedProperty languageProperty, string[] languages,
			params GUILayoutOption[] options)
		{
			if (languages.Length > 0)
			{
				int index = Mathf.Max(0, Array.IndexOf(languages, languageProperty.stringValue));
				index = EditorGUILayout.Popup(index, languages, options);
				languageProperty.stringValue = languages[index];
			}
			else
			{
				languageProperty.stringValue = EditorGUILayout.TextField(languageProperty.stringValue, options);
			}
		}

		/// <summary>Labeled variant of <see cref="DrawLanguageField(SerializedProperty, string[], GUILayoutOption[])"/>.</summary>
		/// <param name="label">Label shown in front of the control.</param>
		/// <param name="languageProperty">String property holding the language key.</param>
		/// <param name="languages">Available language keys.</param>
		public static void DrawLanguageField(string label, SerializedProperty languageProperty, string[] languages)
		{
			if (languages.Length > 0)
			{
				int index = Mathf.Max(0, Array.IndexOf(languages, languageProperty.stringValue));
				index = EditorGUILayout.Popup(label, index, languages);
				languageProperty.stringValue = languages[index];
			}
			else
			{
				languageProperty.stringValue = EditorGUILayout.TextField(label, languageProperty.stringValue);
			}
		}

		/// <summary>Shows a hint when no localization data is loaded to feed the language popups.</summary>
		/// <param name="languages">Available language keys.</param>
		public static void DrawNoLanguagesHint(string[] languages)
		{
			if (languages.Length == 0)
			{
				EditorGUILayout.HelpBox(
					"Load localization data in a LocalizationManager to pick languages from a list.",
					MessageType.Info);
			}
		}
	}
}

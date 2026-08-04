using System;
using UnityEditor;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>
	/// Custom inspector for <see cref="LocalizedText"/> that lets the user pick
	/// a translation key from the data loaded in the <see cref="LocalizationManager"/>.
	/// </summary>
	[CustomEditor(typeof(LocalizedText))]
	public class LocalizedTextEditor : UnityEditor.Editor
	{
		private SerializedProperty _key;
		private int _selectedKeyIndex;

		private void OnEnable()
		{
			_key = serializedObject.FindProperty("_key");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(_key, new GUIContent("Translation Key"));

			LocalizationManager manager = LocalizationManager.Instance;
			string[] keys = manager != null ? manager.GetKeys() : Array.Empty<string>();

			if (keys.Length > 0)
			{
				_selectedKeyIndex = Mathf.Clamp(_selectedKeyIndex, 0, keys.Length - 1);
				_selectedKeyIndex = EditorGUILayout.Popup("Available Keys", _selectedKeyIndex, keys);
				if (GUILayout.Button("Use Selected Key"))
					_key.stringValue = keys[_selectedKeyIndex];
			}
			else
			{
				EditorGUILayout.HelpBox(
					"Load localization data in the LocalizationManager to pick a key from a list.",
					MessageType.Info);
			}

			if (serializedObject.ApplyModifiedProperties())
				((LocalizedText)target).Refresh();
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedText))]
public class LocalizedTextEditor : Editor {
	private LocalizationManager localizationManager;
	private int selectedIndex;

	public override void OnInspectorGUI() {
		if (localizationManager == null)
			localizationManager = FindObjectOfType<LocalizationManager>();
		LocalizedText myTarget = (LocalizedText)target;
		myTarget.key = EditorGUILayout.TextField("Translation ID", myTarget.key);
		if (localizationManager != null)
		{
			string[] keysToShow = localizationManager.GetKeys();
			if (keysToShow != null)
			{
				GUILayout.Label("Select key ID");
				selectedIndex = EditorGUILayout.Popup(selectedIndex, keysToShow);
				if (GUILayout.Button("Set selected key"))
					myTarget.key = keysToShow[selectedIndex];
			}
		}
	}
}
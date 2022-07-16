using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedText))]
public class LocalizedTextEditor : Editor {
	private LocalizationManager _localizationManager;
	private int _selectedIndex;

	public override void OnInspectorGUI() {
		if (_localizationManager == null)
			_localizationManager = FindObjectOfType<LocalizationManager>();
		
		LocalizedText myTarget = (LocalizedText)target;
		myTarget.key = EditorGUILayout.TextField("Translation ID", myTarget.key);
		
		string[] keysToShow = _localizationManager.GetKeys();
		if (keysToShow != null)
		{
			GUILayout.Label("Select key ID");
			_selectedIndex = EditorGUILayout.Popup(_selectedIndex, keysToShow);
			if (GUILayout.Button("Set selected key"))
				myTarget.key = keysToShow[_selectedIndex];
		}
	}
}
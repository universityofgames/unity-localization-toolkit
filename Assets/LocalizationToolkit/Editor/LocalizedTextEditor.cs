using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedText))]
public class LocalizedTextEditor : Editor {
	private LocalizationManager _localizationManager;
	private int _selectedIndex;

	public override void OnInspectorGUI() {
		// First checks if a reference to the LocalizationManager script has been set and if not, 
		// it tries to find one in the current scene. Then, it creates a text field where the user 
		// can input the key, or id, for the text that they want to display.
		if (_localizationManager == null)
			_localizationManager = FindObjectOfType<LocalizationManager>();
		
		LocalizedText myTarget = (LocalizedText)target;
		myTarget.key = EditorGUILayout.TextField("Translation ID", myTarget.key);
		
		// The code then retrieves a list of keys from the LocalizationManager and uses them to 
		// populate a dropdown list in the inspector. The user can select a key from the list and 
		// press the 'Set selected key' button to set the key for the LocalizedText. This allows 
		// for easy selection and assignment of keys for localized text.
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
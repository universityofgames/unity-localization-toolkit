using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownChange : MonoBehaviour {
    // UI Dropdown Component
    private Dropdown _dropdown;

    private void Start() {
        // Load component
        _dropdown = GetComponent<Dropdown>();
        
        // Init language selection
        InitLanguageSelection();
    }
    
    /// <summary>Fetch available languages</summary>
    private void InitLanguageSelection() {
        // Check if the component exists 
        if (_dropdown) {
            _dropdown.options.Clear();
			
            // fetch available languages
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            string[] languages = LocalizationManager.instance.GetAvailableLanguages();
            if (languages != null)
            {
                for (int i = 0; i < languages.Length; i++)
                {
                    options.Add(new Dropdown.OptionData(languages[i]));
                }
                
                // Add available options to dropdown component
                _dropdown.AddOptions(options);
                
                // Assign listener
                _dropdown.onValueChanged.AddListener(ChangeLanguage);
            }
        }
    }
	
    /// <summary>Change language by index</summary>
    /// <param name="langIndex">Language Index</param>
    private void ChangeLanguage(int langIndex) {
        string lang = _dropdown.options[langIndex].text;
        LocalizationManager.instance.LoadLanguage(lang);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownChange : MonoBehaviour {
    private Dropdown _dropdown;

    private void Start() {
        _dropdown = GetComponent<Dropdown>();
        InitLanguageSelection();
    }
    
    /// <summary>Fetch available languages</summary>
    private void InitLanguageSelection() {
        if (_dropdown) {
            _dropdown.options.Clear();
			
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            string[] languages = LocalizationManager.instance.GetAvailableLanguages();
            if (languages != null)
            {
                for (int i = 0; i < languages.Length; i++)
                {
                    options.Add(new Dropdown.OptionData(languages[i]));
                }
                _dropdown.AddOptions(options);
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public enum AvailableExtensions { json, xml };

[ExecuteInEditMode]
public class LocalizationManager : MonoBehaviour {
	public static LocalizationManager instance;

	public static event Action OnLanguageChanged;

	public string fileName;
	public AvailableExtensions extension;

	private LocalizationData localizationData;

	private Dictionary<string, string> languageTranslations;
	private string missingTextString = "Localized text not found";

	private string defaultLanguage = "default";

	[SerializeField]
	private string selectedLanguage;

	private void Awake() {
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}

		InitLocalizationData();
	}

	public void InitLocalizationData() {
		string filePath = Path.Combine(Application.streamingAssetsPath, fileName + "." + extension.ToString().ToLower());
		if (File.Exists(filePath))
		{
			string data = File.ReadAllText(filePath);
			if (extension == AvailableExtensions.json)
			{
				JSONObject jsonData = new JSONObject(data);

				localizationData = new LocalizationData(jsonData);
				localizationData.SaveLocalizationDataToJSON();
			}
			else if (extension == AvailableExtensions.xml)
			{
				XDocument xmlDocument = XDocument.Parse(data);
				localizationData = new LocalizationData(xmlDocument);
			}
			LoadLanguage("pl");
		}
		else
		{
			Debug.LogError("Cannot find file!");
		}
	}

	public void LoadLanguage(string language) {
		if (localizationData.languages.ContainsKey(language))
		{
			selectedLanguage = language;
		}
		else
		{
			selectedLanguage = defaultLanguage;
		}
		languageTranslations = localizationData.languages[selectedLanguage];
		if (OnLanguageChanged != null)
			OnLanguageChanged();
	}

	public string GetLocalizedValue(string key) {
		string result = missingTextString;
		if (languageTranslations.ContainsKey(key))
		{
			result = languageTranslations[key];
		}
		return result;
	}

	public string[] GetAvailableLanguages() {
		if (IsDataEmpty())
			return null;

		return new List<string>(localizationData.languages.Keys).ToArray();
	}

	public string[] GetKeys() {
		if (IsDataEmpty())
			return null;
		return new List<string>(localizationData.languages[defaultLanguage].Keys).ToArray();
	}

	private bool IsDataEmpty() {
		return localizationData == null || localizationData.languages == null;
	}
}
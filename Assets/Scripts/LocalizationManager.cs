using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalizationManager : MonoBehaviour {
	public static LocalizationManager instance;
	private LocalizationData localizationData;
	private Dictionary<string, string> languageTranslations;
	private bool isReady = false;
	private string missingTextString = "Localized text not found";

	private string defaultLanguage = "default";
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

		DontDestroyOnLoad(gameObject);
		InitLocalizationData();
	}

	private void InitLocalizationData() {
		string filePath = Path.Combine(Application.streamingAssetsPath, "lang.json");
		if (File.Exists(filePath))
		{
			string data = File.ReadAllText(filePath);
			JSONObject jsonData = new JSONObject(data);

			localizationData = new LocalizationData(jsonData);
			localizationData.SaveLocalizationDataToJSON();
		}
		else
		{
			Debug.LogError("Cannot find file!");
		}
		LoadLanguage("pl");
		isReady = true;
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
	}

	public string GetLocalizedValue(string key) {
		string result = missingTextString;
		if (languageTranslations.ContainsKey(key))
		{
			result = languageTranslations[key];
		}
		return result;
	}

	public bool GetIsReady() {
		return isReady;
	}
}
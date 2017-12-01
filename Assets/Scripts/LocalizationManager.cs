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

	public string fileURL = "";
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

	public void LoadFromWeb() {
		string ext = WebLoader.GetExtensionFromUrl(fileURL);
		if (ext == "")
		{
			Debug.LogError("File needs .xml or .json extension");
		}
		else
		{
			string data = WebLoader.LoadStringFileFromWeb(fileURL);
			if (data != "")
			{
				AvailableExtensions extension = (AvailableExtensions)Enum.Parse(typeof(AvailableExtensions), ext);
				localizationData = LoadLocalizationData(data, extension);
				LoadLanguage("pl");
			}
		}
	}

	public void InitLocalizationData() {
		LoadFromWeb();
		if (localizationData == null)
		{
			string filePath = Path.Combine(Application.streamingAssetsPath, fileName + "." + extension.ToString().ToLower());
			if (File.Exists(filePath))
			{
				string data = File.ReadAllText(filePath);
				localizationData = LoadLocalizationData(data, extension);
				LoadLanguage("pl");
			}
			else
			{
				Debug.LogError("Cannot find file!");
			}
		}
	}

	private LocalizationData LoadLocalizationData(string rawData, AvailableExtensions extension) {
		LocalizationData localizationData = null;
		if (extension == AvailableExtensions.json)
		{
			JSONObject jsonData = new JSONObject(rawData);

			localizationData = new LocalizationData(jsonData);
			localizationData.SaveLocalizationDataToJSON();
		}
		else if (extension == AvailableExtensions.xml)
		{
			XDocument xmlDocument = XDocument.Parse(rawData);
			localizationData = new LocalizationData(xmlDocument);
		}
		return localizationData;
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
		if (languageTranslations != null && languageTranslations.ContainsKey(key))
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
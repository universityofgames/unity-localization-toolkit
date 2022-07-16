using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using Newtonsoft.Json;

public enum AvailableExtensions { json, xml };

[ExecuteInEditMode]
public class LocalizationManager : MonoBehaviour {
	public static LocalizationManager instance;
	public static event Action OnLanguageChanged;
	
	public string fileURL = "";
	public string fileName;
	public AvailableExtensions extension;

	private LocalizationData _currentLocalizationData;
	private Dictionary<string, string> _currentlanguageTranslations;
	private string _missingTextString = "Localized text not found";
	private string _defaultLanguage = "default";
	private string _selectedLanguage = "";

	private void Awake() {
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}

		// Auto load
		if (fileURL.Trim().Length > 0)
		{
			LoadFromWeb(fileURL);
		}
		else if (fileName.Trim().Length > 0)
		{
			LoadFromFile(fileName, extension);
		}
	}
	
	/// <summary>Load file from local</summary>
	/// <param name="filename">filename</param>
	/// /// <param name="extension">extension</param>
	public void LoadFromFile(string filename, AvailableExtensions extension) {
		string filePath = Path.Combine(Application.streamingAssetsPath, fileName + "." + extension.ToString().ToLower());
		try
		{
			string rawData = File.ReadAllText(filePath);
			_currentLocalizationData = LoadLocalizationData(rawData, extension);
			LoadLanguage(_defaultLanguage);
		}
		catch (Exception e)
		{
			Debug.LogError("Cannot find file at: " + filePath);
		}
	}
	
	/// <summary>Load file from internet</summary>
	/// <param name="url">URL to file</param>
	public void LoadFromWeb(string url) {
		string ext = WebLoader.GetExtensionFromUrl(url);
		if (ext.Length == 0) {
			Debug.LogError("File needs .xml or .json extension");
			return;
		}
		
		string data = WebLoader.LoadStringFileFromWeb(url);
		if (data.Length == 0) {
			return;
		}
		
		AvailableExtensions currentExtension = (AvailableExtensions)Enum.Parse(typeof(AvailableExtensions), ext);
		_currentLocalizationData = LoadLocalizationData(data, currentExtension);
		LoadLanguage(_defaultLanguage);
	}

	/// <summary>Load localization Data</summary>
	/// <param name="rawData">rawData</param>
	/// /// <param name="extension">extension</param>
	private LocalizationData LoadLocalizationData(string rawData, AvailableExtensions extension) {
		LocalizationData localizationData = new LocalizationData();
		switch (extension)
		{
			case AvailableExtensions.json:
				Dictionary<string, Dictionary<string, string>> jsonData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(rawData);
				localizationData = new LocalizationData(jsonData);
				localizationData.SaveLocalizationDataToJSON();
				break;
			case AvailableExtensions.xml:
				XDocument xmlDocument = XDocument.Parse(rawData);
				localizationData = new LocalizationData(xmlDocument);
				break;
		}
		return localizationData;
	}

	/// <summary>Load language Data</summary>
	/// <param name="langKey">Language Key</param>
	public void LoadLanguage(string langKey) {
		_selectedLanguage = _currentLocalizationData.languages.ContainsKey(langKey) ? langKey : _defaultLanguage;
		_currentlanguageTranslations = _currentLocalizationData.languages[_selectedLanguage];
		if (OnLanguageChanged != null)
			OnLanguageChanged();
	}

	/// <summary>Get Localized Value</summary>
	/// <param name="key">key</param>
	public string GetLocalizedValue(string key) {
		string result = _missingTextString;
		try
		{
			result = _currentlanguageTranslations[key];
		}
		catch (Exception e)
		{
			Debug.LogError("Cannot find key: " + key);
		}
		return result;
	}

	/// <summary>Get Available Languages</summary>
	public string[] GetAvailableLanguages() {
		if (IsDataEmpty())
			return null;

		return new List<string>(_currentLocalizationData.languages.Keys).ToArray();
	}

	/// <summary>Get All Keys</summary>
	public string[] GetKeys() {
		if (IsDataEmpty())
			return null;
		return new List<string>(_currentLocalizationData.languages[_defaultLanguage].Keys).ToArray();
	}

	private bool IsDataEmpty() {
		return _currentLocalizationData == null || _currentLocalizationData.languages == null;
	}
}
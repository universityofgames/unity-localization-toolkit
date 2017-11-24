using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocalizationData {
	public Dictionary<string, Dictionary<string, string>> languages;

	public LocalizationData() {
		languages = new Dictionary<string, Dictionary<string, string>>();
		languages.Add("default", new Dictionary<string, string>());
	}

	public LocalizationData(JSONObject jsonData) {
		languages = new Dictionary<string, Dictionary<string, string>>();
		LoadObjectFromJSON(jsonData);
	}

	private void LoadObjectFromJSON(JSONObject jsonData) {
		for (int i = 0; i < jsonData.Count; i++)
		{
			string language = jsonData.keys[i];
			JSONObject languageJSON = jsonData[language];

			languages.Add(language, new Dictionary<string, string>());
			for (int j = 0; j < languageJSON.Count; j++)
			{
				string key = languageJSON.keys[j];
				languages[language].Add(key, languageJSON[key].str);
			}
		}
	}

	public JSONObject SaveLocalizationDataToJSON() {
		JSONObject data = new JSONObject();
		foreach (var langs in languages)
		{
			JSONObject dict = new JSONObject(langs.Value);
			data.AddField(langs.Key, dict);
		}
		Debug.Log(data);
		return data;
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocalizationData {
	public Dictionary<string, Dictionary<string, string>> languages;

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
				languages[language].Add(languageJSON[j]["key"].str, languageJSON[j]["value"].str);
			}
		}
	}
}
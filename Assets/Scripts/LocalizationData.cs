using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

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

	public LocalizationData(XmlDocument xmlDocument) {
		languages = new Dictionary<string, Dictionary<string, string>>();
		LoadObjectFromXML(xmlDocument);
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

	private void LoadObjectFromXML(XmlDocument xmlDocument) {
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

	public XDocument SaveLocalizationDataToXML() {
		XDocument doc = new XDocument(new XElement("translations"));
		foreach (var langs in languages)
		{
			XElement el = new XElement(langs.Key,
	langs.Value.Select(kv => new XElement(kv.Key, kv.Value)));
			doc.Element("translations").Add(el);
		}

		return doc;
	}
}
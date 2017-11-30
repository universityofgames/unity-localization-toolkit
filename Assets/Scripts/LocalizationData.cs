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
	private string xmlRootNode = "translations";

	public LocalizationData(string defaultLanguage, string defaultKey) {
		languages = new Dictionary<string, Dictionary<string, string>>();
		languages.Add(defaultLanguage, new Dictionary<string, string>());
		languages[defaultLanguage].Add(defaultKey, "");
	}

	public LocalizationData(JSONObject jsonData) {
		languages = new Dictionary<string, Dictionary<string, string>>();
		LoadObjectFromJSON(jsonData);
	}

	public LocalizationData(XDocument xmlDocument) {
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

	private void LoadObjectFromXML(XDocument xmlDocument) {
		XElement translations = xmlDocument.Element(xmlRootNode);
		foreach (var el in translations.Elements())
		{
			string language = el.Name.LocalName;
			languages.Add(language, new Dictionary<string, string>());
			foreach (var tr in el.Elements())
			{
				languages[language].Add(tr.Name.LocalName, tr.Value);
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
		return data;
	}

	public XDocument SaveLocalizationDataToXML() {
		XDocument doc = new XDocument(new XElement(xmlRootNode));
		foreach (var langs in languages)
		{
			XElement el = new XElement(langs.Key, langs.Value.Select(kv => new XElement(kv.Key.Trim(), kv.Value.Trim())));
			doc.Element(xmlRootNode).Add(el);
		}
		return doc;
	}
}
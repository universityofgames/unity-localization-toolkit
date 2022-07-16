using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

[System.Serializable]
public class LocalizationData
{
	public Dictionary<string, Dictionary<string, string>> languages = new Dictionary<string, Dictionary<string, string>>();
	private string xmlRootNode = "translations";

	public LocalizationData() {
		languages = new Dictionary<string, Dictionary<string, string>>();
	}
	
	public LocalizationData(string defaultLanguage, string defaultKey) {
		languages = new Dictionary<string, Dictionary<string, string>>();
		languages.Add(defaultLanguage, new Dictionary<string, string>());
		languages[defaultLanguage].Add(defaultKey, "");
	}

	public LocalizationData(Dictionary<string, Dictionary<string, string>> jsonData)
	{
		languages = jsonData;
	}

	public LocalizationData(XDocument xmlDocument) {
		languages = new Dictionary<string, Dictionary<string, string>>();
		LoadObjectFromXML(xmlDocument);
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

	public Dictionary<string, Dictionary<string, string>> SaveLocalizationDataToJSON() {
		Dictionary<string, Dictionary<string, string>> jsonData = new Dictionary<string, Dictionary<string, string>>();
		foreach (var langs in languages) {
			jsonData.Add(langs.Key, langs.Value);
		}
		return jsonData;
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
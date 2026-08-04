using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>
	/// In-memory representation of a localization table: a set of languages, each mapping
	/// translation keys to translated values. Supports JSON, XML and CSV (de)serialization.
	/// </summary>
	/// <remarks>
	/// The language named <see cref="LocalizationManager.DefaultLanguageKey"/> acts as the
	/// fallback and as the source of the key list in the editor tooling. Use the static
	/// factory methods (<see cref="Parse"/>, <see cref="FromJson"/>, <see cref="FromXml"/>,
	/// <see cref="FromCsv"/>) to read files and the <c>To*</c> methods to write them.
	/// </remarks>
	[Serializable]
	public class LocalizationData
	{
		/// <summary>Name of the root element used by the XML representation.</summary>
		public const string XmlRootElement = "translations";

		/// <summary>Translation tables indexed by language key.</summary>
		[JsonProperty("languages")]
		public Dictionary<string, Dictionary<string, string>> Languages { get; set; } =
			new Dictionary<string, Dictionary<string, string>>();

		public LocalizationData()
		{
		}

		/// <summary>Creates a data set seeded with a single language and a single empty entry.</summary>
		/// <param name="defaultLanguage">Key of the initial language.</param>
		/// <param name="defaultKey">Initial translation key.</param>
		public LocalizationData(string defaultLanguage, string defaultKey)
		{
			Languages[defaultLanguage] = new Dictionary<string, string> { { defaultKey, string.Empty } };
		}

		/// <summary>Creates a data set that wraps existing translation tables.</summary>
		/// <param name="languages">Translation tables indexed by language key.</param>
		public LocalizationData(Dictionary<string, Dictionary<string, string>> languages)
		{
			Languages = languages ?? new Dictionary<string, Dictionary<string, string>>();
		}

		/// <summary>Parses raw text in the given format into a localization data set.</summary>
		/// <param name="rawData">Raw file contents.</param>
		/// <param name="format">Format of the contents.</param>
		/// <returns>The parsed localization data.</returns>
		/// <exception cref="FormatException">Thrown when the content is not valid for the given format.</exception>
		public static LocalizationData Parse(string rawData, LocalizationFileFormat format)
		{
			switch (format)
			{
				case LocalizationFileFormat.Json: return FromJson(rawData);
				case LocalizationFileFormat.Xml: return FromXml(rawData);
				case LocalizationFileFormat.Csv: return FromCsv(rawData);
				default: throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		/// <summary>Serializes the data set to text in the given format.</summary>
		/// <param name="format">Target file format.</param>
		/// <returns>The serialized file contents.</returns>
		public string Serialize(LocalizationFileFormat format)
		{
			switch (format)
			{
				case LocalizationFileFormat.Json: return ToJson();
				case LocalizationFileFormat.Xml: return ToXml();
				case LocalizationFileFormat.Csv: return ToCsv();
				default: throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		/// <summary>Parses the JSON representation (<c>{"languages": {...}}</c>).</summary>
		/// <param name="json">Raw JSON contents.</param>
		/// <returns>The parsed localization data.</returns>
		public static LocalizationData FromJson(string json)
		{
			return JsonConvert.DeserializeObject<LocalizationData>(json) ?? new LocalizationData();
		}

		/// <summary>Parses the XML representation rooted at <see cref="XmlRootElement"/>.</summary>
		/// <param name="xml">Raw XML contents.</param>
		/// <returns>The parsed localization data.</returns>
		/// <exception cref="FormatException">Thrown when the root element is missing.</exception>
		public static LocalizationData FromXml(string xml)
		{
			XElement root = XDocument.Parse(xml).Element(XmlRootElement);
			if (root == null)
				throw new FormatException($"XML document is missing the <{XmlRootElement}> root element.");

			var data = new LocalizationData();
			foreach (XElement languageElement in root.Elements())
			{
				var table = new Dictionary<string, string>();
				foreach (XElement entry in languageElement.Elements())
					table[entry.Name.LocalName] = entry.Value;

				data.Languages[languageElement.Name.LocalName] = table;
			}

			return data;
		}

		/// <summary>Parses the CSV representation (header: <c>key</c> followed by one column per language).</summary>
		/// <param name="csv">Raw CSV contents.</param>
		/// <returns>The parsed localization data.</returns>
		/// <exception cref="FormatException">Thrown when the header row is missing or has no language columns.</exception>
		public static LocalizationData FromCsv(string csv)
		{
			List<List<string>> rows = CsvUtility.Parse(csv);
			if (rows.Count == 0 || rows[0].Count < 2)
				throw new FormatException("CSV data needs a header row with a key column followed by one column per language.");

			var data = new LocalizationData();
			List<string> header = rows[0];
			for (int column = 1; column < header.Count; column++)
				data.Languages[header[column]] = new Dictionary<string, string>();

			for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
			{
				List<string> row = rows[rowIndex];
				if (row.Count == 0 || string.IsNullOrWhiteSpace(row[0]))
					continue;

				for (int column = 1; column < header.Count; column++)
					data.Languages[header[column]][row[0]] = column < row.Count ? row[column] : string.Empty;
			}

			return data;
		}

		/// <summary>Serializes the data set to indented JSON.</summary>
		/// <returns>The JSON file contents.</returns>
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		/// <summary>Serializes the data set to indented UTF-8 XML.</summary>
		/// <returns>The XML file contents, including the declaration.</returns>
		public string ToXml()
		{
			var document = new XDocument(new XElement(XmlRootElement,
				Languages.Select(language => new XElement(language.Key,
					language.Value.Select(entry => new XElement(entry.Key.Trim(), entry.Value?.Trim()))))));

			var settings = new XmlWriterSettings
			{
				Encoding = new UTF8Encoding(false),
				ConformanceLevel = ConformanceLevel.Document,
				OmitXmlDeclaration = false,
				Indent = true,
				IndentChars = "  ",
				NewLineHandling = NewLineHandling.Replace
			};

			using (var stringWriter = new Utf8StringWriter())
			{
				using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
					document.WriteTo(xmlWriter);

				return stringWriter.ToString();
			}
		}

		/// <summary>Serializes the data set to RFC 4180 compliant CSV.</summary>
		/// <returns>The CSV file contents.</returns>
		public string ToCsv()
		{
			List<string> languages = Languages.Keys.ToList();
			var keys = new List<string>();
			var seenKeys = new HashSet<string>();
			foreach (string language in languages)
			{
				foreach (string key in Languages[language].Keys)
				{
					if (seenKeys.Add(key))
						keys.Add(key);
				}
			}

			var builder = new StringBuilder();
			builder.AppendLine(CsvUtility.WriteRow(new[] { "key" }.Concat(languages)));
			foreach (string key in keys)
			{
				IEnumerable<string> values = languages.Select(language =>
					Languages[language].TryGetValue(key, out string value) ? value : string.Empty);
				builder.AppendLine(CsvUtility.WriteRow(new[] { key }.Concat(values)));
			}

			return builder.ToString();
		}

		private sealed class Utf8StringWriter : StringWriter
		{
			public override Encoding Encoding => Encoding.UTF8;
		}
	}
}

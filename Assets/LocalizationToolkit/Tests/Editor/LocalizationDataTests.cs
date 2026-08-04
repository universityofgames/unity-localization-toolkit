using System.Collections.Generic;
using NUnit.Framework;

namespace UniversityOfGames.LocalizationToolkit.Tests
{
	public class LocalizationDataTests
	{
		private static LocalizationData CreateSampleData()
		{
			return new LocalizationData(new Dictionary<string, Dictionary<string, string>>
			{
				["default"] = new Dictionary<string, string> { ["hello"] = "Hello", ["bye"] = "Bye" },
				["Polish"] = new Dictionary<string, string> { ["hello"] = "Cześć", ["bye"] = "Pa!" }
			});
		}

		private static void AssertSampleData(LocalizationData data)
		{
			Assert.That(data.Languages.Keys, Is.EquivalentTo(new[] { "default", "Polish" }));
			Assert.That(data.Languages["default"]["hello"], Is.EqualTo("Hello"));
			Assert.That(data.Languages["default"]["bye"], Is.EqualTo("Bye"));
			Assert.That(data.Languages["Polish"]["hello"], Is.EqualTo("Cześć"));
			Assert.That(data.Languages["Polish"]["bye"], Is.EqualTo("Pa!"));
		}

		[Test]
		public void JsonSerialization_RoundTripsAllLanguagesAndValues()
		{
			LocalizationData restored = LocalizationData.FromJson(CreateSampleData().ToJson());
			AssertSampleData(restored);
		}

		[Test]
		public void FromJson_ReadsLegacyFileShape()
		{
			const string json = "{\"languages\":{\"default\":{\"hello\":\"Hello\"}}}";
			LocalizationData data = LocalizationData.FromJson(json);
			Assert.That(data.Languages["default"]["hello"], Is.EqualTo("Hello"));
		}

		[Test]
		public void XmlSerialization_RoundTripsAllLanguagesAndValues()
		{
			LocalizationData restored = LocalizationData.FromXml(CreateSampleData().ToXml());
			AssertSampleData(restored);
		}

		[Test]
		public void FromXml_WithoutExpectedRootElement_Throws()
		{
			Assert.That(() => LocalizationData.FromXml("<wrong><default/></wrong>"),
				Throws.TypeOf<System.FormatException>());
		}

		[Test]
		public void CsvSerialization_RoundTripsAllLanguagesAndValues()
		{
			LocalizationData restored = LocalizationData.FromCsv(CreateSampleData().ToCsv());
			AssertSampleData(restored);
		}

		[Test]
		public void CsvSerialization_RoundTripsFieldsWithSeparatorsQuotesAndNewlines()
		{
			var data = new LocalizationData(new Dictionary<string, Dictionary<string, string>>
			{
				["default"] = new Dictionary<string, string>
				{
					["greeting"] = "Hello, \"world\"",
					["multiline"] = "First line\nSecond line"
				}
			});

			LocalizationData restored = LocalizationData.FromCsv(data.ToCsv());
			Assert.That(restored.Languages["default"]["greeting"], Is.EqualTo("Hello, \"world\""));
			Assert.That(restored.Languages["default"]["multiline"], Is.EqualTo("First line\nSecond line"));
		}

		[Test]
		public void FromCsv_MissingCells_BecomeEmptyValues()
		{
			const string csv = "key,default,Polish\nhello,Hello\n";
			LocalizationData data = LocalizationData.FromCsv(csv);
			Assert.That(data.Languages["Polish"]["hello"], Is.EqualTo(string.Empty));
		}

		[Test]
		public void FromCsv_WithoutLanguageColumns_Throws()
		{
			Assert.That(() => LocalizationData.FromCsv("key\nhello\n"),
				Throws.TypeOf<System.FormatException>());
		}

		[Test]
		public void SeededConstructor_CreatesLanguageWithSingleEmptyEntry()
		{
			var data = new LocalizationData("default", "NEW_KEY");
			Assert.That(data.Languages["default"]["NEW_KEY"], Is.EqualTo(string.Empty));
		}
	}
}

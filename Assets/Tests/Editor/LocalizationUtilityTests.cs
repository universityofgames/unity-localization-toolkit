using NUnit.Framework;

namespace UniversityOfGames.LocalizationToolkit.Tests
{
	public class LocalizationUtilityTests
	{
		[TestCase("json", LocalizationFileFormat.Json)]
		[TestCase(".json", LocalizationFileFormat.Json)]
		[TestCase("XML", LocalizationFileFormat.Xml)]
		[TestCase("csv", LocalizationFileFormat.Csv)]
		public void TryParseExtension_ResolvesKnownExtensions(string extension, LocalizationFileFormat expected)
		{
			Assert.That(LocalizationFileFormatUtility.TryParseExtension(extension, out LocalizationFileFormat format), Is.True);
			Assert.That(format, Is.EqualTo(expected));
		}

		[TestCase("txt")]
		[TestCase("")]
		[TestCase(null)]
		public void TryParseExtension_RejectsUnknownExtensions(string extension)
		{
			Assert.That(LocalizationFileFormatUtility.TryParseExtension(extension, out _), Is.False);
		}

		[Test]
		public void ToExtension_MatchesParseExtension()
		{
			foreach (LocalizationFileFormat format in System.Enum.GetValues(typeof(LocalizationFileFormat)))
			{
				Assert.That(LocalizationFileFormatUtility.TryParseExtension(format.ToExtension(), out LocalizationFileFormat parsed), Is.True);
				Assert.That(parsed, Is.EqualTo(format));
			}
		}

		[TestCase("https://example.com/lang.json", LocalizationFileFormat.Json)]
		[TestCase("https://example.com/lang.xml?version=2", LocalizationFileFormat.Xml)]
		[TestCase("https://example.com/lang.csv#section", LocalizationFileFormat.Csv)]
		public void TryGetFileFormatFromUrl_ResolvesSupportedUrls(string url, LocalizationFileFormat expected)
		{
			Assert.That(RemoteFileLoader.TryGetFileFormatFromUrl(url, out LocalizationFileFormat format), Is.True);
			Assert.That(format, Is.EqualTo(expected));
		}

		[TestCase("https://example.com/lang")]
		[TestCase("https://example.com/lang.txt")]
		[TestCase("")]
		public void TryGetFileFormatFromUrl_RejectsUnsupportedUrls(string url)
		{
			Assert.That(RemoteFileLoader.TryGetFileFormatFromUrl(url, out _), Is.False);
		}

		[Test]
		public void ApplyTokens_ReplacesEveryPlaceholder()
		{
			string result = LocalizationTextFormatter.ApplyTokens(
				"Hello {name}, you are {age} years old. Bye {name}!",
				("name", "Anna"), ("age", "30"));

			Assert.That(result, Is.EqualTo("Hello Anna, you are 30 years old. Bye Anna!"));
		}

		[Test]
		public void ApplyTokens_WithoutTokens_ReturnsOriginalText()
		{
			Assert.That(LocalizationTextFormatter.ApplyTokens("Hello {name}"), Is.EqualTo("Hello {name}"));
		}

		[Test]
		public void ApplyTokens_NullTokenValue_ReplacesWithEmptyString()
		{
			Assert.That(LocalizationTextFormatter.ApplyTokens("Hi {name}", ("name", null)), Is.EqualTo("Hi "));
		}
	}
}

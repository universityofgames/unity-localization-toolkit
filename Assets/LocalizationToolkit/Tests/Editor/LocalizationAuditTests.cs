using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UniversityOfGames.LocalizationToolkit.Editor;

namespace UniversityOfGames.LocalizationToolkit.Tests
{
	public class LocalizationAuditTests
	{
		private static LocalizationData CreateData()
		{
			return new LocalizationData(new Dictionary<string, Dictionary<string, string>>
			{
				["default"] = new Dictionary<string, string>
				{
					["hello"] = "Hello",
					["bye"] = "Bye",
					["title"] = "Title"
				},
				["Polish"] = new Dictionary<string, string>
				{
					["hello"] = "Cześć",
					["bye"] = string.Empty,
					["title"] = "   "
				}
			});
		}

		[Test]
		public void GetStatistics_CountsNonEmptyValuesPerLanguage()
		{
			List<LocalizationAudit.LanguageStatistics> statistics =
				LocalizationAudit.GetStatistics(CreateData(), "default");

			LocalizationAudit.LanguageStatistics defaultStats = statistics.First(s => s.Language == "default");
			LocalizationAudit.LanguageStatistics polishStats = statistics.First(s => s.Language == "Polish");

			Assert.That(defaultStats.Filled, Is.EqualTo(3));
			Assert.That(defaultStats.Total, Is.EqualTo(3));
			Assert.That(polishStats.Filled, Is.EqualTo(1), "Empty and whitespace-only values must not count as filled.");
			Assert.That(polishStats.Completion, Is.EqualTo(1f / 3f).Within(0.001f));
		}

		[Test]
		public void GetKeysMissingFromData_ReturnsSortedDistinctUnknownKeys()
		{
			var contentKeys = new[] { "hello", "score_label", "pause_menu", "score_label", "" };

			List<string> missing = LocalizationAudit.GetKeysMissingFromData(contentKeys, CreateData(), "default");

			Assert.That(missing, Is.EqualTo(new[] { "pause_menu", "score_label" }));
		}

		[Test]
		public void GetUnusedKeys_ReturnsDataKeysAbsentFromContent()
		{
			var contentKeys = new[] { "hello" };

			List<string> unused = LocalizationAudit.GetUnusedKeys(contentKeys, CreateData(), "default");

			Assert.That(unused, Is.EqualTo(new[] { "bye", "title" }));
		}
	}
}

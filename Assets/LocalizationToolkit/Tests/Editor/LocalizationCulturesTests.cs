using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Tests
{
	public class LocalizationCulturesTests
	{
		[TestCase("Polish", "pl-PL")]
		[TestCase("German", "de-DE")]
		[TestCase("Japanese", "ja-JP")]
		public void GetCulture_MapsKnownLanguages(string language, string expectedCulture)
		{
			Assert.That(LocalizationCultures.GetCulture(language).Name, Is.EqualTo(expectedCulture));
		}

		[TestCase("Klingon")]
		[TestCase("")]
		[TestCase(null)]
		public void GetCulture_FallsBackToInvariant(string language)
		{
			Assert.That(LocalizationCultures.GetCulture(language), Is.EqualTo(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ApplyTokens_FormatsNumbersWithTheGivenCulture()
		{
			CultureInfo polish = LocalizationCultures.GetCulture("Polish");
			string expected = "Total: " + 1234.5.ToString("N1", polish);

			Assert.That(LocalizationTextFormatter.ApplyTokens("Total: {value:N1}", polish, ("value", 1234.5)),
				Is.EqualTo(expected));
		}

		[Test]
		public void ApplyTokens_FormatsCurrencyWithTheGivenCulture()
		{
			CultureInfo german = LocalizationCultures.GetCulture("German");
			string expected = "Price: " + 9.99m.ToString("C", german);

			Assert.That(LocalizationTextFormatter.ApplyTokens("Price: {price:C}", german, ("price", 9.99m)),
				Is.EqualTo(expected));
		}

		[Test]
		public void ApplyTokens_FormatsDatesWithTheGivenCulture()
		{
			CultureInfo french = LocalizationCultures.GetCulture("French");
			var date = new DateTime(2026, 8, 4);
			string expected = "Date: " + date.ToString("d", french);

			Assert.That(LocalizationTextFormatter.ApplyTokens("Date: {date:d}", french, ("date", date)),
				Is.EqualTo(expected));
		}

		[Test]
		public void ApplyTokens_LeavesUnknownPlaceholdersUntouched()
		{
			Assert.That(LocalizationTextFormatter.ApplyTokens("{known} and {unknown}",
					CultureInfo.InvariantCulture, ("known", 1)),
				Is.EqualTo("1 and {unknown}"));
		}

		[Test]
		public void FormatLocalized_UsesTheActiveLanguageCulture()
		{
			var gameObject = new GameObject("CultureManager");
			try
			{
				var manager = gameObject.AddComponent<LocalizationManager>();
				manager.LoadData(new LocalizationData(new Dictionary<string, Dictionary<string, string>>
				{
					["default"] = new Dictionary<string, string> { ["total"] = "Total: {price:N2}" },
					["Polish"] = new Dictionary<string, string> { ["total"] = "Suma: {price:N2}" }
				}));

				manager.LoadLanguage("Polish");
				string expected = "Suma: " + 1234.56m.ToString("N2", LocalizationCultures.GetCulture("Polish"));

				Assert.That(manager.FormatLocalized("total", ("price", 1234.56m)), Is.EqualTo(expected));
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
		}

		[Test]
		public void IsRightToLeft_DefaultsToArabicAndHebrew()
		{
			var gameObject = new GameObject("RtlManager");
			try
			{
				var manager = gameObject.AddComponent<LocalizationManager>();
				Assert.That(manager.IsRightToLeft("Arabic"), Is.True);
				Assert.That(manager.IsRightToLeft("Hebrew"), Is.True);
				Assert.That(manager.IsRightToLeft("Polish"), Is.False);
				Assert.That(manager.IsRightToLeft(null), Is.False);
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
		}
	}
}

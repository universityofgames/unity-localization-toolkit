using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace UniversityOfGames.LocalizationToolkit.Tests
{
	public class LocalizedFontTests
	{
		private GameObject _textObject;
		private GameObject _managerObject;

		[TearDown]
		public void TearDown()
		{
			if (_textObject != null)
				Object.DestroyImmediate(_textObject);
			if (_managerObject != null)
				Object.DestroyImmediate(_managerObject);
		}

		[Test]
		public void GetEntryForLanguage_ReturnsMatchingEntryOrNull()
		{
			_textObject = new GameObject("Text", typeof(Text));
			var localized = _textObject.AddComponent<LocalizedFont>();
			var entry = new LocalizedFontEntry { Language = "Japanese", SizeMultiplier = 0.9f };
			localized.Overrides.Add(entry);

			Assert.That(localized.GetEntryForLanguage("Japanese"), Is.SameAs(entry));
			Assert.That(localized.GetEntryForLanguage("Polish"), Is.Null);
			Assert.That(localized.GetEntryForLanguage(null), Is.Null);
		}

		[Test]
		public void Refresh_AppliesSizeMultiplierOfActiveLanguage()
		{
			_textObject = new GameObject("Text", typeof(Text));
			var text = _textObject.GetComponent<Text>();
			text.fontSize = 20;

			var localized = _textObject.AddComponent<LocalizedFont>();
			localized.Overrides.Add(new LocalizedFontEntry { Language = "Polish", SizeMultiplier = 1.5f });

			_managerObject = new GameObject("Manager");
			var manager = _managerObject.AddComponent<LocalizationManager>();
			manager.LoadData(new LocalizationData(new Dictionary<string, Dictionary<string, string>>
			{
				["default"] = new Dictionary<string, string> { ["hello"] = "Hello" },
				["Polish"] = new Dictionary<string, string> { ["hello"] = "Cześć" }
			}));

			manager.LoadLanguage("Polish");
			Assert.That(text.fontSize, Is.EqualTo(30));

			manager.LoadLanguage("default");
			Assert.That(text.fontSize, Is.EqualTo(20));
		}
	}
}

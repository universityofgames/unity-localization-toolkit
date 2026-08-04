using System.Collections.Generic;
using NUnit.Framework;

namespace UniversityOfGames.LocalizationToolkit.Tests
{
	public class LocalizationManagerTests
	{
		private static readonly string[] Available = { "default", "English", "Polish" };

		[Test]
		public void ResolveStartupLanguage_PrefersSavedLanguage()
		{
			Assert.That(LocalizationManager.ResolveStartupLanguage("Polish", "English", true, Available),
				Is.EqualTo("Polish"));
		}

		[Test]
		public void ResolveStartupLanguage_IgnoresSavedLanguageMissingFromData()
		{
			Assert.That(LocalizationManager.ResolveStartupLanguage("German", "English", true, Available),
				Is.EqualTo("English"));
		}

		[Test]
		public void ResolveStartupLanguage_WithoutSavedChoice_UsesSystemLanguage()
		{
			Assert.That(LocalizationManager.ResolveStartupLanguage(null, "Polish", true, Available),
				Is.EqualTo("Polish"));
		}

		[Test]
		public void ResolveStartupLanguage_WithDetectionDisabled_FallsBackToDefault()
		{
			Assert.That(LocalizationManager.ResolveStartupLanguage(null, "Polish", false, Available),
				Is.EqualTo(LocalizationManager.DefaultLanguageKey));
		}

		[Test]
		public void ResolveStartupLanguage_WithUnknownSystemLanguage_FallsBackToDefault()
		{
			Assert.That(LocalizationManager.ResolveStartupLanguage(null, "Klingon", true, Available),
				Is.EqualTo(LocalizationManager.DefaultLanguageKey));
		}

		[Test]
		public void ResolveStartupLanguage_SavedLanguageWinsOverDetectionSetting()
		{
			Assert.That(LocalizationManager.ResolveStartupLanguage("Polish", "English", false, Available),
				Is.EqualTo("Polish"));
		}

		[Test]
		public void OnLanguageChanged_UnityEventFiresOnEveryLanguageSwitch()
		{
			var gameObject = new UnityEngine.GameObject("EventManager");
			try
			{
				var manager = gameObject.AddComponent<LocalizationManager>();
				int invocations = 0;
				manager.OnLanguageChanged.AddListener(() => invocations++);

				manager.LoadData(new LocalizationData(new Dictionary<string, Dictionary<string, string>>
				{
					["default"] = new Dictionary<string, string> { ["hello"] = "Hello" },
					["Polish"] = new Dictionary<string, string> { ["hello"] = "Cześć" }
				}));
				int afterLoad = invocations;

				manager.LoadLanguage("Polish");

				Assert.That(afterLoad, Is.GreaterThanOrEqualTo(1), "Loading data activates a language and must fire the event.");
				Assert.That(invocations, Is.EqualTo(afterLoad + 1));
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
		}
	}
}

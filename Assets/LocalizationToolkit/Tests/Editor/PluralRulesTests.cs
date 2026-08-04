using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Tests
{
	public class PluralRulesTests
	{
		[TestCase(1, PluralCategory.One)]
		[TestCase(2, PluralCategory.Few)]
		[TestCase(4, PluralCategory.Few)]
		[TestCase(5, PluralCategory.Many)]
		[TestCase(11, PluralCategory.Many)]
		[TestCase(12, PluralCategory.Many)]
		[TestCase(14, PluralCategory.Many)]
		[TestCase(22, PluralCategory.Few)]
		[TestCase(25, PluralCategory.Many)]
		[TestCase(0, PluralCategory.Many)]
		public void Resolve_Polish(int count, PluralCategory expected)
		{
			Assert.That(PluralRules.Resolve("Polish", count), Is.EqualTo(expected));
		}

		[TestCase(1, PluralCategory.One)]
		[TestCase(21, PluralCategory.One)]
		[TestCase(11, PluralCategory.Many)]
		[TestCase(3, PluralCategory.Few)]
		[TestCase(113, PluralCategory.Many)]
		[TestCase(5, PluralCategory.Many)]
		public void Resolve_Russian(int count, PluralCategory expected)
		{
			Assert.That(PluralRules.Resolve("Russian", count), Is.EqualTo(expected));
		}

		[TestCase(1, PluralCategory.One)]
		[TestCase(3, PluralCategory.Few)]
		[TestCase(5, PluralCategory.Other)]
		public void Resolve_Czech(int count, PluralCategory expected)
		{
			Assert.That(PluralRules.Resolve("Czech", count), Is.EqualTo(expected));
		}

		[TestCase(0, PluralCategory.One)]
		[TestCase(1, PluralCategory.One)]
		[TestCase(2, PluralCategory.Other)]
		public void Resolve_French(int count, PluralCategory expected)
		{
			Assert.That(PluralRules.Resolve("French", count), Is.EqualTo(expected));
		}

		[TestCase(0, PluralCategory.Zero)]
		[TestCase(1, PluralCategory.One)]
		[TestCase(2, PluralCategory.Two)]
		[TestCase(3, PluralCategory.Few)]
		[TestCase(10, PluralCategory.Few)]
		[TestCase(11, PluralCategory.Many)]
		[TestCase(99, PluralCategory.Many)]
		[TestCase(100, PluralCategory.Other)]
		public void Resolve_Arabic(int count, PluralCategory expected)
		{
			Assert.That(PluralRules.Resolve("Arabic", count), Is.EqualTo(expected));
		}

		[TestCase(1)]
		[TestCase(5)]
		public void Resolve_Japanese_IsAlwaysOther(int count)
		{
			Assert.That(PluralRules.Resolve("Japanese", count), Is.EqualTo(PluralCategory.Other));
		}

		[TestCase(1, PluralCategory.One)]
		[TestCase(2, PluralCategory.Other)]
		[TestCase(0, PluralCategory.Other)]
		public void Resolve_EnglishLikeDefault(int count, PluralCategory expected)
		{
			Assert.That(PluralRules.Resolve("German", count), Is.EqualTo(expected));
			Assert.That(PluralRules.Resolve("Klingon", count), Is.EqualTo(expected));
		}

		[Test]
		public void GetPlural_ResolvesSuffixedKeysWithFallbacks()
		{
			var gameObject = new GameObject("PluralManager");
			try
			{
				var manager = gameObject.AddComponent<LocalizationManager>();
				manager.LoadData(new LocalizationData(new Dictionary<string, Dictionary<string, string>>
				{
					["default"] = new Dictionary<string, string>
					{
						["apples.one"] = "{count} apple",
						["apples.other"] = "{count} apples",
						["coins"] = "{count} coins"
					},
					["Polish"] = new Dictionary<string, string>
					{
						["apples.one"] = "{count} jabłko",
						["apples.few"] = "{count} jabłka",
						["apples.many"] = "{count} jabłek"
					}
				}));

				manager.LoadLanguage("Polish");
				Assert.That(manager.GetPlural("apples", 1), Is.EqualTo("1 jabłko"));
				Assert.That(manager.GetPlural("apples", 2), Is.EqualTo("2 jabłka"));
				Assert.That(manager.GetPlural("apples", 5), Is.EqualTo("5 jabłek"));
				Assert.That(manager.GetPlural("apples", 22), Is.EqualTo("22 jabłka"));

				manager.LoadLanguage("default");
				Assert.That(manager.GetPlural("apples", 1), Is.EqualTo("1 apple"));
				Assert.That(manager.GetPlural("apples", 7), Is.EqualTo("7 apples"));
				Assert.That(manager.GetPlural("coins", 3), Is.EqualTo("3 coins"),
					"A bare key must act as the last fallback.");
			}
			finally
			{
				Object.DestroyImmediate(gameObject);
			}
		}
	}
}

using NUnit.Framework;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Tests
{
	public class LocalizedImageTests
	{
		private GameObject _gameObject;
		private Texture2D _texture;
		private Sprite _defaultSprite;
		private Sprite _polishSprite;

		[SetUp]
		public void SetUp()
		{
			_gameObject = new GameObject("LocalizedImageTest", typeof(SpriteRenderer));
			_texture = new Texture2D(4, 4);
			_defaultSprite = Sprite.Create(_texture, new Rect(0f, 0f, 4f, 4f), Vector2.zero);
			_polishSprite = Sprite.Create(_texture, new Rect(0f, 0f, 2f, 2f), Vector2.zero);
		}

		[TearDown]
		public void TearDown()
		{
			Object.DestroyImmediate(_gameObject);
			Object.DestroyImmediate(_defaultSprite);
			Object.DestroyImmediate(_polishSprite);
			Object.DestroyImmediate(_texture);
		}

		private LocalizedImage CreateComponent()
		{
			var localized = _gameObject.AddComponent<LocalizedImage>();
			localized.DefaultSprite = _defaultSprite;
			localized.Overrides.Add(new LocalizedSpriteEntry { Language = "Polish", Sprite = _polishSprite });
			return localized;
		}

		[Test]
		public void GetSpriteForLanguage_WithOverride_ReturnsOverrideSprite()
		{
			Assert.That(CreateComponent().GetSpriteForLanguage("Polish"), Is.EqualTo(_polishSprite));
		}

		[Test]
		public void GetSpriteForLanguage_WithoutOverride_FallsBackToDefault()
		{
			Assert.That(CreateComponent().GetSpriteForLanguage("German"), Is.EqualTo(_defaultSprite));
		}

		[Test]
		public void GetSpriteForLanguage_WithNullLanguage_ReturnsDefault()
		{
			Assert.That(CreateComponent().GetSpriteForLanguage(null), Is.EqualTo(_defaultSprite));
		}

		[Test]
		public void GetSpriteForLanguage_IgnoresOverridesWithoutSprite()
		{
			LocalizedImage localized = CreateComponent();
			localized.Overrides.Add(new LocalizedSpriteEntry { Language = "German", Sprite = null });
			Assert.That(localized.GetSpriteForLanguage("German"), Is.EqualTo(_defaultSprite));
		}
	}
}

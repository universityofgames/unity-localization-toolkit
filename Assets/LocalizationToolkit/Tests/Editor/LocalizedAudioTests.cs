using NUnit.Framework;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Tests
{
	public class LocalizedAudioTests
	{
		private GameObject _gameObject;
		private AudioClip _defaultClip;
		private AudioClip _polishClip;

		[SetUp]
		public void SetUp()
		{
			_gameObject = new GameObject("LocalizedAudioTest", typeof(AudioSource));
			_defaultClip = AudioClip.Create("DefaultClip", 44100, 1, 44100, false);
			_polishClip = AudioClip.Create("PolishClip", 44100, 1, 44100, false);
		}

		[TearDown]
		public void TearDown()
		{
			Object.DestroyImmediate(_gameObject);
			Object.DestroyImmediate(_defaultClip);
			Object.DestroyImmediate(_polishClip);
		}

		private LocalizedAudio CreateComponent()
		{
			var localized = _gameObject.AddComponent<LocalizedAudio>();
			localized.DefaultClip = _defaultClip;
			localized.Overrides.Add(new LocalizedAudioEntry { Language = "Polish", Clip = _polishClip });
			return localized;
		}

		[Test]
		public void GetClipForLanguage_WithOverride_ReturnsOverrideClip()
		{
			Assert.That(CreateComponent().GetClipForLanguage("Polish"), Is.EqualTo(_polishClip));
		}

		[Test]
		public void GetClipForLanguage_WithoutOverride_FallsBackToDefault()
		{
			Assert.That(CreateComponent().GetClipForLanguage("German"), Is.EqualTo(_defaultClip));
			Assert.That(CreateComponent().GetClipForLanguage(null), Is.EqualTo(_defaultClip));
		}

		[Test]
		public void Refresh_AssignsClipToTheAudioSource()
		{
			LocalizedAudio localized = CreateComponent();
			localized.Refresh();

			Assert.That(_gameObject.GetComponent<AudioSource>().clip, Is.EqualTo(_defaultClip));
		}
	}
}

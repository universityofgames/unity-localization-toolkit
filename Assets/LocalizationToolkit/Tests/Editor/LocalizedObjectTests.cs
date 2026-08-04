using NUnit.Framework;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Tests
{
	public class LocalizedObjectTests
	{
		private GameObject _root;
		private GameObject _defaultTarget;
		private GameObject _polishTarget;

		[SetUp]
		public void SetUp()
		{
			_root = new GameObject("LocalizedObjectTest");
			_defaultTarget = new GameObject("DefaultTarget");
			_polishTarget = new GameObject("PolishTarget");
			_defaultTarget.transform.SetParent(_root.transform);
			_polishTarget.transform.SetParent(_root.transform);
		}

		[TearDown]
		public void TearDown()
		{
			Object.DestroyImmediate(_root);
		}

		private LocalizedObject CreateComponent()
		{
			var localized = _root.AddComponent<LocalizedObject>();
			localized.DefaultTarget = _defaultTarget;
			localized.Entries.Add(new LocalizedObjectEntry { Language = "Polish", Target = _polishTarget });
			return localized;
		}

		[Test]
		public void GetTargetForLanguage_WithEntry_ReturnsEntryObject()
		{
			Assert.That(CreateComponent().GetTargetForLanguage("Polish"), Is.EqualTo(_polishTarget));
		}

		[Test]
		public void GetTargetForLanguage_WithoutEntry_FallsBackToDefault()
		{
			LocalizedObject localized = CreateComponent();
			Assert.That(localized.GetTargetForLanguage("German"), Is.EqualTo(_defaultTarget));
			Assert.That(localized.GetTargetForLanguage(null), Is.EqualTo(_defaultTarget));
		}

		[Test]
		public void Refresh_ActivatesOnlyTheResolvedObject()
		{
			LocalizedObject localized = CreateComponent();
			localized.Refresh();

			Assert.That(_defaultTarget.activeSelf, Is.True, "Without a manager the default object must be active.");
			Assert.That(_polishTarget.activeSelf, Is.False, "Non-matching entries must be deactivated.");
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>GameObject shown for a single language.</summary>
	[Serializable]
	public class LocalizedObjectEntry
	{
		[Tooltip("Language key the object applies to, e.g. 'Polish'.")]
		public string Language;

		[Tooltip("Object active while that language is active.")]
		public GameObject Target;
	}

	/// <summary>
	/// Activates exactly one GameObject per language — for language-specific layouts,
	/// decorations or any content that cannot be expressed as a simple text or sprite swap.
	/// </summary>
	/// <remarks>
	/// Add an entry per language; while that language is active only its object is
	/// enabled and all other configured objects are disabled. Languages without an
	/// entry activate the default object. Activation runs at play time, on enable and
	/// on every language change.
	/// </remarks>
	[DisallowMultipleComponent]
	[AddComponentMenu("Localization Toolkit/Localized Object")]
	[HelpURL(LocalizationToolkitInfo.DocumentationUrl)]
	public class LocalizedObject : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("Object active when no entry matches the active language.")]
		private GameObject _defaultTarget;

		[SerializeField]
		[Tooltip("Per-language objects; exactly one is active at a time.")]
		private List<LocalizedObjectEntry> _entries = new List<LocalizedObjectEntry>();

		/// <summary>Object active when no entry matches the active language.</summary>
		public GameObject DefaultTarget
		{
			get => _defaultTarget;
			set
			{
				_defaultTarget = value;
				Refresh();
			}
		}

		/// <summary>Per-language objects. Call <see cref="Refresh"/> after modifying the list.</summary>
		public List<LocalizedObjectEntry> Entries => _entries;

		private void OnEnable()
		{
			LocalizationManager.LanguageChanged += Refresh;
			Refresh();
		}

		private void OnDisable()
		{
			LocalizationManager.LanguageChanged -= Refresh;
		}

		/// <summary>Resolves the object that should be active for the given language.</summary>
		/// <param name="languageKey">Language key to resolve, or null for the default object.</param>
		/// <returns>The matching entry's object, or the default object when no entry applies.</returns>
		public GameObject GetTargetForLanguage(string languageKey)
		{
			if (!string.IsNullOrEmpty(languageKey))
			{
				foreach (LocalizedObjectEntry entry in _entries)
				{
					if (entry != null && entry.Target != null && entry.Language == languageKey)
						return entry.Target;
				}
			}

			return _defaultTarget;
		}

		/// <summary>Activates the object of the active language and deactivates all others.</summary>
		public void Refresh()
		{
			LocalizationManager manager = LocalizationManager.Instance;
			string language = manager != null && manager.IsLoaded ? manager.ActiveLanguage : null;
			GameObject expected = GetTargetForLanguage(language);

			foreach (LocalizedObjectEntry entry in _entries)
			{
				if (entry != null && entry.Target != null)
					entry.Target.SetActive(entry.Target == expected);
			}

			if (_defaultTarget != null)
				_defaultTarget.SetActive(_defaultTarget == expected);
		}
	}
}

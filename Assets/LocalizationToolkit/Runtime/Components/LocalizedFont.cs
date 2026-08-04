using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>Font override applied for a single language.</summary>
	[Serializable]
	public class LocalizedFontEntry
	{
		[Tooltip("Language key the fonts apply to, e.g. 'Japanese'.")]
		public string Language;

		[Tooltip("TextMeshPro font asset used while that language is active.")]
		public TMP_FontAsset TmpFont;

		[Tooltip("Legacy UI font used while that language is active.")]
		public Font LegacyFont;

		[Min(0.1f)]
		[Tooltip("Multiplier applied to the original font size, e.g. 0.9 for dense scripts.")]
		public float SizeMultiplier = 1f;
	}

	/// <summary>
	/// Swaps the font of a <see cref="TMP_Text"/> or legacy <see cref="Text"/> component
	/// to match the active language — essential for CJK, Cyrillic, Thai or Arabic, whose
	/// glyphs are usually missing from Latin font assets.
	/// </summary>
	/// <remarks>
	/// Add an override per language that needs a different font asset; languages without
	/// an override use the default font. When the default fields are left empty, the
	/// component captures the font present on first use. The optional size multiplier
	/// compensates scripts that render larger or denser than the original font.
	/// </remarks>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[AddComponentMenu("Localization Toolkit/Localized Font")]
	[HelpURL(LocalizationToolkitInfo.DocumentationUrl)]
	public class LocalizedFont : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("TextMeshPro font used when no override matches. Captured automatically when left empty.")]
		private TMP_FontAsset _defaultTmpFont;

		[SerializeField]
		[Tooltip("Legacy font used when no override matches. Captured automatically when left empty.")]
		private Font _defaultLegacyFont;

		[SerializeField]
		[Tooltip("Per-language font overrides.")]
		private List<LocalizedFontEntry> _overrides = new List<LocalizedFontEntry>();

		private TMP_Text _tmpText;
		private Text _uiText;
		private float _originalTmpSize;
		private int _originalLegacySize;
		private bool _originalsCaptured;

		/// <summary>Per-language font overrides. Call <see cref="Refresh"/> after modifying the list.</summary>
		public List<LocalizedFontEntry> Overrides => _overrides;

		private void OnEnable()
		{
			LocalizationManager.LanguageChanged += Refresh;
			Refresh();
		}

		private void OnDisable()
		{
			LocalizationManager.LanguageChanged -= Refresh;
		}

		/// <summary>Returns the override configured for the given language, or null.</summary>
		/// <param name="languageKey">Language key to resolve.</param>
		/// <returns>The matching entry, or null when the language has no override.</returns>
		public LocalizedFontEntry GetEntryForLanguage(string languageKey)
		{
			if (!string.IsNullOrEmpty(languageKey))
			{
				foreach (LocalizedFontEntry entry in _overrides)
				{
					if (entry != null && entry.Language == languageKey)
						return entry;
				}
			}

			return null;
		}

		/// <summary>Re-applies the font of the active language to the attached text component.</summary>
		public void Refresh()
		{
			CacheTargetComponents();

			LocalizationManager manager = LocalizationManager.Instance;
			string language = manager != null && manager.IsLoaded ? manager.ActiveLanguage : null;
			LocalizedFontEntry entry = GetEntryForLanguage(language);
			float multiplier = entry != null ? entry.SizeMultiplier : 1f;

			if (_tmpText != null)
			{
				TMP_FontAsset font = entry != null && entry.TmpFont != null ? entry.TmpFont : _defaultTmpFont;
				if (font != null)
					_tmpText.font = font;
				_tmpText.fontSize = _originalTmpSize * multiplier;
			}
			else if (_uiText != null)
			{
				Font font = entry != null && entry.LegacyFont != null ? entry.LegacyFont : _defaultLegacyFont;
				if (font != null)
					_uiText.font = font;
				_uiText.fontSize = Mathf.RoundToInt(_originalLegacySize * multiplier);
			}
		}

		private void CacheTargetComponents()
		{
			if (_tmpText == null && _uiText == null)
			{
				_tmpText = GetComponent<TMP_Text>();
				_uiText = GetComponent<Text>();
				if (_tmpText == null && _uiText == null)
				{
					Debug.LogWarning("[LocalizationToolkit] LocalizedFont requires a Text or TMP_Text component on the same GameObject.", this);
					return;
				}
			}

			if (!_originalsCaptured)
			{
				if (_tmpText != null)
				{
					_originalTmpSize = _tmpText.fontSize;
					if (_defaultTmpFont == null)
						_defaultTmpFont = _tmpText.font;
				}

				if (_uiText != null)
				{
					_originalLegacySize = _uiText.fontSize;
					if (_defaultLegacyFont == null)
						_defaultLegacyFont = _uiText.font;
				}

				_originalsCaptured = true;
			}
		}
	}
}

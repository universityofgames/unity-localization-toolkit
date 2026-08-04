using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>Sprite override shown for a single language.</summary>
	[Serializable]
	public class LocalizedSpriteEntry
	{
		[Tooltip("Language key the sprite applies to, e.g. 'Polish'.")]
		public string Language;

		[Tooltip("Sprite shown while that language is active.")]
		public Sprite Sprite;
	}

	/// <summary>
	/// Swaps the sprite of a UI <see cref="Image"/> or a <see cref="SpriteRenderer"/>
	/// to match the active language — for localized logos, flags, banners or any
	/// artwork containing text.
	/// </summary>
	/// <remarks>
	/// Assign a default sprite and add an override per language that needs different
	/// artwork. Languages without an override fall back to the default sprite. The
	/// sprite refreshes when the component is enabled and every time
	/// <see cref="LocalizationManager.LanguageChanged"/> is raised; in the editor,
	/// language switches preview directly in the Scene view.
	/// </remarks>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[AddComponentMenu("Localization Toolkit/Localized Image")]
	[HelpURL(LocalizationToolkitInfo.DocumentationUrl)]
	public class LocalizedImage : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("Sprite used when no override matches the active language.")]
		private Sprite _defaultSprite;

		[SerializeField]
		[Tooltip("Per-language sprite overrides.")]
		private List<LocalizedSpriteEntry> _overrides = new List<LocalizedSpriteEntry>();

		private Image _image;
		private SpriteRenderer _spriteRenderer;

		/// <summary>Sprite used when no override matches the active language. Setting it refreshes the target.</summary>
		public Sprite DefaultSprite
		{
			get => _defaultSprite;
			set
			{
				_defaultSprite = value;
				Refresh();
			}
		}

		/// <summary>Per-language sprite overrides. Call <see cref="Refresh"/> after modifying the list.</summary>
		public List<LocalizedSpriteEntry> Overrides => _overrides;

		private void OnEnable()
		{
			LocalizationManager.LanguageChanged += Refresh;
			Refresh();
		}

		private void OnDisable()
		{
			LocalizationManager.LanguageChanged -= Refresh;
		}

		/// <summary>Resolves the sprite that should be shown for the given language.</summary>
		/// <param name="languageKey">Language key to resolve, or null for the default sprite.</param>
		/// <returns>The matching override sprite, or the default sprite when no override applies.</returns>
		public Sprite GetSpriteForLanguage(string languageKey)
		{
			if (!string.IsNullOrEmpty(languageKey))
			{
				foreach (LocalizedSpriteEntry entry in _overrides)
				{
					if (entry != null && entry.Sprite != null && entry.Language == languageKey)
						return entry.Sprite;
				}
			}

			return _defaultSprite;
		}

		/// <summary>Re-applies the sprite of the active language to the attached renderer.</summary>
		public void Refresh()
		{
			LocalizationManager manager = LocalizationManager.Instance;
			string language = manager != null && manager.IsLoaded ? manager.ActiveLanguage : null;

			Sprite sprite = GetSpriteForLanguage(language);
			if (sprite == null)
				return;

			if (_image == null && _spriteRenderer == null)
				CacheTargetComponents();

			if (_image != null)
				_image.sprite = sprite;
			else if (_spriteRenderer != null)
				_spriteRenderer.sprite = sprite;
		}

		private void CacheTargetComponents()
		{
			_image = GetComponent<Image>();
			_spriteRenderer = GetComponent<SpriteRenderer>();
			if (_image == null && _spriteRenderer == null)
				Debug.LogWarning("[LocalizationToolkit] LocalizedImage requires an Image or SpriteRenderer component on the same GameObject.", this);
		}
	}
}

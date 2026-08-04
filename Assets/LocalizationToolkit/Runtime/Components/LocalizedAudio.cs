using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>Audio clip override played for a single language.</summary>
	[Serializable]
	public class LocalizedAudioEntry
	{
		[Tooltip("Language key the clip applies to, e.g. 'Polish'.")]
		public string Language;

		[Tooltip("Clip used while that language is active.")]
		public AudioClip Clip;
	}

	/// <summary>
	/// Swaps the clip of an <see cref="AudioSource"/> to match the active language —
	/// for localized voice-overs, narrations or spoken tutorials.
	/// </summary>
	/// <remarks>
	/// Assign a default clip and add an override per language that has its own
	/// recording. Languages without an override fall back to the default clip. When the
	/// language changes while the source is playing, the new clip starts playing from
	/// the beginning.
	/// </remarks>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(AudioSource))]
	[AddComponentMenu("Localization Toolkit/Localized Audio")]
	[HelpURL(LocalizationToolkitInfo.DocumentationUrl)]
	public class LocalizedAudio : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("Clip used when no override matches the active language.")]
		private AudioClip _defaultClip;

		[SerializeField]
		[Tooltip("Per-language clip overrides.")]
		private List<LocalizedAudioEntry> _overrides = new List<LocalizedAudioEntry>();

		private AudioSource _source;

		/// <summary>Clip used when no override matches the active language. Setting it refreshes the source.</summary>
		public AudioClip DefaultClip
		{
			get => _defaultClip;
			set
			{
				_defaultClip = value;
				Refresh();
			}
		}

		/// <summary>Per-language clip overrides. Call <see cref="Refresh"/> after modifying the list.</summary>
		public List<LocalizedAudioEntry> Overrides => _overrides;

		private void OnEnable()
		{
			LocalizationManager.LanguageChanged += Refresh;
			Refresh();
		}

		private void OnDisable()
		{
			LocalizationManager.LanguageChanged -= Refresh;
		}

		/// <summary>Resolves the clip that should play for the given language.</summary>
		/// <param name="languageKey">Language key to resolve, or null for the default clip.</param>
		/// <returns>The matching override clip, or the default clip when no override applies.</returns>
		public AudioClip GetClipForLanguage(string languageKey)
		{
			if (!string.IsNullOrEmpty(languageKey))
			{
				foreach (LocalizedAudioEntry entry in _overrides)
				{
					if (entry != null && entry.Clip != null && entry.Language == languageKey)
						return entry.Clip;
				}
			}

			return _defaultClip;
		}

		/// <summary>Re-applies the clip of the active language to the attached audio source.</summary>
		public void Refresh()
		{
			LocalizationManager manager = LocalizationManager.Instance;
			string language = manager != null && manager.IsLoaded ? manager.ActiveLanguage : null;

			AudioClip clip = GetClipForLanguage(language);
			if (clip == null)
				return;

			if (_source == null)
				_source = GetComponent<AudioSource>();

			if (_source == null || _source.clip == clip)
				return;

			bool wasPlaying = Application.isPlaying && _source.isPlaying;
			_source.clip = clip;
			if (wasPlaying)
				_source.Play();
		}
	}
}

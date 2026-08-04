using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>
	/// Central access point for localized content. Loads localization data from
	/// StreamingAssets or a remote URL and exposes translations for the active language.
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[AddComponentMenu("Localization Toolkit/Localization Manager")]
	public class LocalizationManager : MonoBehaviourSingleton<LocalizationManager>
	{
		/// <summary>Language key used as a fallback when a requested language is unavailable.</summary>
		public const string DefaultLanguageKey = "default";

		/// <summary>Raised whenever a different language is loaded.</summary>
		public static event Action LanguageChanged;

		[SerializeField, FormerlySerializedAs("fileURL")]
		[Tooltip("Optional URL of a remote localization file. Takes precedence over the local file.")]
		private string _remoteUrl = string.Empty;

		[SerializeField, FormerlySerializedAs("fileName")]
		[Tooltip("Name (without extension) of a localization file inside StreamingAssets.")]
		private string _localFileName = string.Empty;

		[SerializeField, FormerlySerializedAs("extension")]
		[Tooltip("Format of the local localization file.")]
		private LocalizationFileFormat _fileFormat = LocalizationFileFormat.Json;

		[SerializeField]
		[Tooltip("When enabled, the system language is selected automatically after the data is loaded.")]
		private bool _detectSystemLanguage = true;

		[SerializeField]
		[Tooltip("Text returned when a translation key cannot be found.")]
		private string _missingTranslationText = "Localized text not found";

		private LocalizationData _data;
		private Dictionary<string, string> _activeTranslations;
		private string _activeLanguage = string.Empty;

		/// <summary>URL of the remote localization file.</summary>
		public string RemoteUrl
		{
			get => _remoteUrl;
			set => _remoteUrl = value;
		}

		/// <summary>Name (without extension) of the local localization file inside StreamingAssets.</summary>
		public string LocalFileName
		{
			get => _localFileName;
			set => _localFileName = value;
		}

		/// <summary>Format of the local localization file.</summary>
		public LocalizationFileFormat FileFormat
		{
			get => _fileFormat;
			set => _fileFormat = value;
		}

		/// <summary>Key of the currently active language, or an empty string when nothing is loaded.</summary>
		public string ActiveLanguage => _activeLanguage;

		/// <summary>True when localization data has been loaded.</summary>
		public bool IsLoaded => _data?.Languages != null && _data.Languages.Count > 0;

		protected override void Awake()
		{
			base.Awake();
			AutoLoad();
		}

		/// <summary>Absolute path of the configured local localization file.</summary>
		public string GetLocalFilePath()
		{
			return Path.Combine(Application.streamingAssetsPath, _localFileName + "." + _fileFormat.ToExtension());
		}

		/// <summary>Loads localization data from a file on disk.</summary>
		/// <param name="filePath">Absolute path of the file.</param>
		/// <param name="format">Format of the file contents.</param>
		public void LoadFromFile(string filePath, LocalizationFileFormat format)
		{
			try
			{
				ApplyData(LocalizationData.Parse(File.ReadAllText(filePath), format));
			}
			catch (Exception exception)
			{
				Debug.LogError($"[LocalizationToolkit] Failed to load localization file '{filePath}': {exception.Message}", this);
			}
		}

		/// <summary>Downloads and loads localization data from a remote URL.</summary>
		/// <param name="url">URL pointing to a .json, .xml or .csv file.</param>
		public void LoadFromWeb(string url)
		{
			if (!RemoteFileLoader.TryGetFileFormatFromUrl(url, out LocalizationFileFormat format))
			{
				Debug.LogError("[LocalizationToolkit] The URL must point to a .json, .xml or .csv file.", this);
				return;
			}

			string rawData = RemoteFileLoader.DownloadText(url);
			if (string.IsNullOrEmpty(rawData))
				return;

			try
			{
				ApplyData(LocalizationData.Parse(rawData, format));
			}
			catch (Exception exception)
			{
				Debug.LogError($"[LocalizationToolkit] Failed to parse localization data from '{url}': {exception.Message}", this);
			}
		}

		/// <summary>Activates the given language, falling back to the default language when missing.</summary>
		/// <param name="languageKey">Key of the language to activate.</param>
		public void LoadLanguage(string languageKey)
		{
			if (!IsLoaded)
			{
				Debug.LogWarning("[LocalizationToolkit] No localization data has been loaded yet.", this);
				return;
			}

			if (languageKey == null || !_data.Languages.TryGetValue(languageKey, out Dictionary<string, string> translations))
			{
				if (!_data.Languages.TryGetValue(DefaultLanguageKey, out translations))
				{
					Debug.LogError($"[LocalizationToolkit] Language '{languageKey}' was not found and the data contains no '{DefaultLanguageKey}' language.", this);
					return;
				}

				languageKey = DefaultLanguageKey;
			}

			_activeLanguage = languageKey;
			_activeTranslations = translations;
			LanguageChanged?.Invoke();
		}

		/// <summary>Returns the translation for the given key in the active language.</summary>
		public string GetLocalizedValue(string key)
		{
			if (TryGetLocalizedValue(key, out string value))
				return value;

			Debug.LogWarning($"[LocalizationToolkit] Missing translation key '{key}' for language '{_activeLanguage}'.", this);
			return _missingTranslationText;
		}

		/// <summary>Returns the translation for the given key with {token} placeholders replaced.</summary>
		public string GetLocalizedValue(string key, params (string token, string value)[] tokens)
		{
			return LocalizationTextFormatter.ApplyTokens(GetLocalizedValue(key), tokens);
		}

		/// <summary>Tries to fetch the translation for the given key in the active language.</summary>
		public bool TryGetLocalizedValue(string key, out string value)
		{
			value = null;
			return key != null && _activeTranslations != null && _activeTranslations.TryGetValue(key, out value);
		}

		/// <summary>All language keys present in the loaded data.</summary>
		public string[] GetAvailableLanguages()
		{
			return IsLoaded ? _data.Languages.Keys.ToArray() : Array.Empty<string>();
		}

		/// <summary>All translation keys, taken from the default language when available.</summary>
		public string[] GetKeys()
		{
			if (!IsLoaded)
				return Array.Empty<string>();

			if (!_data.Languages.TryGetValue(DefaultLanguageKey, out Dictionary<string, string> table))
				table = _data.Languages.Values.First();

			return table.Keys.ToArray();
		}

		private void AutoLoad()
		{
			if (!string.IsNullOrWhiteSpace(_remoteUrl))
				LoadFromWeb(_remoteUrl);
			else if (!string.IsNullOrWhiteSpace(_localFileName))
				LoadFromFile(GetLocalFilePath(), _fileFormat);
		}

		private void ApplyData(LocalizationData data)
		{
			_data = data;
			LoadLanguage(_detectSystemLanguage ? Application.systemLanguage.ToString() : DefaultLanguageKey);
		}
	}
}

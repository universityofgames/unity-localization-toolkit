using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>
	/// Central access point for localized content. Loads localization data from a
	/// <see cref="TextAsset"/>, a remote URL or StreamingAssets and exposes the
	/// translations of the currently active language.
	/// </summary>
	/// <remarks>
	/// Add a single instance to your scene. On <c>Awake</c> the manager automatically
	/// loads the first configured source, in this order: file asset, remote URL,
	/// StreamingAssets file. When <c>Detect System Language</c> is enabled, the
	/// player's <see cref="Application.systemLanguage"/> is selected automatically,
	/// with <see cref="DefaultLanguageKey"/> as the fallback.
	/// </remarks>
	/// <example>
	/// <code>
	/// string title = LocalizationManager.Instance.GetLocalizedValue("title");
	/// string welcome = LocalizationManager.Instance.GetLocalizedValue("welcome_player", ("name", playerName));
	/// LocalizationManager.Instance.LoadLanguage("Polish");
	/// LocalizationManager.LanguageChanged += OnLanguageChanged;
	/// </code>
	/// </example>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[AddComponentMenu("Localization Toolkit/Localization Manager")]
	[HelpURL(LocalizationToolkitInfo.DocumentationUrl)]
	public class LocalizationManager : MonoBehaviourSingleton<LocalizationManager>
	{
		/// <summary>Language key used as a fallback when a requested language is unavailable.</summary>
		public const string DefaultLanguageKey = "default";

		/// <summary>
		/// Raised whenever a different language is activated. <see cref="LocalizedText"/>
		/// components subscribe to this event to refresh themselves automatically.
		/// </summary>
		public static event Action LanguageChanged;

		[SerializeField]
		[Tooltip("Localization file asset (JSON, XML or CSV). Takes precedence over the remote URL and the StreamingAssets file.")]
		private TextAsset _localizationFile;

		[SerializeField, FormerlySerializedAs("fileURL")]
		[Tooltip("Optional URL of a remote localization file (.json, .xml or .csv).")]
		private string _remoteUrl = string.Empty;

		[SerializeField, FormerlySerializedAs("fileName")]
		[Tooltip("Name (without extension) of a localization file inside StreamingAssets.")]
		private string _localFileName = string.Empty;

		[SerializeField, FormerlySerializedAs("extension")]
		[Tooltip("Format of the StreamingAssets localization file.")]
		private LocalizationFileFormat _fileFormat = LocalizationFileFormat.Json;

		[SerializeField]
		[Tooltip("Select the player's system language automatically after the data is loaded.")]
		private bool _detectSystemLanguage = true;

		[SerializeField]
		[Tooltip("Text returned when a translation key cannot be found.")]
		private string _missingTranslationText = "Localized text not found";

		private LocalizationData _data;
		private Dictionary<string, string> _activeTranslations;
		private string _activeLanguage = string.Empty;

		/// <summary>Localization file asset (JSON, XML or CSV); the format is detected automatically.</summary>
		public TextAsset LocalizationFile
		{
			get => _localizationFile;
			set => _localizationFile = value;
		}

		/// <summary>URL of the remote localization file (.json, .xml or .csv).</summary>
		public string RemoteUrl
		{
			get => _remoteUrl;
			set => _remoteUrl = value;
		}

		/// <summary>Name (without extension) of the localization file inside StreamingAssets.</summary>
		public string LocalFileName
		{
			get => _localFileName;
			set => _localFileName = value;
		}

		/// <summary>Format of the StreamingAssets localization file.</summary>
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

		/// <summary>Builds the absolute path of the configured StreamingAssets localization file.</summary>
		/// <returns>Absolute file path combining the StreamingAssets folder, file name and format extension.</returns>
		public string GetLocalFilePath()
		{
			return Path.Combine(Application.streamingAssetsPath, _localFileName + "." + _fileFormat.ToExtension());
		}

		/// <summary>Loads localization data from a text asset; the format is detected automatically.</summary>
		/// <param name="asset">Text asset containing JSON, XML or CSV localization data.</param>
		public void LoadFromTextAsset(TextAsset asset)
		{
			if (asset == null)
			{
				Debug.LogError("[LocalizationToolkit] No localization file asset has been assigned.", this);
				return;
			}

			try
			{
				ApplyData(LocalizationData.Parse(asset.text, LocalizationFileFormatUtility.DetectFormat(asset.text)));
			}
			catch (Exception exception)
			{
				Debug.LogError($"[LocalizationToolkit] Failed to parse localization asset '{asset.name}': {exception.Message}", this);
			}
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
		/// <remarks>The download blocks until it completes; see <see cref="RemoteFileLoader.DownloadText"/>.</remarks>
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

		/// <summary>
		/// Activates the given language and raises <see cref="LanguageChanged"/>.
		/// Falls back to <see cref="DefaultLanguageKey"/> when the language is missing.
		/// </summary>
		/// <param name="languageKey">Key of the language to activate, e.g. <c>"Polish"</c>.</param>
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
		/// <param name="key">Translation key to look up.</param>
		/// <returns>The translated value, or the configured missing-translation text when the key is unknown.</returns>
		public string GetLocalizedValue(string key)
		{
			if (TryGetLocalizedValue(key, out string value))
				return value;

			Debug.LogWarning($"[LocalizationToolkit] Missing translation key '{key}' for language '{_activeLanguage}'.", this);
			return _missingTranslationText;
		}

		/// <summary>Returns the translation for the given key with <c>{token}</c> placeholders replaced.</summary>
		/// <param name="key">Translation key to look up.</param>
		/// <param name="tokens">Pairs of token names (without braces) and replacement values.</param>
		/// <returns>The translated value with every placeholder substituted.</returns>
		/// <example>
		/// <code>
		/// // "welcome_player" = "Welcome, {name}! Level {level}."
		/// string text = manager.GetLocalizedValue("welcome_player", ("name", "Anna"), ("level", "3"));
		/// </code>
		/// </example>
		public string GetLocalizedValue(string key, params (string token, string value)[] tokens)
		{
			return LocalizationTextFormatter.ApplyTokens(GetLocalizedValue(key), tokens);
		}

		/// <summary>Tries to fetch the translation for the given key in the active language.</summary>
		/// <param name="key">Translation key to look up.</param>
		/// <param name="value">The translated value when the method returns true; otherwise null.</param>
		/// <returns>True when the key exists in the active language.</returns>
		public bool TryGetLocalizedValue(string key, out string value)
		{
			value = null;
			return key != null && _activeTranslations != null && _activeTranslations.TryGetValue(key, out value);
		}

		/// <summary>Lists every language key present in the loaded data.</summary>
		/// <returns>Language keys, or an empty array when no data is loaded.</returns>
		public string[] GetAvailableLanguages()
		{
			return IsLoaded ? _data.Languages.Keys.ToArray() : Array.Empty<string>();
		}

		/// <summary>Lists every translation key, taken from the default language when available.</summary>
		/// <returns>Translation keys, or an empty array when no data is loaded.</returns>
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
			if (_localizationFile != null)
				LoadFromTextAsset(_localizationFile);
			else if (!string.IsNullOrWhiteSpace(_remoteUrl))
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

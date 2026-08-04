using System.Collections.Generic;
using System.Globalization;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>
	/// Maps language keys (matching <see cref="UnityEngine.SystemLanguage"/> names)
	/// to <see cref="CultureInfo"/> instances used for number, date and currency
	/// formatting in <c>{token:format}</c> placeholders.
	/// </summary>
	public static class LocalizationCultures
	{
		private static readonly Dictionary<string, string> CultureNames = new Dictionary<string, string>
		{
			["English"] = "en-US",
			["Polish"] = "pl-PL",
			["German"] = "de-DE",
			["Spanish"] = "es-ES",
			["French"] = "fr-FR",
			["Italian"] = "it-IT",
			["Portuguese"] = "pt-PT",
			["Russian"] = "ru-RU",
			["Ukrainian"] = "uk-UA",
			["Czech"] = "cs-CZ",
			["Slovak"] = "sk-SK",
			["Romanian"] = "ro-RO",
			["Hungarian"] = "hu-HU",
			["Dutch"] = "nl-NL",
			["Swedish"] = "sv-SE",
			["Norwegian"] = "nb-NO",
			["Danish"] = "da-DK",
			["Finnish"] = "fi-FI",
			["Greek"] = "el-GR",
			["Turkish"] = "tr-TR",
			["Arabic"] = "ar-SA",
			["Hebrew"] = "he-IL",
			["Japanese"] = "ja-JP",
			["Korean"] = "ko-KR",
			["Chinese"] = "zh-CN",
			["ChineseSimplified"] = "zh-CN",
			["ChineseTraditional"] = "zh-TW",
			["Thai"] = "th-TH",
			["Vietnamese"] = "vi-VN",
			["Indonesian"] = "id-ID",
			["Bulgarian"] = "bg-BG",
			["Belarusian"] = "be-BY"
		};

		private static readonly Dictionary<string, CultureInfo> Cache = new Dictionary<string, CultureInfo>();

		/// <summary>Returns the culture matching a language key.</summary>
		/// <param name="languageKey">Language key, e.g. <c>"Polish"</c>.</param>
		/// <returns>The mapped culture, or <see cref="CultureInfo.InvariantCulture"/> for unknown languages.</returns>
		public static CultureInfo GetCulture(string languageKey)
		{
			if (string.IsNullOrEmpty(languageKey))
				return CultureInfo.InvariantCulture;

			if (Cache.TryGetValue(languageKey, out CultureInfo cached))
				return cached;

			CultureInfo culture = CultureInfo.InvariantCulture;
			if (CultureNames.TryGetValue(languageKey, out string cultureName))
			{
				try
				{
					culture = CultureInfo.GetCultureInfo(cultureName);
				}
				catch (CultureNotFoundException)
				{
					// Fall back to the invariant culture on trimmed ICU data sets.
				}
			}

			Cache[languageKey] = culture;
			return culture;
		}
	}
}

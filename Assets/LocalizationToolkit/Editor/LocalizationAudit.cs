using System.Collections.Generic;
using System.Linq;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>Pure helpers computing completeness statistics and key-usage audits.</summary>
	public static class LocalizationAudit
	{
		/// <summary>Completion statistics of a single language.</summary>
		public readonly struct LanguageStatistics
		{
			/// <summary>Language key the statistics belong to.</summary>
			public readonly string Language;

			/// <summary>Number of keys with a non-empty translation.</summary>
			public readonly int Filled;

			/// <summary>Total number of keys, taken from the key source language.</summary>
			public readonly int Total;

			public LanguageStatistics(string language, int filled, int total)
			{
				Language = language;
				Filled = filled;
				Total = total;
			}

			/// <summary>Completion ratio in the 0-1 range.</summary>
			public float Completion => Total == 0 ? 1f : (float)Filled / Total;
		}

		/// <summary>Computes per-language completion statistics.</summary>
		/// <param name="data">Localization data to analyze.</param>
		/// <param name="keySourceLanguage">Language whose keys define the full key set.</param>
		/// <returns>One entry per language, in data order.</returns>
		public static List<LanguageStatistics> GetStatistics(LocalizationData data, string keySourceLanguage)
		{
			ICollection<string> keys = data.Languages[keySourceLanguage].Keys;
			var statistics = new List<LanguageStatistics>();

			foreach (KeyValuePair<string, Dictionary<string, string>> language in data.Languages)
			{
				int filled = keys.Count(key =>
					language.Value.TryGetValue(key, out string value) && !string.IsNullOrWhiteSpace(value));
				statistics.Add(new LanguageStatistics(language.Key, filled, keys.Count));
			}

			return statistics;
		}

		/// <summary>Lists keys used in content but missing from the localization data.</summary>
		/// <param name="contentKeys">Keys referenced by components in scenes and prefabs.</param>
		/// <param name="data">Localization data to compare against.</param>
		/// <param name="keySourceLanguage">Language whose keys define the full key set.</param>
		/// <returns>Sorted, distinct missing keys.</returns>
		public static List<string> GetKeysMissingFromData(IEnumerable<string> contentKeys,
			LocalizationData data, string keySourceLanguage)
		{
			Dictionary<string, string> keySource = data.Languages[keySourceLanguage];
			return contentKeys
				.Where(key => !string.IsNullOrEmpty(key) && !keySource.ContainsKey(key))
				.Distinct()
				.OrderBy(key => key)
				.ToList();
		}

		/// <summary>Lists keys present in the localization data but never used in content.</summary>
		/// <param name="contentKeys">Keys referenced by components in scenes and prefabs.</param>
		/// <param name="data">Localization data to compare against.</param>
		/// <param name="keySourceLanguage">Language whose keys define the full key set.</param>
		/// <returns>Sorted unused keys.</returns>
		public static List<string> GetUnusedKeys(IEnumerable<string> contentKeys,
			LocalizationData data, string keySourceLanguage)
		{
			var used = new HashSet<string>(contentKeys);
			return data.Languages[keySourceLanguage].Keys
				.Where(key => !used.Contains(key))
				.OrderBy(key => key)
				.ToList();
		}
	}
}

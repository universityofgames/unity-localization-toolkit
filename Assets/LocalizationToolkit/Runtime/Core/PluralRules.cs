using System;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>CLDR plural categories.</summary>
	public enum PluralCategory
	{
		Zero,
		One,
		Two,
		Few,
		Many,
		Other
	}

	/// <summary>
	/// Resolves CLDR plural categories for integer counts. Covers the rule families of
	/// the commonly shipped languages; unknown languages use the English-like rule
	/// (1 → One, everything else → Other).
	/// </summary>
	public static class PluralRules
	{
		/// <summary>Resolves the plural category of a count in the given language.</summary>
		/// <param name="languageKey">Language key, e.g. <c>"Polish"</c> (matches <see cref="UnityEngine.SystemLanguage"/> names).</param>
		/// <param name="count">The item count; the sign is ignored.</param>
		/// <returns>The CLDR plural category the count belongs to.</returns>
		public static PluralCategory Resolve(string languageKey, int count)
		{
			int n = Math.Abs(count);
			int mod10 = n % 10;
			int mod100 = n % 100;

			switch (languageKey)
			{
				case "Polish":
					if (n == 1)
						return PluralCategory.One;
					if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14))
						return PluralCategory.Few;
					return PluralCategory.Many;

				case "Russian":
				case "Ukrainian":
				case "Belarusian":
				case "SerboCroatian":
					if (mod10 == 1 && mod100 != 11)
						return PluralCategory.One;
					if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14))
						return PluralCategory.Few;
					return PluralCategory.Many;

				case "Czech":
				case "Slovak":
					if (n == 1)
						return PluralCategory.One;
					if (n >= 2 && n <= 4)
						return PluralCategory.Few;
					return PluralCategory.Other;

				case "French":
					return n == 0 || n == 1 ? PluralCategory.One : PluralCategory.Other;

				case "Romanian":
					if (n == 1)
						return PluralCategory.One;
					if (n == 0 || (mod100 >= 2 && mod100 <= 19))
						return PluralCategory.Few;
					return PluralCategory.Other;

				case "Arabic":
					if (n == 0)
						return PluralCategory.Zero;
					if (n == 1)
						return PluralCategory.One;
					if (n == 2)
						return PluralCategory.Two;
					if (mod100 >= 3 && mod100 <= 10)
						return PluralCategory.Few;
					if (mod100 >= 11 && mod100 <= 99)
						return PluralCategory.Many;
					return PluralCategory.Other;

				case "Japanese":
				case "Korean":
				case "Chinese":
				case "ChineseSimplified":
				case "ChineseTraditional":
				case "Thai":
				case "Vietnamese":
				case "Indonesian":
					return PluralCategory.Other;

				default:
					return n == 1 ? PluralCategory.One : PluralCategory.Other;
			}
		}
	}
}

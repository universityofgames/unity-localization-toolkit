using System.Text;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>
	/// Generates a pseudo-localized language for UI testing: accented characters reveal
	/// missing glyphs, ~30% padding reveals layouts that break on longer translations,
	/// and brackets reveal truncated or concatenated strings.
	/// </summary>
	public static class PseudoLocalizer
	{
		/// <summary>Key of the generated pseudo language.</summary>
		public const string LanguageKey = "Pseudo";

		private const float PaddingRatio = 0.3f;
		private const string SourceCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private const string PseudoCharacters = "áƀćđéƒğĥíĵķĺɱńóṕǫŕşŧúṽŵẋýžÁƁĆĐÉƑĞĤÍĴĶĹṀŃÓṔǪŔŞŦÚṼŴẊÝŽ";

		/// <summary>Converts a source text into its pseudo-localized form.</summary>
		/// <param name="text">Source text; <c>{token}</c> placeholders are preserved verbatim.</param>
		/// <returns>The pseudo-localized text, wrapped in ⟦ ⟧ and padded by ~30%.</returns>
		public static string Generate(string text)
		{
			if (string.IsNullOrEmpty(text))
				return text;

			var builder = new StringBuilder(text.Length * 2);
			builder.Append('⟦');

			int index = 0;
			while (index < text.Length)
			{
				char character = text[index];

				if (character == '{')
				{
					int close = text.IndexOf('}', index);
					if (close >= 0)
					{
						builder.Append(text, index, close - index + 1);
						index = close + 1;
						continue;
					}
				}

				int mapIndex = SourceCharacters.IndexOf(character);
				builder.Append(mapIndex >= 0 ? PseudoCharacters[mapIndex] : character);
				index++;
			}

			builder.Append(' ');
			builder.Append('~', Mathf.CeilToInt(text.Length * PaddingRatio));
			builder.Append('⟧');
			return builder.ToString();
		}
	}
}

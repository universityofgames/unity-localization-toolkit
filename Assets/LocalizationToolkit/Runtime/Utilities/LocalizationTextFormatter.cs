using System.Text;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>Replaces {token} placeholders inside translated strings.</summary>
	public static class LocalizationTextFormatter
	{
		/// <summary>Replaces every {token} placeholder in the text with its provided value.</summary>
		/// <param name="text">Text containing zero or more {token} placeholders.</param>
		/// <param name="tokens">Pairs of token names (without braces) and replacement values.</param>
		public static string ApplyTokens(string text, params (string token, string value)[] tokens)
		{
			if (string.IsNullOrEmpty(text) || tokens == null || tokens.Length == 0)
				return text;

			var builder = new StringBuilder(text);
			foreach ((string token, string value) in tokens)
			{
				if (!string.IsNullOrEmpty(token))
					builder.Replace("{" + token + "}", value ?? string.Empty);
			}

			return builder.ToString();
		}
	}
}

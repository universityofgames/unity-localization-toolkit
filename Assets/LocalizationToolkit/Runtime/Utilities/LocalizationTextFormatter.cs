using System;
using System.Collections.Generic;
using System.Text;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>Replaces {token} placeholders inside translated strings.</summary>
	public static class LocalizationTextFormatter
	{
		/// <summary>Replaces every <c>{token}</c> placeholder in the text with its provided value.</summary>
		/// <param name="text">Text containing zero or more <c>{token}</c> placeholders.</param>
		/// <param name="tokens">Pairs of token names (without braces) and replacement values.</param>
		/// <returns>The text with every matching placeholder substituted.</returns>
		/// <example>
		/// <code>
		/// LocalizationTextFormatter.ApplyTokens("Hello {name}!", ("name", "Anna")); // "Hello Anna!"
		/// </code>
		/// </example>
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

		/// <summary>
		/// Replaces <c>{token}</c> and <c>{token:format}</c> placeholders, formatting the
		/// values with the given culture — e.g. <c>{price:C}</c> or <c>{date:d}</c>.
		/// </summary>
		/// <param name="text">Text containing zero or more placeholders.</param>
		/// <param name="formatProvider">Culture used to format <see cref="IFormattable"/> values.</param>
		/// <param name="tokens">Pairs of token names (without braces) and replacement values.</param>
		/// <returns>The text with every matching placeholder substituted; unknown placeholders stay untouched.</returns>
		/// <example>
		/// <code>
		/// var culture = LocalizationCultures.GetCulture("German");
		/// LocalizationTextFormatter.ApplyTokens("Price: {price:C}", culture, ("price", 9.99m));
		/// </code>
		/// </example>
		public static string ApplyTokens(string text, IFormatProvider formatProvider,
			params (string token, object value)[] tokens)
		{
			if (string.IsNullOrEmpty(text) || tokens == null || tokens.Length == 0)
				return text;

			var values = new Dictionary<string, object>(tokens.Length);
			foreach ((string token, object value) in tokens)
			{
				if (!string.IsNullOrEmpty(token))
					values[token] = value;
			}

			var builder = new StringBuilder(text.Length + 16);
			int index = 0;
			while (index < text.Length)
			{
				char character = text[index];
				if (character == '{')
				{
					int close = text.IndexOf('}', index);
					if (close > index)
					{
						string placeholder = text.Substring(index + 1, close - index - 1);
						string name = placeholder;
						string format = null;

						int colon = placeholder.IndexOf(':');
						if (colon >= 0)
						{
							name = placeholder.Substring(0, colon);
							format = placeholder.Substring(colon + 1);
						}

						if (values.TryGetValue(name, out object value))
						{
							builder.Append(FormatValue(value, format, formatProvider));
							index = close + 1;
							continue;
						}
					}
				}

				builder.Append(character);
				index++;
			}

			return builder.ToString();
		}

		private static string FormatValue(object value, string format, IFormatProvider formatProvider)
		{
			if (value == null)
				return string.Empty;

			if (value is IFormattable formattable)
				return formattable.ToString(string.IsNullOrEmpty(format) ? null : format, formatProvider);

			return value.ToString();
		}
	}
}

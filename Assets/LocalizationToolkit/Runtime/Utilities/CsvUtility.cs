using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>Minimal RFC 4180 compliant CSV reader and writer.</summary>
	internal static class CsvUtility
	{
		private static readonly char[] CharactersRequiringQuotes = { ',', '"', '\n', '\r' };

		/// <summary>Parses CSV text into rows of fields, honoring quoted fields.</summary>
		internal static List<List<string>> Parse(string text)
		{
			var rows = new List<List<string>>();
			if (string.IsNullOrEmpty(text))
				return rows;

			var row = new List<string>();
			var field = new StringBuilder();
			bool inQuotes = false;

			int index = 0;
			while (index < text.Length)
			{
				char character = text[index];

				if (inQuotes)
				{
					if (character == '"')
					{
						if (index + 1 < text.Length && text[index + 1] == '"')
						{
							field.Append('"');
							index += 2;
							continue;
						}

						inQuotes = false;
						index++;
						continue;
					}

					field.Append(character);
					index++;
					continue;
				}

				switch (character)
				{
					case '"':
						inQuotes = true;
						index++;
						break;
					case ',':
						row.Add(field.ToString());
						field.Length = 0;
						index++;
						break;
					case '\r':
					case '\n':
						if (character == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
							index++;

						row.Add(field.ToString());
						field.Length = 0;
						rows.Add(row);
						row = new List<string>();
						index++;
						break;
					default:
						field.Append(character);
						index++;
						break;
				}
			}

			if (field.Length > 0 || row.Count > 0)
			{
				row.Add(field.ToString());
				rows.Add(row);
			}

			return rows;
		}

		/// <summary>Joins fields into a single CSV row, quoting fields when required.</summary>
		internal static string WriteRow(IEnumerable<string> fields)
		{
			return string.Join(",", fields.Select(EscapeField));
		}

		private static string EscapeField(string value)
		{
			value = value ?? string.Empty;
			return value.IndexOfAny(CharactersRequiringQuotes) >= 0
				? "\"" + value.Replace("\"", "\"\"") + "\""
				: value;
		}
	}
}

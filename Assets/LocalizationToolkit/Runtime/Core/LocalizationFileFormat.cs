using System;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>Supported localization file formats.</summary>
	public enum LocalizationFileFormat
	{
		Json = 0,
		Xml = 1,
		Csv = 2
	}

	/// <summary>Helpers for mapping between localization file formats and file extensions.</summary>
	public static class LocalizationFileFormatUtility
	{
		/// <summary>Returns the lower-case file extension (without the leading dot) used by the given format.</summary>
		public static string ToExtension(this LocalizationFileFormat format)
		{
			switch (format)
			{
				case LocalizationFileFormat.Json: return "json";
				case LocalizationFileFormat.Xml: return "xml";
				case LocalizationFileFormat.Csv: return "csv";
				default: throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		/// <summary>Detects the format of raw localization text by inspecting its first significant character.</summary>
		/// <param name="content">Raw localization file contents.</param>
		public static LocalizationFileFormat DetectFormat(string content)
		{
			if (!string.IsNullOrEmpty(content))
			{
				foreach (char character in content)
				{
					if (char.IsWhiteSpace(character) || character == '\uFEFF')
						continue;

					if (character == '{' || character == '[')
						return LocalizationFileFormat.Json;

					return character == '<' ? LocalizationFileFormat.Xml : LocalizationFileFormat.Csv;
				}
			}

			return LocalizationFileFormat.Json;
		}

		/// <summary>Tries to resolve a localization file format from a file extension.</summary>
		/// <param name="extension">File extension, with or without the leading dot.</param>
		/// <param name="format">Resolved format when the method returns true.</param>
		public static bool TryParseExtension(string extension, out LocalizationFileFormat format)
		{
			switch (extension?.Trim().TrimStart('.').ToLowerInvariant())
			{
				case "json": format = LocalizationFileFormat.Json; return true;
				case "xml": format = LocalizationFileFormat.Xml; return true;
				case "csv": format = LocalizationFileFormat.Csv; return true;
				default: format = default; return false;
			}
		}
	}
}

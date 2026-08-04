using UnityEngine;
using UnityEngine.Networking;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>Downloads localization files over HTTP(S).</summary>
	public static class RemoteFileLoader
	{
		/// <summary>Request timeout in seconds.</summary>
		public const int TimeoutSeconds = 10;

		/// <summary>
		/// Downloads the given URL and returns its body as text, or an empty string on failure.
		/// The call blocks until the request completes; localization files are expected to be
		/// small and loaded once. Not supported on WebGL.
		/// </summary>
		public static string DownloadText(string url)
		{
			using (UnityWebRequest request = UnityWebRequest.Get(url))
			{
				request.timeout = TimeoutSeconds;
				UnityWebRequestAsyncOperation operation = request.SendWebRequest();
				while (!operation.isDone)
				{
				}

				if (request.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError($"[LocalizationToolkit] Failed to download '{url}': {request.error}");
					return string.Empty;
				}

				return request.downloadHandler.text.Trim();
			}
		}

		/// <summary>Tries to resolve the localization file format from the URL's file extension.</summary>
		public static bool TryGetFileFormatFromUrl(string url, out LocalizationFileFormat format)
		{
			format = default;
			if (string.IsNullOrWhiteSpace(url))
				return false;

			int queryIndex = url.IndexOfAny(new[] { '?', '#' });
			if (queryIndex >= 0)
				url = url.Substring(0, queryIndex);

			int dotIndex = url.LastIndexOf('.');
			return dotIndex >= 0 && LocalizationFileFormatUtility.TryParseExtension(url.Substring(dotIndex + 1), out format);
		}
	}
}

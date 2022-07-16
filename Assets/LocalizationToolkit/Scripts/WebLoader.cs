using System;
using System.Net;
using UnityEngine;

public static class WebLoader {
	/// <summary>This method fetch file from web</summary>
	/// <param name="url">URL to file</param>
	public static string LoadStringFileFromWeb(string url) {
		string data = "";
		try
		{
			using (var wc = new WebClient())
			{
				data = wc.DownloadString(url).Trim();
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Cannot load file from web: " + e.Message);
		}
		
		return data;
	}

	/// <summary>This method fetch extension from url file</summary>
	/// <param name="fileUrl">URL to file</param>
	public static string GetExtensionFromUrl(string url) {
		string[] separatedURL = url.Split('.');
		if (separatedURL.Length > 0)
		{
			string lastSegment = separatedURL[separatedURL.Length - 1].ToLower();
			if (lastSegment == AvailableExtensions.json.ToString().ToLower() || lastSegment == AvailableExtensions.xml.ToString().ToLower())
				return lastSegment;
		}
		return "";
	}
}
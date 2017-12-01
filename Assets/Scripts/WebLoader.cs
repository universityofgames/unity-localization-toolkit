using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

public static class WebLoader {

	public static string LoadStringFileFromWeb(string fileURL) {
		string data = "";
		using (var wc = new WebClient())
		{
			data = wc.DownloadString(fileURL).Trim();
		}
		return data;
	}
}
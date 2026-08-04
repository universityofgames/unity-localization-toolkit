using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>
	/// Translates localization entries with an AI model (Anthropic Claude or OpenAI GPT).
	/// Editor-only: the API key is provided by the user at edit time and is never
	/// serialized into assets or builds.
	/// </summary>
	public static class AiTranslator
	{
		/// <summary>Request timeout in seconds.</summary>
		public const int TimeoutSeconds = 180;

		private const int MaxOutputTokens = 16000;

		/// <summary>Builds the instruction prompt for a translation batch.</summary>
		public static string BuildPrompt(string sourceLanguage, string targetLanguage, IDictionary<string, string> entries)
		{
			var builder = new StringBuilder();
			builder.AppendLine("You are a professional game localization translator.");
			builder.AppendLine($"Translate the following user interface strings from \"{sourceLanguage}\" to \"{targetLanguage}\".");
			builder.AppendLine();
			builder.AppendLine("Rules:");
			builder.AppendLine("- Return ONLY a valid JSON object that maps every key to its translated value. No markdown, no commentary.");
			builder.AppendLine("- Keep every key exactly as provided and translate only the values.");
			builder.AppendLine("- Preserve placeholders wrapped in curly braces, such as {name} or {score}, exactly as they appear.");
			builder.AppendLine("- Keep the tone natural for game UI text in the target language.");
			builder.AppendLine();
			builder.AppendLine("Strings to translate:");
			builder.AppendLine(JsonConvert.SerializeObject(entries, Formatting.Indented));
			return builder.ToString();
		}

		/// <summary>Builds the JSON request body for the given provider.</summary>
		public static string BuildRequestBody(AiTranslationProvider provider, string model, string prompt)
		{
			var messages = new JArray
			{
				new JObject
				{
					["role"] = "user",
					["content"] = prompt
				}
			};

			var body = new JObject
			{
				["model"] = model,
				["messages"] = messages
			};

			if (provider == AiTranslationProvider.Claude)
				body["max_tokens"] = MaxOutputTokens;

			return body.ToString(Formatting.None);
		}

		/// <summary>Extracts the model's text output from a provider response.</summary>
		public static string ExtractResponseText(AiTranslationProvider provider, string responseJson)
		{
			JObject root = JObject.Parse(responseJson);

			if (provider == AiTranslationProvider.Claude)
			{
				var text = new StringBuilder();
				if (root["content"] is JArray blocks)
				{
					foreach (JToken block in blocks)
					{
						if ((string)block["type"] == "text")
							text.Append((string)block["text"]);
					}
				}

				if (text.Length == 0)
					throw new InvalidOperationException($"The response contains no text (stop_reason: {(string)root["stop_reason"] ?? "unknown"}).");

				return text.ToString();
			}

			string content = (string)root.SelectToken("choices[0].message.content");
			if (string.IsNullOrEmpty(content))
				throw new InvalidOperationException("The response contains no message content.");

			return content;
		}

		/// <summary>Parses the model output into a key-to-translation dictionary, tolerating markdown fences.</summary>
		public static Dictionary<string, string> ParseTranslations(string modelOutput)
		{
			if (string.IsNullOrWhiteSpace(modelOutput))
				throw new FormatException("The model returned an empty response.");

			int start = modelOutput.IndexOf('{');
			int end = modelOutput.LastIndexOf('}');
			if (start < 0 || end <= start)
				throw new FormatException("The model response does not contain a JSON object.");

			string json = modelOutput.Substring(start, end - start + 1);
			Dictionary<string, string> translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
			if (translations == null || translations.Count == 0)
				throw new FormatException("The model response contains no translations.");

			return translations;
		}

		/// <summary>
		/// Translates the given entries and returns the resulting dictionary, or null when
		/// the request failed or was cancelled. Blocks the editor and shows a progress bar.
		/// </summary>
		public static Dictionary<string, string> TranslateEntries(
			AiTranslationProvider provider, string apiKey, string model,
			string sourceLanguage, string targetLanguage, IDictionary<string, string> entries)
		{
			string prompt = BuildPrompt(sourceLanguage, targetLanguage, entries);
			string body = BuildRequestBody(provider, model, prompt);

			using (UnityWebRequest request = CreateRequest(provider, apiKey, body))
			{
				UnityWebRequestAsyncOperation operation = request.SendWebRequest();

				try
				{
					while (!operation.isDone)
					{
						bool cancelled = EditorUtility.DisplayCancelableProgressBar(
							"AI Translation",
							$"Translating {entries.Count} entries from '{sourceLanguage}' to '{targetLanguage}' using {model}...",
							Mathf.Clamp01(0.1f + request.downloadProgress * 0.9f));

						if (cancelled)
						{
							request.Abort();
							Debug.LogWarning("[LocalizationToolkit] AI translation cancelled by the user.");
							return null;
						}

						Thread.Sleep(50);
					}
				}
				finally
				{
					EditorUtility.ClearProgressBar();
				}

				if (request.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError($"[LocalizationToolkit] AI translation request failed: {request.error}\n{request.downloadHandler?.text}");
					return null;
				}

				try
				{
					string text = ExtractResponseText(provider, request.downloadHandler.text);
					return ParseTranslations(text);
				}
				catch (Exception exception)
				{
					Debug.LogError($"[LocalizationToolkit] Failed to parse the AI translation response: {exception.Message}");
					return null;
				}
			}
		}

		private static UnityWebRequest CreateRequest(AiTranslationProvider provider, string apiKey, string jsonBody)
		{
			var request = new UnityWebRequest(provider.GetEndpoint(), UnityWebRequest.kHttpVerbPOST)
			{
				uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody)),
				downloadHandler = new DownloadHandlerBuffer(),
				timeout = TimeoutSeconds
			};

			request.SetRequestHeader("Content-Type", "application/json");

			if (provider == AiTranslationProvider.Claude)
			{
				request.SetRequestHeader("x-api-key", apiKey);
				request.SetRequestHeader("anthropic-version", "2023-06-01");
			}
			else
			{
				request.SetRequestHeader("Authorization", "Bearer " + apiKey);
			}

			return request;
		}
	}
}

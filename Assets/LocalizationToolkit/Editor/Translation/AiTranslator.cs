using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>Outcome of an AI translation request.</summary>
	public enum AiTranslationStatus
	{
		Success,
		Cancelled,
		Failed
	}

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
		/// <param name="sourceLanguage">Language the entries are written in.</param>
		/// <param name="targetLanguage">Language to translate into.</param>
		/// <param name="entries">Key-to-source-text pairs to translate.</param>
		/// <param name="profile">Optional project profile adding game context, tone and a glossary.</param>
		/// <returns>The complete prompt sent as the user message.</returns>
		public static string BuildPrompt(string sourceLanguage, string targetLanguage,
			IDictionary<string, string> entries, LocalizationAiProfile profile = null)
		{
			var builder = new StringBuilder();
			builder.AppendLine("You are a professional game localization translator.");
			builder.AppendLine($"Translate the following user interface strings from \"{sourceLanguage}\" to \"{targetLanguage}\".");

			if (profile != null)
			{
				if (!string.IsNullOrWhiteSpace(profile.GameDescription))
				{
					builder.AppendLine();
					builder.AppendLine("Game context: " + profile.GameDescription.Trim());
				}

				if (!string.IsNullOrWhiteSpace(profile.Tone))
					builder.AppendLine("Tone of voice: " + profile.Tone.Trim());
			}

			builder.AppendLine();
			builder.AppendLine("Rules:");
			builder.AppendLine("- Return ONLY a valid JSON object that maps every key to its translated value. No markdown, no commentary.");
			builder.AppendLine("- Keep every key exactly as provided and translate only the values.");
			builder.AppendLine("- Preserve placeholders wrapped in curly braces, such as {name} or {score}, exactly as they appear.");
			builder.AppendLine("- Keep the tone natural for game UI text in the target language.");

			if (profile != null && profile.DoNotTranslate != null && profile.DoNotTranslate.Count > 0)
			{
				string terms = string.Join(", ", profile.DoNotTranslate
					.Where(term => !string.IsNullOrWhiteSpace(term))
					.Select(term => term.Trim()));
				if (terms.Length > 0)
					builder.AppendLine("- Never translate these terms; keep them exactly as written: " + terms + ".");
			}

			if (profile != null && !string.IsNullOrWhiteSpace(profile.ExtraInstructions))
			{
				builder.AppendLine();
				builder.AppendLine("Additional instructions:");
				builder.AppendLine(profile.ExtraInstructions.Trim());
			}

			builder.AppendLine();
			builder.AppendLine("Strings to translate:");
			builder.AppendLine(JsonConvert.SerializeObject(entries, Formatting.Indented));
			return builder.ToString();
		}

		/// <summary>Builds the JSON request body for the given provider.</summary>
		/// <param name="provider">Target provider; determines the payload shape.</param>
		/// <param name="model">Model identifier to request.</param>
		/// <param name="prompt">User message produced by <see cref="BuildPrompt"/>.</param>
		/// <returns>The serialized request body.</returns>
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
		/// <param name="provider">Provider the response came from.</param>
		/// <param name="responseJson">Raw response body.</param>
		/// <returns>The concatenated text output of the model.</returns>
		/// <exception cref="InvalidOperationException">Thrown when the response contains no text, e.g. on a refusal.</exception>
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
		/// <param name="modelOutput">Text output of the model.</param>
		/// <returns>Translated values indexed by their original keys.</returns>
		/// <exception cref="FormatException">Thrown when the output contains no parsable JSON object.</exception>
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

		/// <summary>Translates the given entries with the configured provider.</summary>
		/// <param name="provider">AI provider to use.</param>
		/// <param name="apiKey">API key of the user's provider account.</param>
		/// <param name="model">Model identifier to request.</param>
		/// <param name="sourceLanguage">Language the entries are written in.</param>
		/// <param name="targetLanguage">Language to translate into.</param>
		/// <param name="entries">Key-to-source-text pairs to translate.</param>
		/// <returns>The translated entries, or null when the request failed or was cancelled.</returns>
		/// <remarks>Blocks the editor while the request runs and shows a cancelable progress bar.</remarks>
		public static Dictionary<string, string> TranslateEntries(
			AiTranslationProvider provider, string apiKey, string model,
			string sourceLanguage, string targetLanguage, IDictionary<string, string> entries)
		{
			AiTranslationStatus status = TranslateEntries(provider, apiKey, model,
				sourceLanguage, targetLanguage, entries, out Dictionary<string, string> translations);
			return status == AiTranslationStatus.Success ? translations : null;
		}

		/// <summary>Translates the given entries with the configured provider.</summary>
		/// <param name="provider">AI provider to use.</param>
		/// <param name="apiKey">API key of the user's provider account.</param>
		/// <param name="model">Model identifier to request.</param>
		/// <param name="sourceLanguage">Language the entries are written in.</param>
		/// <param name="targetLanguage">Language to translate into.</param>
		/// <param name="entries">Key-to-source-text pairs to translate.</param>
		/// <param name="translations">The translated entries when the method returns <see cref="AiTranslationStatus.Success"/>.</param>
		/// <param name="profile">Optional project profile adding game context, tone and a glossary.</param>
		/// <returns>Whether the request succeeded, failed or was cancelled by the user.</returns>
		/// <remarks>Blocks the editor while the request runs and shows a cancelable progress bar.</remarks>
		public static AiTranslationStatus TranslateEntries(
			AiTranslationProvider provider, string apiKey, string model,
			string sourceLanguage, string targetLanguage, IDictionary<string, string> entries,
			out Dictionary<string, string> translations, LocalizationAiProfile profile = null)
		{
			translations = null;
			string prompt = BuildPrompt(sourceLanguage, targetLanguage, entries, profile);
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
							return AiTranslationStatus.Cancelled;
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
					return AiTranslationStatus.Failed;
				}

				try
				{
					string text = ExtractResponseText(provider, request.downloadHandler.text);
					translations = ParseTranslations(text);
					return AiTranslationStatus.Success;
				}
				catch (Exception exception)
				{
					Debug.LogError($"[LocalizationToolkit] Failed to parse the AI translation response: {exception.Message}");
					return AiTranslationStatus.Failed;
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

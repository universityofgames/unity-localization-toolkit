using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UniversityOfGames.LocalizationToolkit.Editor;

namespace UniversityOfGames.LocalizationToolkit.Tests
{
	public class AiTranslatorTests
	{
		private static readonly Dictionary<string, string> SampleEntries = new Dictionary<string, string>
		{
			["hello"] = "Hello",
			["welcome_player"] = "Welcome, {name}!"
		};

		[Test]
		public void BuildPrompt_ContainsLanguagesKeysAndValues()
		{
			string prompt = AiTranslator.BuildPrompt("English", "Polish", SampleEntries);

			Assert.That(prompt, Does.Contain("\"English\""));
			Assert.That(prompt, Does.Contain("\"Polish\""));
			Assert.That(prompt, Does.Contain("hello"));
			Assert.That(prompt, Does.Contain("Welcome, {name}!"));
			Assert.That(prompt, Does.Contain("ONLY a valid JSON object"));
		}

		[Test]
		public void BuildPrompt_WithProfile_IncludesContextToneGlossaryAndInstructions()
		{
			var profile = UnityEngine.ScriptableObject.CreateInstance<LocalizationAiProfile>();
			try
			{
				profile.GameDescription = "A cozy farming RPG for casual players.";
				profile.Tone = "warm and playful";
				profile.DoNotTranslate.Add("Mana");
				profile.DoNotTranslate.Add("XP");
				profile.ExtraInstructions = "Keep strings short.";

				string prompt = AiTranslator.BuildPrompt("English", "Polish", SampleEntries, profile);

				Assert.That(prompt, Does.Contain("A cozy farming RPG"));
				Assert.That(prompt, Does.Contain("warm and playful"));
				Assert.That(prompt, Does.Contain("Mana, XP"));
				Assert.That(prompt, Does.Contain("Keep strings short."));
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(profile);
			}
		}

		[Test]
		public void BuildPrompt_WithoutProfile_MatchesPlainPrompt()
		{
			Assert.That(AiTranslator.BuildPrompt("English", "Polish", SampleEntries, null),
				Is.EqualTo(AiTranslator.BuildPrompt("English", "Polish", SampleEntries)));
		}

		[Test]
		public void BuildRequestBody_ForClaude_UsesMessagesAndMaxTokens()
		{
			string body = AiTranslator.BuildRequestBody(AiTranslationProvider.Claude, "claude-opus-5", "prompt text");
			JObject root = JObject.Parse(body);

			Assert.That((string)root["model"], Is.EqualTo("claude-opus-5"));
			Assert.That((int)root["max_tokens"], Is.GreaterThan(0));
			Assert.That((string)root["messages"][0]["role"], Is.EqualTo("user"));
			Assert.That((string)root["messages"][0]["content"], Is.EqualTo("prompt text"));
		}

		[Test]
		public void BuildRequestBody_ForOpenAi_UsesChatMessagesWithoutMaxTokens()
		{
			string body = AiTranslator.BuildRequestBody(AiTranslationProvider.OpenAi, "gpt-4o", "prompt text");
			JObject root = JObject.Parse(body);

			Assert.That((string)root["model"], Is.EqualTo("gpt-4o"));
			Assert.That(root["max_tokens"], Is.Null);
			Assert.That((string)root["messages"][0]["role"], Is.EqualTo("user"));
		}

		[Test]
		public void ExtractResponseText_ForClaude_ConcatenatesTextBlocks()
		{
			const string response = "{\"content\":[{\"type\":\"text\",\"text\":\"{\\\"hello\\\":\"},{\"type\":\"text\",\"text\":\"\\\"Cześć\\\"}\"}],\"stop_reason\":\"end_turn\"}";
			Assert.That(AiTranslator.ExtractResponseText(AiTranslationProvider.Claude, response),
				Is.EqualTo("{\"hello\":\"Cześć\"}"));
		}

		[Test]
		public void ExtractResponseText_ForClaude_WithoutText_Throws()
		{
			const string response = "{\"content\":[],\"stop_reason\":\"refusal\"}";
			Assert.That(() => AiTranslator.ExtractResponseText(AiTranslationProvider.Claude, response),
				Throws.InvalidOperationException.With.Message.Contain("refusal"));
		}

		[Test]
		public void ExtractResponseText_ForOpenAi_ReadsFirstChoiceMessage()
		{
			const string response = "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"{\\\"hello\\\":\\\"Cześć\\\"}\"}}]}";
			Assert.That(AiTranslator.ExtractResponseText(AiTranslationProvider.OpenAi, response),
				Is.EqualTo("{\"hello\":\"Cześć\"}"));
		}

		[Test]
		public void ParseTranslations_AcceptsPlainJson()
		{
			Dictionary<string, string> result = AiTranslator.ParseTranslations("{\"hello\":\"Cześć\",\"bye\":\"Pa\"}");
			Assert.That(result["hello"], Is.EqualTo("Cześć"));
			Assert.That(result["bye"], Is.EqualTo("Pa"));
		}

		[Test]
		public void ParseTranslations_StripsMarkdownFences()
		{
			Dictionary<string, string> result = AiTranslator.ParseTranslations("```json\n{\"hello\":\"Cześć\"}\n```");
			Assert.That(result["hello"], Is.EqualTo("Cześć"));
		}

		[Test]
		public void ParseTranslations_WithoutJsonObject_Throws()
		{
			Assert.That(() => AiTranslator.ParseTranslations("Sorry, I cannot help with that."),
				Throws.TypeOf<System.FormatException>());
		}
	}
}

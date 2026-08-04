namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>Supported AI translation providers.</summary>
	public enum AiTranslationProvider
	{
		Claude = 0,
		OpenAi = 1
	}

	/// <summary>Provider-specific defaults and endpoints.</summary>
	public static class AiTranslationProviderInfo
	{
		public const string ClaudeEndpoint = "https://api.anthropic.com/v1/messages";
		public const string OpenAiEndpoint = "https://api.openai.com/v1/chat/completions";

		public const string DefaultClaudeModel = "claude-opus-5";
		public const string DefaultOpenAiModel = "gpt-4o";

		/// <summary>Display name of the provider.</summary>
		public static string GetDisplayName(this AiTranslationProvider provider)
		{
			return provider == AiTranslationProvider.Claude ? "Claude (Anthropic)" : "GPT (OpenAI)";
		}

		/// <summary>Default model identifier used when the user has not entered one.</summary>
		public static string GetDefaultModel(this AiTranslationProvider provider)
		{
			return provider == AiTranslationProvider.Claude ? DefaultClaudeModel : DefaultOpenAiModel;
		}

		/// <summary>HTTP endpoint that translation requests are sent to.</summary>
		public static string GetEndpoint(this AiTranslationProvider provider)
		{
			return provider == AiTranslationProvider.Claude ? ClaudeEndpoint : OpenAiEndpoint;
		}
	}
}

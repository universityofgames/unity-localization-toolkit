using System.Collections.Generic;
using UnityEngine;

namespace UniversityOfGames.LocalizationToolkit.Editor
{
	/// <summary>
	/// Project-specific context that turns AI translations from generic machine output
	/// into production-quality localization: game description, tone of voice and a
	/// glossary of terms that must never be translated.
	/// </summary>
	/// <remarks>
	/// Create one via <c>Assets → Create → Localization Toolkit → AI Translation Profile</c>
	/// and assign it in the AI Translation section of the Localization Editor. The
	/// profile is an editor-only asset — it is never included in builds.
	/// </remarks>
	[CreateAssetMenu(fileName = "LocalizationAiProfile", menuName = "Localization Toolkit/AI Translation Profile")]
	public class LocalizationAiProfile : ScriptableObject
	{
		[TextArea(3, 8)]
		[Tooltip("Short description of the game: genre, setting, audience. Gives the translator context.")]
		public string GameDescription = string.Empty;

		[Tooltip("Desired tone of voice, e.g. 'casual and playful' or 'formal and epic'.")]
		public string Tone = string.Empty;

		[Tooltip("Terms that must never be translated: proper names, stats like XP or Mana, brand names.")]
		public List<string> DoNotTranslate = new List<string>();

		[TextArea(2, 6)]
		[Tooltip("Any additional instructions passed verbatim to the translator model.")]
		public string ExtraInstructions = string.Empty;
	}
}

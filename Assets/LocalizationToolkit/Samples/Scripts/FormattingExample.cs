using System;
using UnityEngine;
using UnityEngine.UI;

namespace UniversityOfGames.LocalizationToolkit.Samples
{
	/// <summary>
	/// Sample: displays <see cref="LocalizationManager.FormatLocalized"/> output,
	/// demonstrating culture-aware number, currency and date formatting per language.
	/// </summary>
	[AddComponentMenu("Localization Toolkit/Samples/Formatting Example")]
	public class FormattingExample : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("Text that displays the formatted output.")]
		private Text _targetText;

		[SerializeField]
		[Tooltip("Translation key containing {score:N0}, {reward:C} and {date:d} placeholders.")]
		private string _key = "stats_line";

		[SerializeField]
		[Tooltip("Sample score value.")]
		private double _score = 987654;

		[SerializeField]
		[Tooltip("Sample reward value formatted as currency.")]
		private double _reward = 49.99;

		private void OnEnable()
		{
			LocalizationManager.LanguageChanged += Refresh;
			Refresh();
		}

		private void OnDisable()
		{
			LocalizationManager.LanguageChanged -= Refresh;
		}

		private void Refresh()
		{
			LocalizationManager manager = LocalizationManager.Instance;
			if (manager == null || !manager.IsLoaded || _targetText == null)
				return;

			_targetText.text = manager.FormatLocalized(_key,
				("score", _score), ("reward", _reward), ("date", DateTime.Now));
		}
	}
}

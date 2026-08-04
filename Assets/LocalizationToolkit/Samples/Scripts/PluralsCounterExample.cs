using UnityEngine;
using UnityEngine.UI;

namespace UniversityOfGames.LocalizationToolkit.Samples
{
	/// <summary>
	/// Sample: displays <see cref="LocalizationManager.GetPlural"/> for an adjustable
	/// count, demonstrating CLDR plural rules across languages.
	/// </summary>
	[AddComponentMenu("Localization Toolkit/Samples/Plurals Counter Example")]
	public class PluralsCounterExample : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("Text that displays the plural output.")]
		private Text _targetText;

		[SerializeField]
		[Tooltip("Base plural key, e.g. 'apples' (uses apples.one/.few/.many/.other).")]
		private string _pluralKey = "apples";

		[SerializeField]
		[Tooltip("Starting count.")]
		private int _count = 1;

		private void OnEnable()
		{
			LocalizationManager.LanguageChanged += Refresh;
			Refresh();
		}

		private void OnDisable()
		{
			LocalizationManager.LanguageChanged -= Refresh;
		}

		/// <summary>Increases the count by one. Wired to the "+" button.</summary>
		public void Increment()
		{
			_count++;
			Refresh();
		}

		/// <summary>Decreases the count by one, never below zero. Wired to the "-" button.</summary>
		public void Decrement()
		{
			_count = Mathf.Max(0, _count - 1);
			Refresh();
		}

		private void Refresh()
		{
			LocalizationManager manager = LocalizationManager.Instance;
			if (manager == null || !manager.IsLoaded || _targetText == null)
				return;

			_targetText.text = manager.GetPlural(_pluralKey, _count);
		}
	}
}

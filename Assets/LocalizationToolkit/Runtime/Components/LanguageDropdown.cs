using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>
	/// Populates a UI <see cref="Dropdown"/> or TextMeshPro <see cref="TMP_Dropdown"/>
	/// with the available languages and switches the active language when the
	/// selection changes.
	/// </summary>
	/// <remarks>
	/// Add this component to a dropdown GameObject — no wiring is required. The options
	/// are filled from <see cref="LocalizationManager.GetAvailableLanguages"/> on
	/// <c>Start</c>, and the currently active language is pre-selected.
	/// </remarks>
	[AddComponentMenu("Localization Toolkit/Language Dropdown")]
	[HelpURL(LocalizationToolkitInfo.DocumentationUrl)]
	public class LanguageDropdown : MonoBehaviour
	{
		private Dropdown _dropdown;
		private TMP_Dropdown _tmpDropdown;

		private void Start()
		{
			_dropdown = GetComponent<Dropdown>();
			_tmpDropdown = GetComponent<TMP_Dropdown>();

			if (_dropdown == null && _tmpDropdown == null)
			{
				Debug.LogWarning("[LocalizationToolkit] LanguageDropdown requires a Dropdown or TMP_Dropdown component on the same GameObject.", this);
				return;
			}

			Populate();
		}

		private void Populate()
		{
			LocalizationManager manager = LocalizationManager.Instance;
			if (manager == null || !manager.IsLoaded)
				return;

			var options = new List<string>(manager.GetAvailableLanguages());
			int activeIndex = Mathf.Max(0, options.IndexOf(manager.ActiveLanguage));

			if (_dropdown != null)
			{
				_dropdown.ClearOptions();
				_dropdown.AddOptions(options);
				_dropdown.SetValueWithoutNotify(activeIndex);
				_dropdown.onValueChanged.AddListener(OnSelectionChanged);
			}
			else
			{
				_tmpDropdown.ClearOptions();
				_tmpDropdown.AddOptions(options);
				_tmpDropdown.SetValueWithoutNotify(activeIndex);
				_tmpDropdown.onValueChanged.AddListener(OnSelectionChanged);
			}
		}

		private void OnSelectionChanged(int index)
		{
			string language = _dropdown != null
				? _dropdown.options[index].text
				: _tmpDropdown.options[index].text;

			LocalizationManager.Instance.LoadLanguage(language);
		}
	}
}

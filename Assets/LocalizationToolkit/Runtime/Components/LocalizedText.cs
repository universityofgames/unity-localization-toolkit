using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>
	/// Keeps a UI Text or TextMeshPro component in sync with the translation
	/// assigned to a key, updating automatically when the language changes.
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[AddComponentMenu("Localization Toolkit/Localized Text")]
	public class LocalizedText : MonoBehaviour
	{
		[SerializeField, FormerlySerializedAs("key")]
		[Tooltip("Translation key used to look up the localized value.")]
		private string _key = string.Empty;

		private Text _uiText;
		private TMP_Text _tmpText;

		/// <summary>Translation key used to look up the localized value.</summary>
		public string Key
		{
			get => _key;
			set
			{
				_key = value;
				Refresh();
			}
		}

		private void OnEnable()
		{
			LocalizationManager.LanguageChanged += Refresh;
			Refresh();
		}

		private void OnDisable()
		{
			LocalizationManager.LanguageChanged -= Refresh;
		}

		/// <summary>Re-applies the translation of the current key to the attached text component.</summary>
		public void Refresh()
		{
			LocalizationManager manager = LocalizationManager.Instance;
			if (manager == null || !manager.IsLoaded || string.IsNullOrEmpty(_key))
				return;

			if (_uiText == null && _tmpText == null)
				CacheTargetComponents();

			string value = manager.GetLocalizedValue(_key);
			if (_tmpText != null)
				_tmpText.text = value;
			else if (_uiText != null)
				_uiText.text = value;
		}

		private void CacheTargetComponents()
		{
			_tmpText = GetComponent<TMP_Text>();
			_uiText = GetComponent<Text>();
			if (_tmpText == null && _uiText == null)
				Debug.LogWarning("[LocalizationToolkit] LocalizedText requires a Text or TMP_Text component on the same GameObject.", this);
		}
	}
}

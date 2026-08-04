using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UniversityOfGames.LocalizationToolkit
{
	/// <summary>
	/// Keeps a UI <see cref="Text"/> or TextMeshPro <see cref="TMP_Text"/> component in
	/// sync with the translation assigned to a key.
	/// </summary>
	/// <remarks>
	/// Add this component next to any text component and assign a translation key.
	/// The text refreshes when the component is enabled and every time
	/// <see cref="LocalizationManager.LanguageChanged"/> is raised. Thanks to
	/// <see cref="ExecuteAlways"/>, language switches made in the inspector preview
	/// directly in the Scene view.
	/// </remarks>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[AddComponentMenu("Localization Toolkit/Localized Text")]
	[HelpURL(LocalizationToolkitInfo.DocumentationUrl)]
	public class LocalizedText : MonoBehaviour
	{
		[SerializeField, FormerlySerializedAs("key")]
		[Tooltip("Translation key used to look up the localized value.")]
		private string _key = string.Empty;

		private Text _uiText;
		private TMP_Text _tmpText;
		private HorizontalAlignmentOptions _originalTmpAlignment;
		private bool _originalTmpAlignmentCaptured;

		/// <summary>Translation key used to look up the localized value. Setting it refreshes the text.</summary>
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
			{
				_tmpText.text = value;
				ApplyTextDirection(manager.IsActiveLanguageRightToLeft);
			}
			else if (_uiText != null)
			{
				_uiText.text = value;
			}
		}

		private void ApplyTextDirection(bool rightToLeft)
		{
			if (!_originalTmpAlignmentCaptured)
			{
				_originalTmpAlignment = _tmpText.horizontalAlignment;
				_originalTmpAlignmentCaptured = true;
			}

			if (_tmpText.isRightToLeftText != rightToLeft)
				_tmpText.isRightToLeftText = rightToLeft;

			_tmpText.horizontalAlignment = rightToLeft
				? MirrorAlignment(_originalTmpAlignment)
				: _originalTmpAlignment;
		}

		private static HorizontalAlignmentOptions MirrorAlignment(HorizontalAlignmentOptions alignment)
		{
			switch (alignment)
			{
				case HorizontalAlignmentOptions.Left: return HorizontalAlignmentOptions.Right;
				case HorizontalAlignmentOptions.Right: return HorizontalAlignmentOptions.Left;
				default: return alignment;
			}
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

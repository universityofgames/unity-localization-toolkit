using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour {
	// key by which the translation is searched for
	public string key;
	private Text _textComponent;

	private void Awake() {
		_textComponent = GetComponent<Text>();
		RefreshText();
		LocalizationManager.OnLanguageChanged += RefreshText;
	}

	private void OnDestroy() {
		LocalizationManager.OnLanguageChanged -= RefreshText;
	}

	/// <summary>Refresh text component</summary>
	private void RefreshText() {
		_textComponent.text = LocalizationManager.instance.GetLocalizedValue(key);
	}
}
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour {
	public string key;
	private Text _myText;

	private void Awake() {
		_myText = GetComponent<Text>();
		RefreshText();
		LocalizationManager.OnLanguageChanged += RefreshText;
	}

	private void OnDestroy() {
		LocalizationManager.OnLanguageChanged -= RefreshText;
	}

	/// <summary>Refresh text</summary>
	private void RefreshText() {
		_myText.text = LocalizationManager.instance.GetLocalizedValue(key);
	}
}
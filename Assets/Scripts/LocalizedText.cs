using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour {
	public string key;
	private Text myText;

	private void Start() {
		myText = GetComponent<Text>();
		RefreshText();
		LocalizationManager.OnLanguageChanged += RefreshText;
	}

	private void OnDestroy() {
		LocalizationManager.OnLanguageChanged -= RefreshText;
	}

	private void RefreshText() {
		myText.text = LocalizationManager.instance.GetLocalizedValue(key);
	}
}
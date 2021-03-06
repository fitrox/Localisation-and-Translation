﻿using UnityEngine;
using UnityEngine.UI;

public class TranslationSample : MonoBehaviour
{
	public Text displayCurrentLanguage;

	public GameObject window;

	GUITranslator guiTranslator;

	void Start ()
	{
		guiTranslator = GUITranslator.Instance;

		//Automatically sets language based on application's language
		guiTranslator.CheckLanguageAndCode ();

		//Updates demonstrative text on the scene
		UIDisplayUpdate ();
	}

	//Select language with given code
	public void ButtonLanguage (string code)
	{
		//Updates GUI with code for new language
		guiTranslator.UpdateGUI (code);

		//Manually updates expecific texts on the scene
		if (displayCurrentLanguage != null)
			UIDisplayUpdate ();
	}

	//Displays a window during the game
	public void ButtonWindow ()
	{
		//activates the window
		window.gameObject.SetActive (true);
		//updates GUI for the new window
		guiTranslator.UpdateGUI ();
	}

	public void ButtonWindowClose ()
	{
		//closes the window
		window.gameObject.SetActive (false);
	}

	private void UIDisplayUpdate ()
	{
		//It is possible to get the translation of a single text instead of using a LocalisedObject.
		//Get translation for given key in the current language
		string current = guiTranslator.GetLocalisedText ("sample_current");
		string language = guiTranslator.GetLocalisedText ("sample_language");
		string code = guiTranslator.Code;

		//Display current language for demonstration purpose
		displayCurrentLanguage.text = string.Format ("{0} : {1} ({2})", current, language, code);
	}
}
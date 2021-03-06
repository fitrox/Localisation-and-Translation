﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalisationAndTranslation;
using System.Text.RegularExpressions;

public class GUITranslator : MonoBehaviour
{
	private string currentLanguage;
	private string code;

	public List<ILocalisedObject> localisedObjectsInScene = new List<ILocalisedObject> ();

	public LanguageManager languageManager;
	public LocalisationManager localisationManager;

	public delegate void HandleOnGUIupdate ();

	public static event HandleOnGUIupdate OnGUIupdate;

	protected static GUITranslator instance;

	public static GUITranslator Instance
	{
		get
		{
			if (instance != null)
				return instance;

			instance = FindObjectOfType<GUITranslator> ();

			if (instance != null)
				return instance;

			return instance;
		}
	}

	void Awake ()
	{
		if (Instance == null)
		{
			instance = this;
		}
		else if (Instance != this)
		{
			Destroy (gameObject);
		}

		if (!CheckAssets ())
			return;

		//Get current language. If none is stored, it's set to main language.
		code = (PlayerPrefs.HasKey ("language")) ? PlayerPrefs.GetString ("language") : languageManager.MainLanguage.code;

		CurrentLanguage = languageManager.GetCurrentLanguage (code);
	}

	//Delays Start to wait all objects are added to the list
	IEnumerator Start ()
	{
		yield return new WaitForSeconds (0.0005f);

		UpdateLocalisedObjects ();
	}

	/// <summary>
	/// Set game's language based on Application's language.
	/// </summary>
	public void CheckLanguageAndCode ()
	{
		string appLanguage = Application.systemLanguage.ToString ();

		Language language;

		if (!CheckAssets ())
			return;

		//Check if application name is supported
		if (languageManager.ContainsLanguage (appLanguage))
		{
			language = languageManager.GetLanguageByName (appLanguage);
		}
		//Otherwise set it to main language
		else
		{
			language = languageManager.MainLanguage;
		}

		SetLanguage (language.code);
	}

	/// <summary>
	/// Adds given localisation the the list
	/// </summary>
	/// <param name="localisedObject"></param>
	public void AddLocalisedObject (ILocalisedObject localisedObject)
	{
		if (localisedObjectsInScene.Contains (localisedObject))
			return;

		localisedObjectsInScene.Add (localisedObject);
	}

	/// <summary> Update localisations on GUI </summary>
	public void UpdateGUI ()
	{
		Invoke ("UpdateLocalisedObjects", 0.0001f);
	}

	/// <summary> Check for translatableObjects and translates them with a given language. </summary>
	/// <param name="code">Code.</param>
	public void UpdateGUI (string code)
	{
		SetLanguage (code);

		Invoke ("UpdateLocalisedObjects", 0.0001f);
	}

	/// <summary> Checks for all localised objects on the scene and updates them to the current language. </summary>
	private void UpdateLocalisedObjects ()
	{
		if (!CheckAssets ())
			return;

		if (OnGUIupdate != null)
			OnGUIupdate.Invoke ();

		foreach (ILocalisedObject localisedObject in localisedObjectsInScene)
		{
			if (string.IsNullOrEmpty (localisedObject.Key))
			{
				Debug.LogWarningFormat ("Localised Object '{0}' has no key.", localisedObject.gameObject.name);
				continue;
			}

			if (localisedObject.Type == EntryType.Text)
			{
				LocalisedText localisedText = (LocalisedText)localisedObject;
				localisedText.Set (GetLocalisedText (localisedObject.Key));
			}
			else if (localisedObject.Type == EntryType.Image)
			{
				LocalisedImage localisedImage = (LocalisedImage)localisedObject;
				localisedImage.Set (GetLocalisedImage (localisedObject.Key));
			}
			else if (localisedObject.Type == EntryType.Audio)
			{
				LocalisedAudio localisedAudio = (LocalisedAudio)localisedObject;
				localisedAudio.Set (GetLocalisedAudio (localisedObject.Key));
			}
			else
			{
				Debug.LogWarningFormat ("No required component found on '{0}'. Check if components are being removed.", localisedObject.gameObject.name);
				continue;
			}
		}
	}

	/// <summary>
	/// Returns translation of a given key.
	/// </summary>
	/// <param name="key"></param>
	/// <returns>The translated text.</returns>
	public string GetLocalisedText (string key)
	{
		string translatedText = languageManager.GetTranslation (key, CurrentLanguage);

		if (string.IsNullOrEmpty (translatedText))
		{
			Debug.LogWarningFormat ("Translation of '{0}' is empty for language '{1}'", key, code);
			translatedText = languageManager.GetTranslation (key, languageManager.MainLanguage.name);

			if (string.IsNullOrEmpty (translatedText))
			{
				Debug.LogWarningFormat ("No translations found for {0}. At least set main language to avoid null.", key);
				return string.Empty;
			}
		}

		translatedText = ParseText (translatedText);

		return translatedText;
	}

	/// <summary>
	/// Parses new line from the string
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	private string ParseText (string text)
	{
		Regex regex = new Regex (@"\\n");
		string[] substrings = regex.Split (text);

		string parsedString = "";

		if (substrings.Length == 1)
		{
			parsedString = substrings[0];
		}
		else
		{
			for (int i = 0; i < substrings.Length; i++)
			{
				if (i < substrings.Length - 1)
					parsedString += substrings[i] + '\n';
				else
					parsedString += substrings[i];
			}
		}

		return parsedString;
	}

	/// <summary>
	///  Returns localised sprite of a given key.
	/// </summary>
	/// <param name="key"></param>
	/// <returns>The Sprite.</returns>
	public Sprite GetLocalisedImage (string key)
	{
		Sprite localisedSprite = localisationManager.GetLocalisedSprite (key, CurrentLanguage);

		if (localisedSprite == null)
		{
			Debug.LogWarningFormat ("Localisation of '{0}' is empty for language '{1}'", key, code);
			localisedSprite = localisationManager.GetLocalisedSprite (key, languageManager.MainLanguage.name);

			if (localisedSprite == null)
			{
				Debug.LogWarningFormat ("No localisations found for {0}. At least set main language to avoid null.", key);
				return null;
			}
		}
		return localisedSprite;
	}

	/// <summary>
	/// Returns localised clip of a given key.
	/// </summary>
	/// <param name="key"></param>
	/// <returns>The AudioClip.</returns>
	public AudioClip GetLocalisedAudio (string key)
	{
		AudioClip localisedClip = localisationManager.GetLocalisedAudioClip (key, CurrentLanguage);

		if (localisedClip == null)
		{
			Debug.LogWarningFormat ("Localisation of '{0}' is empty for language '{1}'", key, code);
			localisedClip = localisationManager.GetLocalisedAudioClip (key, languageManager.MainLanguage.name);

			if (localisedClip == null)
			{
				Debug.LogWarningFormat ("No localisations found for {0}. At least set main language to avoid null.", key);
				return null;
			}
		}
		return localisedClip;
	}

	private bool CheckAssets ()
	{
		if (languageManager == null)
		{
			Debug.LogWarning ("Instance of LanguageManager is not set.");
			return false;
		}
		else if (localisationManager == null)
		{
			Debug.LogWarning ("Instance of LocalisationManager is not set.");
			return false;
		}

		return true;
	}

	/// <summary> Sets Language with given code. </summary>
	/// <param name="code">Code.</param>
	public void SetLanguage (string code)
	{
		this.code = code;
		CurrentLanguage = languageManager.GetCurrentLanguage (code);

		PlayerPrefs.SetString ("Language", code);
	}

	public string CurrentLanguage
	{
		get
		{
			return currentLanguage;
		}

		set
		{
			currentLanguage = value;
		}
	}

	public string Code
	{
		get
		{
			return code;
		}

		set
		{
			code = value;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class NGramV2 : MonoBehaviour {
	//using Manu's letter nGram as a basis

	public TextAsset corpusFile;
	[Range(1, 25)]
	public int n = 3; //choose n value
	[Range(1, 3000)]
	public int nLetters = 500; //choose n letters to display
	public Text dialouge; 
	public InputField playerSpeech;

	private string[] words; //list of words in text file
	//Dictionary for nGram model
	//Key: string that represent the history, or the preious n - 1 characters
	//Value: Dictionary that maps each character to a frequency
	private Dictionary<string, Dictionary<string, float>> nGram; 

	public string[] SplitWords(string s){
		//Input: string to be split
		//split words based on non-words chharacters
		return Regex.Split (s, @"\W+");
	}


	public Dictionary<string, Dictionary<string, float>> normalize(Dictionary<string, Dictionary<string, int>> lm){
		//normalizes the nested dictionary for each history based on frequency in ascending order
		//converts frequency to a probability
		float total = 0f;
		Dictionary<string, Dictionary<string, float>> norm_lm = new Dictionary<string, Dictionary<string, float>> ();

		//goes through each nested dictionary
		foreach (KeyValuePair<string, Dictionary<string,int>> myDict in lm){
			//sorts the entries based on frequency (ascending)
			IEnumerable<KeyValuePair<string,int>> innerDict = from pair in myDict.Value
				orderby pair.Value ascending
				select pair;
			//gets the total frequency
			foreach (KeyValuePair<string, int> entry in innerDict) {
				total += entry.Value;
			}

			//creates new dictionary for entry and makes the sorted entries (innerDict) into a dictioanry
			Dictionary<string, float> sortedDict = new Dictionary<string, float>();
			innerDict = innerDict.ToDictionary(pair => pair.Key, pair => pair.Value);

			//creates the sorted dictionary based on probability
			foreach (KeyValuePair<string, int> entry in innerDict) {
				sortedDict.Add (entry.Key, entry.Value / total);
			}

			//adds normalized dictionary (sortedDict) as an entry
			total = 0f;
			norm_lm.Add (myDict.Key, sortedDict);
		}
		return norm_lm;
	}

	public Dictionary<string, Dictionary<string, float>> train_lm(string text, int n){
		//creates a probablilty distribution based on the previous histoy of letters

		Dictionary<string, Dictionary<string, int>> raw_lm = new Dictionary<string, Dictionary<string, int>> ();
		string history = "";
		//starts with a dummy history
		for (int i = 0; i < n; i++) {
			history += "~";
		}

		Dictionary<string, int> alphabet = new Dictionary<string, int> ();
		alphabet.Add ("a", 0);
		alphabet.Add ("b", 0);
		alphabet.Add ("c", 0);
		alphabet.Add ("d", 0);
		alphabet.Add ("e", 0);
		alphabet.Add ("f", 0);
		alphabet.Add ("g", 0);
		alphabet.Add ("h", 0);
		alphabet.Add ("i", 0);
		alphabet.Add ("j", 0);
		alphabet.Add ("k", 0);
		alphabet.Add ("l", 0);
		alphabet.Add ("m", 0);
		alphabet.Add ("n", 0);
		alphabet.Add ("o", 0);
		alphabet.Add ("p", 0);
		alphabet.Add ("q", 0);
		alphabet.Add ("r", 0);
		alphabet.Add ("s", 0);
		alphabet.Add ("t", 0);
		alphabet.Add ("u", 0);
		alphabet.Add ("v", 0);
		alphabet.Add ("w", 0);
		alphabet.Add ("x", 0);
		alphabet.Add ("y", 0);
		alphabet.Add ("z", 0);

		Debug.Log ("Starting ngam training /n");
		foreach (char c in text) {
			if (!raw_lm.ContainsKey (history)) {
				raw_lm.Add (history, alphabet);
			}
			if (!raw_lm [history].ContainsKey (c.ToString())) {
				raw_lm [history].Add (c.ToString (), 0);
			}

			//increments frequency for the letter given the history
			raw_lm [history] [c.ToString()] += 1;
			//shifts the history to onclude latest character
			history = history.Substring(1) + c;
		}

		//normalizes the ngram
		Dictionary<string, Dictionary<string, float>> lm = normalize (raw_lm);
		Debug.Log("Ending ngram training /n");
		return lm;

	}

	public string generate_letter(Dictionary<string, Dictionary<string, float>> lm, string history){
		//pick letter on given history based on weighted probability
		//Debug.Log("Object reference for ngram is: " + (lm == null));
		if (!lm.ContainsKey (history)) {
			//dummy return
			return "~";
		}

		float rand = UnityEngine.Random.Range (0f, 1f);
		float cumulative = 0f;
		string x = "";
		foreach (KeyValuePair<string, float> letter in lm[history]) {
			//weighted probability to get the next letter
			cumulative += letter.Value;
			if (rand < cumulative) {
				x = letter.Key;
				break;
			}
		}

		return x;
	}

	public string generate_text(Dictionary<string, Dictionary<string, float>> lm, string history = null){
		if (history == null) {
			//creates a blank history
			history = "";
			for (int i = 0; i < n - 1; i++) {
				history += "~";
			}
		} else if (history.Length > n - 1) {
			//takes the last n - 1 letters of the history
			history = history.Substring (history.Length - n + 1);
		} 
		while (history.Length < n - 1) {
			//adds to the front of the history dummy variables to get n - 1 charcters
			history = "~" + history;
		}

		//Debug.Log ("Current History: " + history);

		string text = "";
		//generate the next nletters specified
		for (int i = 0; i < nLetters; i++) {
			//gets next letter
			string c = generate_letter (lm, history);
			//adds to the text so far
			text = text + c;
			Debug.Log ("Letter " + i + ": " + c);
			//shifts history
			history = history.Substring (1) + c;
			Debug.Log ("Current History: " + history);
		}

		return text;
	}


	void Start (){
		nGram = train_lm (corpusFile.text, n);
	}

	public void OnSubmit(){
		//Debug.Log("Object reference for ngram is: " + (nGram == null));
		dialouge.text = generate_text (nGram, playerSpeech.text);
	}

}

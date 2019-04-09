using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;

public class ChatbotLogic : MonoBehaviour {

	//TRIGRAM LEGGO MY EGGO BABY

	public TextAsset[] corpusFile;
	[Range(1, 25)]
	public int n = 3; //choose n value
	[Range(1, 150)]
	public int nLetters = 50; //choose n letters to display
	public Text dialouge; 
	public InputField playerSpeech;
	public string[] BadWords;
	public string[] badResponse;
	public string[] Greetings;
	public string[] greetResponses;
	public float randProb = .01f;

	public static ChatbotLogic logic;

	private string[] words; //list of words in text file
	//Dictionary for nGram model
	//Key: string that represent the history, or the preious n - 1 characters
	//Value: Dictionary that maps each character to a frequency
	private Dictionary<string, Dictionary<string, float>> nGram; 


	public void Save(){
		Debug.Log ("Saved");
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/ngram.dat");

		Ngram g = new Ngram ();
		g.gram = nGram;

		bf.Serialize (file, g);
		file.Close ();
	}

	public void Load(){
		if (File.Exists (Application.persistentDataPath + "/ngram.dat")) {
			Debug.Log("Loaded");
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/ngram.dat",  FileMode.Open);
			Ngram g = (Ngram)bf.Deserialize (file);
			file.Close ();

			nGram = g.gram;
		}
	}

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

	public Dictionary<string, Dictionary<string, float>> train_lm(TextAsset[] texts, int n){
		//Debug.Log ("Starting ngam training /n");
		string text = "";
		foreach (TextAsset file in texts) {
			if (file != null) {
				text = text + file.text + " ";
			}
		}
		text = text.ToLower ();
		string[] words = SplitWords (text);
		Dictionary<string, Dictionary<string, int>> raw_lm = new Dictionary<string, Dictionary<string, int>> ();
		string history = "";

		for (int x = n; x > 1; x--) { 
			for (int i = 0; i < words.Length - x; i++) {
				history = "";
				for (int j = 0; j < x - 1; j++) {
					history = history + words [i + j] + " ";
				}
				history = history.Trim ();

				Dictionary<string, int> wordFreq;
				if (!raw_lm.ContainsKey (history)) {
					wordFreq = new Dictionary<string, int> ();
					raw_lm.Add (history, wordFreq);
				} else {
					wordFreq = raw_lm [history];
				}

				if (!wordFreq.ContainsKey (words [i + x - 1])) {
					wordFreq.Add (words [i + x - 1], 0);
				}
				wordFreq[words [i + x - 1]] += 1;
			}
		}

		Dictionary<string, Dictionary<string, float>> lm = normalize (raw_lm);
		//Debug.Log("Ending ngram training /n");
		return lm;
	}

	public string generate_letter(Dictionary<string, Dictionary<string, float>> lm, string history){
		//pick word on given history based on weighted probability
		//Debug.Log("Object reference for ngram is: " + (lm == null));
		string[] words = SplitWords(history);

		for (int x = n; x > 1; x--) { 
			for (int i = 0; i < words.Length - x + 1; i++) {
				history = "";
				for (int j = 0; j < x - 1; j++) {
					history = history + words [i + j] + " ";
				}
				history = history.Trim ();
			}
			if (lm.ContainsKey (history)) {
				break;
			}
		}

		if (!lm.ContainsKey (history)) {
			//random choice

			foreach (KeyValuePair<string, Dictionary<string, float>> entry in lm) {
				if (UnityEngine.Random.Range (0f, 1f) < randProb) {
					history = entry.Key;
					break;
				}
			}
		}

		float rand = UnityEngine.Random.Range (0f, 1f);
		float cumulative = 0f;
		string a = "";
		foreach (KeyValuePair<string, float> letter in lm[history]) {
			//weighted probability to get the next letter
			cumulative += letter.Value;
			if (rand < cumulative) {
				a = letter.Key;
				break;
			}
		}
		a = a + " ";
		return a;
	}

	public string generate_text(Dictionary<string, Dictionary<string, float>> lm, string history = null){
		if (history == null) {
			//creates a random history
			foreach (KeyValuePair<string, Dictionary<string, float>> entry in lm) {
				if (UnityEngine.Random.Range (0f, 1f) < randProb) {
					history = entry.Key;
					break;
				}
			}
		} 
		string[] words = SplitWords(history);
		if (words.Length > n - 1) {
			//takes the last n - 1 letters of the history

			history = "";
			for (int i = words.Length - n; i < words.Length; i++) {
				history = history + words [i] + " ";
			}
		} 


		//Debug.Log ("Current History: " + history);

		string text = "";
		//generate the next nletters specified
		for (int i = 0; i < UnityEngine.Random.Range(50, 150); i++) {
			//gets next letter
			string c = generate_letter (lm, history);

			//adds to the text so far
			text = text + c;
			//Debug.Log ("Word " + i + ": " + c);
			//shifts history
			history = history.Substring (1) + c;
			//Debug.Log ("Current History: " + history);
		}

		return text;
	}

	bool containsCertainWords(string text, string[] banned){
		foreach (string word in banned) {
			if (text.ToLower().Contains (word.ToLower()))
				return true;
		}
		return false;
	}

	/*
	void Awake (){
		Load ();
		if (nGram == null) {
			nGram = train_lm (corpusFile.text, n);
			Save ();
		}
	}
	*/

	void Awake(){
		logic = this;
	}

	void Start(){

		UnityEngine.Object[] store = Resources.LoadAll ("_Corpus", typeof(TextAsset));
		corpusFile = new TextAsset[store.Length];
		for (int i = 0; i < store.Length; i++) {
			corpusFile [i] = (TextAsset)store [i];
			Debug.Log ("Loaded in: " + corpusFile [i].name);
		}

		nGram = train_lm (corpusFile, n);
	}

	public void OnSubmit(){
		//Debug.Log("Object reference for ngram is: " + (nGram == null));
		if (playerSpeech.text.ToLower() == ""){
			dialouge.text = "You are supposed to say something you know?";
		}
		else if (containsCertainWords (playerSpeech.text.ToLower (), BadWords)) {
			dialouge.text = badResponse [UnityEngine.Random.Range (0, badResponse.Length)];
		} else if (containsCertainWords (playerSpeech.text.ToLower (), Greetings)) {
			dialouge.text = greetResponses [UnityEngine.Random.Range (0, greetResponses.Length)];
		} else {
			string temp = generate_text (nGram, playerSpeech.text.ToLower ());
			while (containsCertainWords (temp, BadWords)) {
				temp = generate_text (nGram, playerSpeech.text.ToLower ());
			}
			dialouge.text = temp;
		}
	}

}

[Serializable]
class Ngram{
	public Dictionary<string, Dictionary<string, float>> gram; 
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Text;
using System.Text.RegularExpressions;

public class NgramV1 : MonoBehaviour {
	/*
	Problems with this version:
		-Way to slow on start up
		-just takes snipits of the original text file, so not really an n gram
	Improvements to be made:
		-use letters and not words for probability so that it is not rigid to the text file
		-ban some words
		-have user input as history
	*/


	public TextAsset corpusFile;
	[Range(1, 25)]
	public int n = 3; //choose n value
	public Text dialouge; 

	private string[] words; //list of words in text file
	private Dictionary<string, List<string>> nGram; //ngram model

	public string[] SplitWords(string s){
		//Input: string to be split
		//split words based on non-words chharacters
		return Regex.Split (s, @"\W+");
	}

	public void spawnNgram(string text, int n = 2){
		//Input: Text file used as corpus and some n value if specified
		//instantiate an ngram based on the given text file
		if (text == null) {
			Debug.Log ("Invalid Input: Text should not be null");
			return;
		}
		if (n<1 || text.Length == 0){
			Debug.Log ("Invalid Input: ngram was given a n of " + n + " or a text size of " + text.Length);
			return;
		}
		words = SplitWords (text);
		nGram = new Dictionary<string, List<string>> ();
		this.n = n;
	}

	public Dictionary<string, List<string>> generateNgrams(){
		//fills the Ngram based on the list of words

		for (int i = 0; i <= words.Length - n; i++){
			//builds histories of length n - 1 then deposits to ngram 
			//history stored as string builder to easily append
			StringBuilder sb = new StringBuilder ();

			int j = 0;
			while (j < n - 1) {
				sb.Append (words [i + j].Trim ());
				j++;
				if (j < n - 1)
					sb.Append (" ");
			}

			String key = sb.ToString ();

			if (!nGram.ContainsKey (key)) {
				//creates new key entry for previously nonexist keys
				List<string> list = new List<string> ();
				list.Add (words [i + j]);
				nGram.Add (key, list);
			} else {
				//updates list of potential words follwoing a history
				List<string> list = nGram [key];
				list.Add (words [i + j]);
			}
		}
		return nGram;
	}

	public string getNgramsText(string userInput = null){
		StringBuilder result = new StringBuilder ();
		StringBuilder startSb = new StringBuilder ();
		int st = UnityEngine.Random.Range(0, words.Length - n);

		for (int i = 0; i < n - 1; i++) {
			startSb.Append(words [st + i]);
			if (i + 1 < n - 1) {
				startSb.Append (" ");
			}
		}
		/*
		if (userInput == null) {
			for (int i = 0; i < n - 1; i++) {
				startSb.Append [st + i];
				if (i + 1 < n - 1) {
					startSb.Append (" ");
				}
			}
		}
		*/

		string start = startSb.ToString ();
		result.Append (start + " ");

		while (true) {
			int size = nGram [start].Capacity;
			string next;

			if (size > 1) {
				st = UnityEngine.Random.Range(0, size - 1);
				next = nGram [start] [st];
				nGram [start].RemoveAt(st);
			} else {
				st = 0;
				next = nGram [start] [st];
			}

			string[] start_split = SplitWords (start);
			string nextKey;

			if (start_split.Length > 1) {
				nextKey = start.Substring (0, start_split [0].Length + 1);
				start = nextKey + " " + next;
			} else {
				start = next;
			}

			result.Append (next);

			if (nGram.ContainsKey (start))
				result.Append (" ");
			else
				break;

		}

		return result.ToString ();
	}


	void Start (){
		spawnNgram (corpusFile.text, n);
		generateNgrams ();
	}

	public void OnSubmit(){
		dialouge.text = getNgramsText ();
	}
}

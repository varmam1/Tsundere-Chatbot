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

public class ChatBotPsuedo : MonoBehaviour {

	public Text dialouge; 
	public InputField playerSpeech;
	private Dictionary<string, string[]> responses;

	bool containsWord(string text, string[] res){
		foreach (string word in res) {
			if (text.Contains (word))
				return true;
		}
		return false;
	}

	// Use this for initialization
	void Start () {
		responses = new Dictionary<string, string[]> ();
		responses.Add("hello", new string[] {"Hello there!", "Hi!", "Hello!"});
		responses.Add("hi", new string[] {"Hello there!", "Hi!", "Hello!"});
		}
	
	public void OnSubmit(){
		if (responses.ContainsKey(playerSpeech.text.ToLower ()) && containsWord (playerSpeech.text.ToLower (), responses[playerSpeech.text.ToLower ()])) {
			dialouge.text = responses [playerSpeech.text.ToLower ()] [UnityEngine.Random.Range (0, responses [playerSpeech.text.ToLower ()].Length)];
		} else {
			dialouge.text = "I was not programmed for that";
		}
	}
}

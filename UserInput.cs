using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInput : MonoBehaviour {

	//user Input Field
	public InputField playerSpeech;

	//log the user's text
	private string userLog;

	public void OnSubmit(){
		//set sentences here
		userLog = playerSpeech.text;

		//Debug.Log ("You said: " + userLog);
	}
}

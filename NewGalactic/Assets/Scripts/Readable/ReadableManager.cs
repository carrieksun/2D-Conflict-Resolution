﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ReadableManager : MonoBehaviour {
	public bool shouldFirstPerson = false;
	public Image arrowKey;
	public Text mainText;
	public ReadableTrigger currTrigger = null;
	public Toggle firstp;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void UpdateText(string newText, ReadableTrigger rt){
		mainText.text = newText;
		currTrigger = rt;

	}

	public void SetShouldFirstPerson(){
		bool shouldFP = firstp.isOn;
		shouldFirstPerson = shouldFP;

		if (currTrigger != null) {
			currTrigger.updateText ();
		}
	}
		
}

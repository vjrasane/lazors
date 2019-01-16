using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TurnText : MonoBehaviour {

	private Text element;
	public string text;
	// Use this for initialization
	void Start () {
		this.element = this.GetComponent<Text> ();
	}

	public void RefreshText() {
		this.element.text = this.text;
	}

	// Update is called once per frame
	void Update () {
	
	}
}

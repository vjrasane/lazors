using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TurnText : MonoBehaviour {

	private Text text;
	public string playerName;
	// Use this for initialization
	void Start () {
		this.text = this.GetComponent<Text> ();
	}

	public void RefreshText(){
		this.text.text = playerName + "'s turn";
	}

	// Update is called once per frame
	void Update () {
	
	}
}

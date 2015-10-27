using UnityEngine;
using System.Collections;

public class PlayerColors : MonoBehaviour {

	public Color[] colors;
	// Use this for initialization
	void Start () {
		Singletons.COLORS = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public Color GetRandom(){
		return colors[(int)Random.Range (0, colors.Length)];
	}
}

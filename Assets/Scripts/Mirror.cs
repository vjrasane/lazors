using UnityEngine;
using System.Collections;

public class Mirror : Positional {

	private bool flipped = false;

	public void Flip(){
		this.flipped = !flipped;
		this.transform.Rotate (new Vector3 (0, 180, 0));

		if(this.position != null && !this.preview)
			grid.RerouteLazers (this);
	}

	void OnMouseDown(){
		Flip ();
	}

	public bool IsFlipped(){
		return flipped;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

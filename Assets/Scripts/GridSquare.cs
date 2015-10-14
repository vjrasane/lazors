using UnityEngine;
using System.Collections;

public class GridSquare : Positional {

	private SpriteRenderer renderer;

//	public Sprite inactive;
//	public Sprite active;

	public Color activeColor;
	public Color inactiveColor;

	// Use this for initialization
	void Start () {
		this.renderer = this.GetComponent<SpriteRenderer> ();
		this.renderer.color = inactiveColor;
	}

	void OnMouseDown(){

	}

	void OnMouseOver(){
		//this.renderer.enabled = true;
		this.renderer.color = activeColor;
		HandleClick ();
	}

	void OnMouseExit(){
		//this.renderer.enabled = false;
		this.renderer.color = inactiveColor;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void HandleClick ()
	{
		if (Input.GetMouseButtonDown (0)) {
			grid.PutMirror (position, false);
			Destroy (this.gameObject);
		} else if (Input.GetMouseButtonDown (1)) {
			grid.PutSafeZone (position);
			Destroy (this.gameObject);
		}

	}
}

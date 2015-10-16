using UnityEngine;
using System.Collections;

public class GridSquare : Positional {

	// Shitty Unity doesn't actually provide access to renderer/collider but complains about using them as a variable...
	#pragma warning disable 0108
	private SpriteRenderer renderer;
	private BoxCollider2D collider;

	public bool full = false;

	public Color activeColor;
	public Color inactiveColor;

	void Awake(){
		this.renderer = this.GetComponent<SpriteRenderer> ();
		this.renderer.color = inactiveColor;
		this.collider = this.GetComponent<BoxCollider2D> ();
	}

	// Use this for initialization
	void Start () {

	}

	void OnMouseDown(){

	}

	void OnMouseOver(){
		this.renderer.color = activeColor;
		ShowPreview ();
		HandleClick ();
	}

	void OnMouseExit(){
		this.renderer.color = inactiveColor;
		HidePreview ();
	}

	void Update () {
	
	}

	void ShowPreview ()
	{
		grid.selectedPiece.gameObject.SetActive (true);
		grid.selectedPiece.transform.position = this.transform.position;
	}

	void HidePreview ()
	{
		grid.selectedPiece.gameObject.SetActive (false);
	}

	void HandleClick ()
	{
		if (Input.GetMouseButtonDown (0)) {
			SetDisabled(true);
			grid.PutMirror (position, grid.selectedPiece.IsFlipped());

		} else if (Input.GetMouseButtonDown (1)) {
			SetDisabled(true);
			grid.PutSafeZone (position);
		}
	}

	public void SetDisabled(bool disabled){
		this.collider.enabled = !disabled;
	}
}

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

	private bool piecePreview = false;
	private bool lazerPreview = false;

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
		if (!piecePreview) {
			this.piecePreview = true;
			grid.selectedPiece.gameObject.SetActive (true);
			grid.selectedPiece.transform.position = this.transform.position;
			grid.selectedPiece.position = this.position;
		}

		if (!lazerPreview && !grid.Firing ()) {
			this.lazerPreview = true;
			grid.PreviewLazers (grid.selectedPiece);
		}
	}

	void HidePreview ()
	{
		if (piecePreview) {
			grid.selectedPiece.gameObject.SetActive (false);
			this.piecePreview = false;
		}

		if (lazerPreview) {
			grid.ClearPreviews();
			this.lazerPreview = false;
		}
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

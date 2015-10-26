using UnityEngine;
using System.Collections;

public class GridSquare : Positional {

	// Shitty Unity doesn't actually provide access to renderer/collider but complains about using them as a variable...
	#pragma warning disable 0108
	private SpriteRenderer renderer;
	private BoxCollider2D collider;

	public Piece piece = null;

	public Color activeColor;
	public Color inactiveColor;

	private bool piecePreview = false;
	private bool lazerPreview = false;
	private bool hoverPreview = false;

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
		if (!piecePreview && this.piece == null) {
			this.piecePreview = true;
			grid.PreviewAt(this);
		}

		if(!hoverPreview && this.piece != null) {
			this.hoverPreview = true;
			this.piece.OnHover();
		}

		if (!lazerPreview) {
			this.lazerPreview = true;
			grid.PreviewLazers ();
		}
	}

	void HidePreview ()
	{
		if (piecePreview) {
			grid.HidePreview();
			this.piecePreview = false;
		}

		if (hoverPreview) {
			this.piece.OnExit();
			this.hoverPreview = false;
		}

		if (lazerPreview) {
			grid.ClearPreviews();
			this.lazerPreview = false;
		}
	}

	void HandleClick ()
	{
		if(this.grid.turnDone)
			return;

		if (Input.GetMouseButtonDown (0)) {
			if(this.piece != null){
				this.piece.OnClick();
			} else {
				grid.PutMirror (this, grid.previewPiece.IsFlipped());
			}
		} else if (Input.GetMouseButtonDown (1)) {

			// TODO Move to piece selection, is buggy
			this.piece = grid.PutSafeZone (this);
		}
	}

}

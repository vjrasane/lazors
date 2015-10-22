using UnityEngine;
using System.Collections;

public class Mirror : Piece {

	private bool flipped = false;

	public override void  OnClick(){
		Flip ();
	}

	public void Flip(){
		DoFlip ();
		
		if (this.position != null && !this.preview) {
			grid.FireTurrets ();
			grid.ChangeTurn();
		}
	}

	public void SetFlipped(bool flipped){
		if (this.flipped != flipped)
			DoFlip ();
	}

	void DoFlip ()
	{
		this.flipped = !flipped;
		this.transform.Rotate (new Vector3 (0, 180, 0));
	}

	public bool IsFlipped(){
		return flipped;
	}

	private bool revertFlip;
	// Use this for initialization
	public override void OnHover () {
		revertFlip = this.grid.previewPiece.IsFlipped () == this.flipped;
		if (revertFlip)
			this.grid.previewPiece.Flip ();
		this.grid.PreviewAt (this);

		grid.arrows.transform.position = this.transform.position;
		grid.arrows.SetActive (true);
	}

	public override void OnExit () {
		this.grid.HidePreview ();
		if (revertFlip)
			this.grid.previewPiece.Flip ();
		grid.arrows.SetActive (false);
	}
}

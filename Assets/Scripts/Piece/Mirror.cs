using UnityEngine;
using System.Collections;

public class Mirror : PieceObject {

	private bool flipped = false;

	public void Flip(){
		DoFlip ();
		
		if (this.position != null && !this.preview) {
			Singletons.GRID.FireTurrets ();
			Singletons.GRID.ChangeTurn();
			Singletons.GRID.CheckTurn();
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

	private bool wasFlipped;
	private Piece.PieceType rememberPiece;

	#region implemented abstract members of PieceObject

	public override Piece.PieceType GetPieceType ()
	{
		return Piece.PieceType.Mirror;
	}

	public override void  HandleClick(){
		Flip ();
	}
	
	public override void HandleHover () {
		rememberPiece = Singletons.GRID.previewPiece.GetPieceType();

		Singletons.GRID.SetPreviewPiece (Piece.PieceType.Mirror); 

		var preview = Singletons.GRID.previewPiece.GetComponent<Mirror> ();
		wasFlipped = preview.IsFlipped ();
		if (wasFlipped == this.flipped)
			preview.Flip ();
		Singletons.GRID.PreviewAt (this);

		Singletons.GRID.arrows.transform.position = this.transform.position;
		Singletons.GRID.arrows.SetActive (true);

		Singletons.GRID.ClearPreviews ();
		Singletons.GRID.PreviewLazers ();
	}

	public override void HandleExit () {
		Singletons.GRID.HidePreview ();

		var preview = Singletons.GRID.previewPiece.GetComponent<Mirror> ();
	
		preview.SetFlipped (wasFlipped);

		Singletons.GRID.arrows.SetActive (false);
		Singletons.GRID.SetPreviewPiece (rememberPiece); 
	}

	public override void HandleRotate(){
		DoFlip ();
	}
	
	#endregion
}

using UnityEngine;
using System.Collections;

public abstract class PieceObject : PositionalObject {

	public abstract Piece.PieceType GetPieceType();
	public abstract Piece AsPiece();

	public Color normalColor;
	public Color previewColor;

	public void SetPreview (bool preview){
		//this.SetPreview(preview);
		this.SetColor(preview);
	}

	public virtual void SetColor(bool preview){
		this.GetComponent<SpriteRenderer> ().color = preview ? this.previewColor : this.normalColor;
	}

	public void Rotate ()
	{
		if (GameSettings.ALLOWED_ROTATES.Contains (this.GetPieceType ()))
			HandleRotate ();
	}

	public void OnClick ()
	{
		if (GameSettings.ALLOWED_ACTIVATES.Contains (this.GetPieceType ()))
			HandleClick ();
		if (Singletons.GRID.squares [this.Position].hover)
			HandleHover ();
	}

	public void OnHover ()
	{
		if (GameSettings.ALLOWED_ACTIVATES.Contains (this.GetPieceType ()))
			HandleHover ();
		else
			DisplayBlocked ();
	}

	public void OnExit(){
		if (GameSettings.ALLOWED_ACTIVATES.Contains (this.GetPieceType ()))
			HandleExit ();
		else
			HideBlocked ();
	}

	public virtual void HandleClick (){}
	public virtual void HandleExit (){}
	public virtual void HandleHover (){}
	public virtual void HandleRotate (){}

	public void DisplayBlocked (){
		Singletons.GRID.blocked.transform.position = this.transform.position;
		Singletons.GRID.blocked.SetActive (true);
	}

	public void HideBlocked (){
		Singletons.GRID.blocked.SetActive (false);
	}

}

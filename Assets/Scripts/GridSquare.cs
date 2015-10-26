using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GridSquare : Positional, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

	// Shitty Unity doesn't actually provide access to renderer/collider but complains about using them as a variable...
	#pragma warning disable 0108
	private SpriteRenderer renderer;

	public PieceObject piece = null;

	public Color activeColor;
	public Color inactiveColor;

	private bool piecePreview = false;
	private bool lazerPreview = false;
	private bool hoverPreview = false;

	void Awake(){
		this.renderer = this.GetComponent<SpriteRenderer> ();
		this.renderer.color = inactiveColor;
	}

	// Use this for initialization
	void Start () {

	}

	void OnMouseDown(){

	}


	void Update () {
	}

	public void ShowPreview ()
	{
		if (!piecePreview && this.piece == null) {
			this.piecePreview = true;
			Singletons.GRID.PreviewAt(this);
		}

		if(!hoverPreview && this.piece != null) {
			this.hoverPreview = true;
			this.piece.OnHover();
		}

		if (!lazerPreview) {
			this.lazerPreview = true;
			Singletons.GRID.PreviewLazers ();
		}
	}

	void HidePreview ()
	{
		if (piecePreview) {
			Singletons.GRID.HidePreview();
			this.piecePreview = false;
		}

		if (hoverPreview) {
			this.piece.OnExit();
			this.hoverPreview = false;
		}

		if (lazerPreview) {
			Singletons.GRID.ClearPreviews();
			this.lazerPreview = false;
		}
	}

	void HandleClick ()
	{
		if(Singletons.GRID.turnDone)
			return;

		if (Input.GetMouseButtonDown (0)) {
			if(this.piece != null){
				this.piece.OnClick();
			} else {
				Singletons.GRID.PutPiece (this);
			}
		} else if (Input.GetMouseButtonDown (1)) {

			// TODO Move to piece selection, is buggy
			this.piece = Singletons.GRID.PutSafeZone (this);
		}
	}

	#region IPointerClickHandler implementation
	public void OnPointerClick (PointerEventData eventData)
	{
		if(Singletons.GRID.turnDone)
			return;
		
		if (eventData.button.Equals(PointerEventData.InputButton.Left)) {
			if(this.piece != null){
				this.piece.OnClick();
			} else {
				Singletons.GRID.PutPiece(this);
			}
		} else if (eventData.button.Equals(PointerEventData.InputButton.Right)) {
			
			// TODO Move to piece selection, is buggy
			this.piece = Singletons.GRID.PutSafeZone (this);
		}
	}
	#endregion

	public bool hover = false;
	#region IPointerEnterHandler implementation

	public void OnPointerEnter (PointerEventData eventData)
	{
		this.hover = true;
		this.renderer.color = activeColor;
		ShowPreview ();
	}

	#endregion

	#region IPointerExitHandler implementation

	public void OnPointerExit (PointerEventData eventData)
	{
		this.hover = false;
		this.renderer.color = inactiveColor;
		HidePreview ();
	}

	#endregion
}

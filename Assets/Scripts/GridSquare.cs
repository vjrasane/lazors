using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GridSquare : PositionalObject, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

	// Shitty Unity doesn't actually provide access to renderer/collider but complains about using them as a variable...
	#pragma warning disable 0108
	private SpriteRenderer renderer;

	public PieceObject piece = null;

	public Color activeColor;
	public Color inactiveColor;

	private bool piecePreview = false;
	private bool lazerPreview = false;
	private bool hoverPreview = false;

	void Start(){
		this.renderer = this.GetComponent<SpriteRenderer> ();
		this.renderer.color = inactiveColor;
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

	public void SetActive ()
	{
		this.renderer.color = activeColor;
	}

	public void SetInactive ()
	{
		this.renderer.color = inactiveColor;
	}

	#region IPointerClickHandler implementation
	public void OnPointerClick (PointerEventData eventData)
	{
		if(Singletons.GRID.turnDone)
			return;
		
		if (eventData.button.Equals(PointerEventData.InputButton.Left)) {
			if(this.piece != null){
				Singletons.GRID.Activate(this.Position);
			} else {
				Singletons.GRID.Place(this.Position);
			}
		}
	}
	#endregion

	public bool hover = false;
	#region IPointerEnterHandler implementation

	public void OnPointerEnter (PointerEventData eventData)
	{
		this.hover = true;
		SetActive ();
		ShowPreview ();
	}

	#endregion

	#region IPointerExitHandler implementation

	public void OnPointerExit (PointerEventData eventData)
	{
		this.hover = false;
		SetInactive ();
		HidePreview ();
	}

	#endregion
}

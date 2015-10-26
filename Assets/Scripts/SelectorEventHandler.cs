using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectorEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler {

	public Color selectedColor;
	public Color clickColor;
	public Color activeColor;
	public Color inactiveColor;

	private Image borderImage;
	private Image pieceImage;

	public PieceSelector container;
	public Piece.PieceType piece;

	void Awake(){
		borderImage = this.GetComponent<Image> ();
		pieceImage = this.transform.FindChild ("piece").GetComponent<Image> ();
	}

	public void SetSprite(Sprite sprite){
		pieceImage.sprite = sprite;
	}

	#region IPointerEnterHandler implementation

	public void OnPointerEnter (PointerEventData eventData)
	{
		if (this.Equals (PieceSelector.selected))
			return;
		borderImage.color = activeColor;
		hover = true;
	}

	#endregion

	#region IPointerExitHandler implementation

	public void OnPointerExit (PointerEventData eventData)
	{
		if (this.Equals (PieceSelector.selected))
			return;
		borderImage.color = inactiveColor;
		hover = false;
	}

	#endregion

	private bool hover;
	#region IPointerDownHandler implementation
	public void OnPointerDown (PointerEventData eventData)
	{
		this.borderImage.color = clickColor;
	}
	#endregion

	#region IPointerUpHandler implementation

	public void OnPointerUp (PointerEventData eventData)
	{
		if (hover) {
			this.SetSelected();
		}
	}

	public void SetSelected ()
	{
		this.container.Select(this);
		this.borderImage.color = selectedColor;
	}

	#endregion

	public void Deselect ()
	{
		this.borderImage.color = inactiveColor;
	}
}

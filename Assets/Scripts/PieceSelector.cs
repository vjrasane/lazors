using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PieceSelector : MonoBehaviour {

	// TODO load settings

	public GameObject selectorPrefab;

	public static SelectorEventHandler selected = null;

	private List<SelectorEventHandler> selectors = new List<SelectorEventHandler> ();

	public Sprite mirrorSprite;
	public Sprite safeZoneSprite;

	void Awake(){
		Singletons.PIECE_SELECTOR = this;
	}

	// Use this for initialization
	void Start () {
		foreach (Piece.PieceType piece in GameSettings.ALLOWED_PIECES) {
			var selector = Instantiate(selectorPrefab).GetComponent<SelectorEventHandler>();
			selector.transform.SetParent(this.transform);
			selector.container = this;
			selector.piece = piece;

			switch(piece)
			{
			case Piece.PieceType.Mirror:
				selector.SetSprite(mirrorSprite);
				break;
			case Piece.PieceType.SafeZone:
				selector.SetSprite(safeZoneSprite);
				break;
			}

			selectors.Add(selector);
		}

		this.selectors[0].SetSelected();
	}

	public void Select(SelectorEventHandler selector){
		if(selected)
			selected.Deselect ();
		selected = selector;
		Singletons.GRID.Select (selected.piece);
	}

	// Update is called once per frame
	void Update () {
	
	}
}

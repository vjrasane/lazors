using UnityEngine;
using System.Collections;

public class SafeZone : PieceObject {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	#region implemented abstract members of PieceObject

	public override Piece.PieceType GetPieceType ()
	{
		return Piece.PieceType.SafeZone;
	}

	#endregion
}

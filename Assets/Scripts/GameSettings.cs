using UnityEngine;
using System.Collections.Generic;

public class GameSettings {

	public static List<Piece.PieceType> ALLOWED_PIECES = new List<Piece.PieceType>();
	public static List<Piece.PieceType> ALLOWED_CLICKS = new List<Piece.PieceType>();
	public static List<Piece.PieceType> ALLOWED_ROTATES = new List<Piece.PieceType>();

	// TODO
//	public int flipSafePeriod;
//
//	public bool allowDraw;

	// Defaults
	static GameSettings(){
		ALLOWED_PIECES.Add (Piece.PieceType.Mirror);
		ALLOWED_PIECES.Add (Piece.PieceType.SafeZone);

		ALLOWED_CLICKS.Add (Piece.PieceType.Mirror);
		ALLOWED_ROTATES.Add (Piece.PieceType.Mirror);
	}

}

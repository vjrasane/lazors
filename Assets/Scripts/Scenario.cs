using UnityEngine;
using System.Collections.Generic;

public class Scenario
{
	public Dictionary<Coordinate, Piece> pieces = new Dictionary<Coordinate, Piece> ();
	public Queue<Move> moves = new Queue<Move> ();

	public int playerCount = 0;

	public Scenario(){
		Coordinate turret1 = Direction.LEFT.toCoordinate () * 2;
		Coordinate turret2 = Direction.RIGHT.toCoordinate () * 2;
		pieces.Add (turret1, new Piece.Turret(Direction.LEFT, ++playerCount));
		pieces.Add (turret2, new Piece.Turret(Direction.RIGHT, ++playerCount));

		Cross (turret1);
		Cross (turret2);
	}

	private void Surround(Coordinate pos){
		foreach (Direction c in Direction.ALL) {
			var target = pos + c;
			if(!pieces.ContainsKey(target))
				pieces.Add (target, Piece.SafeZone.INSTANCE);
		}
	}

	private void Cross(Coordinate pos){
		foreach (Direction c in Direction.CROSS) {
			var target = pos + c;
			if(!pieces.ContainsKey(target))
				pieces.Add (target, Piece.SafeZone.INSTANCE);
		}
	}

	public abstract class Move : Positional {
		
		public Player player;
		private Coordinate position;
		private bool preview;
		
		#region Positional implementation
		public Coordinate Position {
			get {
				return position;
			}
			set {
				position = value;
			}
		}
		public bool Preview {
			get {
				return preview;
			}
			set {
				preview = value;
			}
		}
		#endregion
	
		public Move(Coordinate pos,  Player player){
			this.position = pos;
			this.player = player;
		}
	}

	public class Place : Move {

		public Piece piece;
		
		public Place(Coordinate pos, Piece piece, Player player) : base(pos, player){
			this.piece = piece;
		}
	}

	public class Activate : Move {
		public Activate(Coordinate pos, Player player) : base(pos, player){}
	}

}


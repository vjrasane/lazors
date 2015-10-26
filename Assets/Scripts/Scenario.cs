using UnityEngine;
using System.Collections.Generic;

public class Scenario
{
	public Dictionary<Coordinate, Piece> pieces = new Dictionary<Coordinate, Piece> ();
	public List<Move> moves = new List<Move> ();

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

		public Move(Coordinate pos, Player player){
			this.position = pos;
			this.player = player;
		}
	}

	public class PlaceMirror : Move {
		
		public bool flipped;
		
		public PlaceMirror(Coordinate pos, Player player, bool flipped) : base(pos, player){
			this.flipped = flipped;
		}
	}

	public class FlipMirror : Move {
		public FlipMirror(Coordinate pos, Player player) : base(pos, player){}
	}

	public class PlaceSafeZone : Move {
		public PlaceSafeZone(Coordinate pos, Player player) : base(pos, player){}
	}

}


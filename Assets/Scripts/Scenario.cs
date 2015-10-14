using UnityEngine;
using System.Collections.Generic;

public class Scenario
{
	public Dictionary<Coordinate, GamePiece> pieces = new Dictionary<Coordinate, GamePiece> ();

	public Scenario(){
		pieces.Add (Direction.LEFT.toCoordinate(), new Turret(Direction.LEFT));
		pieces.Add (Direction.RIGHT.toCoordinate(), new Turret(Direction.RIGHT));

		Surround (Direction.LEFT);
		Surround (Direction.RIGHT);
	}

	private void Surround(Coordinate pos){
		foreach (Direction c in Direction.ALL) {
			var target = pos + c;
			if(!pieces.ContainsKey(target))
				pieces.Add (target, SafeZone.INSTANCE);
		}
	}

	public interface GamePiece{};

	public class Turret : GamePiece{
		public Direction facing;

		public Turret(Direction facing){
			this.facing = facing;
		}
	}

	public class SafeZone : GamePiece{
		public static SafeZone INSTANCE = new SafeZone();
	}

	public class Mirror : GamePiece {
		public bool flipped;

		public Mirror(bool flipped){
			this.flipped = flipped;
		}
	}
}


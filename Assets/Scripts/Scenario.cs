using UnityEngine;
using System.Collections.Generic;

public class Scenario
{
	public Dictionary<Coordinate, GamePiece> pieces = new Dictionary<Coordinate, GamePiece> ();

	public Scenario(){
		pieces.Add (Coordinate.LEFT, new Turret(Coordinate.LEFT));
		pieces.Add (Coordinate.RIGHT, new Turret(Coordinate.RIGHT));

		Surround (Coordinate.LEFT);
		Surround (Coordinate.RIGHT);
	}

	private void Surround(Coordinate pos){
		foreach (Coordinate c in Coordinate.DIRECTIONS) {
			var target = pos + c;
			if(!pieces.ContainsKey(target))
				pieces.Add (target, SafeZone.INSTANCE);
		}
	}

	public interface GamePiece{};

	public class Turret : GamePiece{
		public Coordinate facing;

		public Turret(Coordinate facing){
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


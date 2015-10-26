using UnityEngine;
using System.Collections;

public abstract class Piece {

	public enum PieceType {
		Mirror, Turret, SafeZone
	}

	public abstract PieceType GetPieceType();

	public class Turret : Piece {
		public Direction facing;
		public int playerNum = -1;
		
		public Turret(Direction facing) : this(facing, -1){
			
		}
		
		public Turret(Direction facing, int playerNum){
			this.facing = facing;
			this.playerNum = playerNum;
		}
		public override PieceType GetPieceType() {
			return PieceType.Turret;
		}
	}
	
	public class SafeZone : Piece {
		
		public static SafeZone INSTANCE = new SafeZone();
		
		public override PieceType GetPieceType() {
			return PieceType.SafeZone;
		}
	}
	
	public class Mirror : Piece {
		public bool flipped;
		
		public Mirror(bool flipped){
			this.flipped = flipped;
		}
		
		public override PieceType GetPieceType() {
			return PieceType.Mirror;
		}
	}
};


using UnityEngine;
using System.Collections;

public class Direction : Coordinate {

	private static Coordinate LEFT_COORD = new Coordinate (-1, 0);

	public static Direction LEFT = new Direction (-1, 0);
	public static Direction TOP = new Direction (0, 1);
	public static Direction DOWN = new Direction (0, -1);
	public static Direction RIGHT = new Direction (1, 0);
	
	public static Direction TOP_RIGHT = TOP + RIGHT;
	public static Direction DOWN_RIGHT = DOWN + RIGHT;
	public static Direction TOP_LEFT = TOP + LEFT;
	public static Direction DOWN_LEFT = DOWN + LEFT;
	
	public static Direction[] ALL = new Direction[8];
	
	static Direction(){
		ALL[0] = TOP;
		ALL [1] = TOP_RIGHT;
		ALL [2] = RIGHT;
		ALL [3] = DOWN_RIGHT;
		ALL [4] = DOWN;
		ALL [5] = DOWN_LEFT;
		ALL [6] = LEFT;
		ALL [7] = TOP_LEFT;
	}

	public float angle = 0.0f;

	public Direction(int x, int y) : base(x,y) {
		calculateAngle (LEFT_COORD);
	}

	private void calculateAngle(Coordinate other){
		float a = Vector2.Angle (this.asVec2(), other.asVec2());
		if (a < 180) {
			a = this.NonZero() * other.NonZero() * a;
		}
		this.angle = a;
	}

	public static Direction operator +(Direction c1, Direction c2){
		return new Direction (c1.x + c2.x, c1.y + c2.y);
	}
	
	public static Direction operator -(Direction c1, Direction c2){
		return new Direction (c1.x - c2.x, c1.y - c2.y);
	}
	
	public static Direction operator *(Direction c1, int multiplier){
		return new Direction (c1.x * multiplier, c1.y * multiplier);
	}

	public Direction mirror(bool flipped){
		int flip = flipped ? 1 : -1;
		return new Direction(flip * y,  flip * x);
	}
}

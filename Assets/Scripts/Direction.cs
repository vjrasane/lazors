using UnityEngine;
using System.Collections.Generic;

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

	public static List<Direction> HORIZONTAL = new List<Direction> ();
	public static List<Direction> VERTICAL = new List<Direction> ();
	public static List<Direction> CROSS = new List<Direction> ();
	public static List<Direction> DIAGONAL = new List<Direction> ();
	public static List<Direction> ALL = new List<Direction>();
	
	static Direction(){
		HORIZONTAL.Add (RIGHT);
		HORIZONTAL.Add (LEFT);

		VERTICAL.Add (TOP);
		VERTICAL.Add (DOWN);

		CROSS.AddRange (HORIZONTAL);
		CROSS.AddRange (VERTICAL);

		DIAGONAL.Add (TOP_RIGHT);
		DIAGONAL.Add (DOWN_RIGHT);
		DIAGONAL.Add (DOWN_LEFT);
		DIAGONAL.Add (TOP_LEFT);

		ALL.AddRange (CROSS);
		ALL.AddRange (DIAGONAL);
	}

	public float angle = 0.0f;

	public Direction(int x, int y) : base(x,y) {
		calculateAngle (LEFT_COORD);
	}

	public Coordinate toCoordinate(){
		return this.copy ();
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

	public Direction Mirror(bool flipped){
		int flip = flipped ? 1 : -1;
		int newY = flip * this.x;
		int newX = flip * this.y;
		return ALL.Find(d => d.x == newX && d.y == newY);
	}

	public Direction Reverse(){
		return ALL.Find(d => d.x == -this.x && d.y == -this.y);
	}
}

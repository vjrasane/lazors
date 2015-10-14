using UnityEngine;
using System.Collections;

public class Coordinate : object {

	public static Coordinate ZERO = new Coordinate (0, 0);

	public static Coordinate TOP = new Coordinate (0, 1);
	public static Coordinate DOWN = new Coordinate (0, -1);
	public static Coordinate RIGHT = new Coordinate (1, 0);
	public static Coordinate LEFT = new Coordinate (-1, 0);

	public static Coordinate TOP_RIGHT = TOP + RIGHT;
	public static Coordinate DOWN_RIGHT = DOWN + RIGHT;
	public static Coordinate TOP_LEFT = TOP + LEFT;
	public static Coordinate DOWN_LEFT = DOWN + LEFT;

	public static Coordinate[] DIRECTIONS = new Coordinate[8];

	static Coordinate(){
		DIRECTIONS[0] = TOP;
		DIRECTIONS [1] = TOP_RIGHT;
		DIRECTIONS [2] = RIGHT;
		DIRECTIONS [3] = DOWN_RIGHT;
		DIRECTIONS [4] = DOWN;
		DIRECTIONS [5] = DOWN_LEFT;
		DIRECTIONS [6] = LEFT;
		DIRECTIONS [7] = TOP_LEFT;
	}

	public int x, y;

	public Coordinate(int x, int y){
		this.x = x;
		this.y = y;
	}

	public Coordinate(Vector2 vec){
		this.x = (int)vec.x;
		this.y = (int)vec.y;
	}

	public Vector2 asVec2(){
		return new Vector2(x,y);
	}

	public Vector3 asVec3(){
		return new Vector3(x,y,0);
	}

	public static Coordinate operator +(Coordinate c1, Coordinate c2){
		return new Coordinate (c1.x + c2.x, c1.y + c2.y);
	}

	public static Coordinate operator -(Coordinate c1, Coordinate c2){
		return new Coordinate (c1.x - c2.x, c1.y - c2.y);
	}

	public static Coordinate operator *(Coordinate c1, int multiplier){
		return new Coordinate (c1.x * multiplier, c1.y * multiplier);
	}

	public static Coordinate operator *(int multiplier, Coordinate c1){
		return c1 * multiplier;
	}

	public float angle(Coordinate other){
//		if (this.x == -other.x || this.y == other.y)
//			return 180;
//		if (this.x != 0)
//			return 90 * -other.y;
//		if (this.y != 0)
//			return 90 * -other.x;
		//var cross = Vector2.
		float angle = Vector2.Angle (this.asVec2(), other.asVec2());
		if (angle < 180) {
			angle = this.NonZero() * other.NonZero() * angle;
		}

		return angle;
		//return Mathf.Rad2Deg * Mathf.Acos (this.x * other.x + this.y * other.y);
	}

	public Coordinate mirror(bool flipped){
		int flip = flipped ? 1 : -1;
		return new Coordinate(flip * y,  flip * x);
	}

	public override bool Equals(object obj) 
	{
		if (obj == null || GetType() != obj.GetType()) 
			return false;
		
		Coordinate c = (Coordinate)obj;
		return (x == c.x) && (y == c.y);
	}

	public override int GetHashCode() 
	{
		return x ^ y;
	}

	public int NonZero(){
		if (x != 0)
			return x;
		else 
			return y;
	}

}


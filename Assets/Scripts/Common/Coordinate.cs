using UnityEngine;
using System.Collections;

public class Coordinate : object {

	public static Coordinate ZERO = new Coordinate (0, 0);

	public int x, y;

	public Coordinate(int x, int y){
		this.x = x;
		this.y = y;
	}

	public Coordinate(Vector2 vec){
		this.x = (int)vec.x;
		this.y = (int)vec.y;
	}

	public Coordinate copy(){
		return new Coordinate (x, y);
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

	public float distance(Coordinate other){
		return Mathf.Sqrt(Mathf.Pow(this.x - other.x, 2) + Mathf.Pow(this.y - other.y, 2));
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

	public override string ToString() 
	{
		return "{" + x + "," + y + "}";
	}

}


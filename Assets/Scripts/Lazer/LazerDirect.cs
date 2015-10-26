using UnityEngine;
using System.Collections;

public abstract class LazerDirect : Lazer {

	protected Direction facing = Direction.LEFT;

	protected float length;

	public Direction GetFacing(){
		return facing;
	}

	public abstract void SetLength(float length);

	public abstract void Rotate (Direction facing);

	public float GetLength(){
		return length;
	}
}

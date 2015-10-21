using UnityEngine;
using System.Collections;

public abstract class LazerDirect : Lazer {

	public Direction facing = Direction.LEFT;

	protected float length;

	public abstract void SetLength(float length);

	public void Rotate(Direction facing){
		this.transform.rotation = Quaternion.identity;
		this.transform.Rotate (new Vector3(0,0,facing.angle));
		this.facing = facing;
	}

	public float GetLength(){
		return length;
	}
}

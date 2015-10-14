using UnityEngine;
using System.Collections;

public class Lazer : Positional {

	public enum State { Straight, Turn, FlippedTurn, Impact }

	private GameObject sprite;
	private SpriteRenderer spriteRenderer;

	public Direction facing = Direction.LEFT;

	public static float length = 0.0f;

	private float currentLength;

	public float maxLength;

	// Use this for initialization
	void Awake () {
		this.sprite = this.transform.FindChild("sprite").gameObject;
		this.spriteRenderer = sprite.GetComponent<SpriteRenderer> ();
		if (length == 0.0f) {
			length = this.spriteRenderer.bounds.size.x;
		}
	}

	public void ClearChildren(){
		foreach (Transform child in transform)
			if (!child.tag.Equals ("Sprite"))
				Destroy (child.gameObject);
	}

	public void ResetSpritePosition(){
		sprite.transform.localPosition = Vector2.zero;
		sprite.transform.Translate (new Vector3 (Lazer.length / 2,0,0));
	}

	public void SetLength (float percentage){
		this.currentLength = percentage;
		this.sprite.transform.localScale = new Vector2(percentage, 1);
	}

	public float GetCurrentLength(){
		return currentLength;
	}

	public void Rotate(Direction facing){
		this.transform.Rotate (new Vector3(0,0,facing.angle));
		this.facing = facing;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetLayerOrder (State lazerState, bool incoming)
	{
		SendTo (IsFront(this.facing, lazerState, incoming));
	}

	public static bool IsFront (Coordinate direction, State lazerState, bool incoming)
	{
		if (lazerState == State.Impact)
			return true;
		else {
			bool front = false;
			
			if(direction.y != 0){
				front = direction.y > 0;
			} else {
				front = direction.x > 0;
				front = lazerState.Equals(State.FlippedTurn) ? !front : front;
			}
			front = incoming ? front : !front;
			return front;
		}
	}

	void SendTo(bool front){
		if (front)
			BringToFront ();
		else
			SendToBack ();
	}

	void SendToBack ()
	{
		this.spriteRenderer.sortingOrder = -2;
	}

	void BringToFront ()
	{
		this.spriteRenderer.sortingOrder = 1;
	}
}

using UnityEngine;
using System.Collections;

public class LazerHit : LazerDirect {

	private LazerStraight lazerIn;
	private LazerStraight lazerOut;
	private LazerImpact impact;

	private SpriteRenderer beamRenderer;
	private SpriteRenderer hilightRenderer;

	void Awake(){
		lazerIn = this.transform.FindChild("lazer_incoming").GetComponent<LazerStraight>();
		lazerOut = this.transform.FindChild("lazer_outgoing").GetComponent<LazerStraight>();
		impact = this.transform.FindChild("lazer_impact").GetComponent<LazerImpact>();
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetTurn(bool turn){
		this.lazerOut.gameObject.SetActive (turn);
	}

	public override void Rotate(Direction facing){
		this.lazerIn.Rotate (facing);
	}

	public void RotateTurn(Direction facing){
		this.lazerOut.Rotate (facing);
	}

	public void SetLayerOrder (PieceObject hit, int layer)
	{
		SendTo (IsFront(hit), layer);
	}
	
	private bool IsFront (PieceObject hit)
	{
		if (hit == null)
			return false;
		else if(hit.GetPieceType() != Piece.PieceType.Mirror)
			return true;
		else {
			var flipped = hit.GetComponent<Mirror>().IsFlipped();
			
			var front = false;
			var facing = this.lazerIn.GetFacing();

			if(facing.y != 0){
				front = facing.y > 0;
			} else {
				front = facing.x > 0;
				front = flipped ? !front : front;
			}
			return front;
		}
	}

	public override void SetLength (float length){
		this.lazerIn.SetLength (length);
		this.impact.transform.position = this.lazerIn.front.transform.position;
		this.lazerOut.transform.position = this.lazerIn.front.transform.position;
	}

	public override void SetVisibility(float alpha){
		this.lazerIn.SetVisibility (alpha);
		this.lazerOut.SetVisibility (alpha);
		this.impact.SetVisibility (alpha);
	}

	public override void BringToFront (int layer)
	{
		this.lazerIn.BringToFront (layer);
		this.lazerOut.BringToFront (layer);
		this.impact.BringToFront (layer);
	}
	
	public override void SendToBack (int layer)
	{
		this.lazerIn.SendToBack (layer);
		this.lazerOut.SendToBack (layer);
		this.impact.SendToBack (layer);
	}

	public override void SetLayer(string name){
		this.lazerIn.SetLayer (name);
		this.lazerOut.SetLayer (name);
		this.impact.SetLayer (name);
	}

	public override void SetRenderColor(Color color){
		this.lazerIn.SetColor(color);
		this.lazerOut.SetColor(color);
		this.impact.SetColor(color);
	}
}

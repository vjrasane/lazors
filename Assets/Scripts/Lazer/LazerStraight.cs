using UnityEngine;
using System.Collections;

public class LazerStraight : LazerDirect {

	protected GameObject spriteInc;
	public GameObject front;

	private SpriteRenderer beamRenderer;
	private SpriteRenderer hilightRenderer;

	// Use this for initialization
	void Awake () {
		this.spriteInc = this.transform.FindChild("sprite").gameObject;
		this.front = spriteInc.transform.FindChild ("front").gameObject;
		this.beamRenderer = spriteInc.GetComponent<SpriteRenderer> ();
		this.hilightRenderer = spriteInc.transform.FindChild("hilight").GetComponent<SpriteRenderer> ();
	}

	public override void Rotate(Direction facing){
		var angle = this.facing.angle (facing);
		this.transform.Rotate (new Vector3(0,0,angle));
		this.facing = facing;
	}

	public override void SetVisibility(float alpha){
		var beam = this.beamRenderer.color;
		beam.a = alpha;
		this.beamRenderer.color = beam;

		var hilight = this.hilightRenderer.color;
		hilight.a = alpha;
		this.hilightRenderer.color = hilight;
	}

	public override void SetLength (float length){
		this.length = length;
		this.spriteInc.transform.localScale = new Vector2(length, 1);
	}

	public override void BringToFront (int layer)
	{
		this.beamRenderer.sortingOrder = layer * 4 - 3;
		this.hilightRenderer.sortingOrder = layer * 4 - 2;
	}

	public override void SendToBack (int layer)
	{
		this.beamRenderer.sortingOrder = -layer * 4;
		this.hilightRenderer.sortingOrder = 1 - layer * 4;
	}

	public override void SetLayer(string name){
		this.beamRenderer.sortingLayerName = name;
		this.hilightRenderer.sortingLayerName = name;
	}

	public override void SetRenderColor(Color color){
		this.hilightRenderer.color = color;
	}
}

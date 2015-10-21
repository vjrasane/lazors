using UnityEngine;
using System.Collections;

public class LazerImpact : Lazer {

	private SpriteRenderer circleRenderer;
	private SpriteRenderer hilightRenderer;

	void Awake(){
		this.circleRenderer = this.GetComponent<SpriteRenderer> ();
		this.hilightRenderer = this.transform.FindChild ("hilight").GetComponent<SpriteRenderer> ();
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override void SetVisibility(float alpha){
		var circle = this.circleRenderer.color;
		circle.a = alpha;
		this.circleRenderer.color = circle;
		
		var hilight = this.hilightRenderer.color;
		hilight.a = alpha;
		this.hilightRenderer.color = hilight;
	}

	public override void BringToFront (int layer)
	{
		this.circleRenderer.sortingOrder = layer * 4 - 1;
		this.hilightRenderer.sortingOrder = layer * 4;
	}
	
	public override void SendToBack (int layer)
	{
		this.circleRenderer.sortingOrder = 2 - layer * 4;
		this.hilightRenderer.sortingOrder = 3 - layer * 4;
	}
	
	public override void SetLayer(string name){
		this.circleRenderer.sortingLayerName = name;
		this.hilightRenderer.sortingLayerName = name;
	}

	public override void SetRenderColor(Color color){
		this.hilightRenderer.color = color;
	}
}

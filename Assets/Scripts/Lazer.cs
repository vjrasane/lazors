﻿using UnityEngine;
using System.Collections;

public class Lazer : Positional {

	private GameObject impact;
	public GameObject lazerImpactPrefab;

	private GameObject sprite;
	public GameObject front;

	private Color lazerColor;

	private SpriteRenderer beamRenderer;
	private SpriteRenderer hilightRenderer;

	public Direction facing = Direction.LEFT;

	public static float length = 0.0f;

	private float currentLength;

	public float maxLength;

	public int layerOrderMultiplier = 1;

	public Positional hit;

	// Use this for initialization
	void Awake () {
		this.front = this.transform.FindChild ("front").gameObject;
		this.sprite = this.transform.FindChild("sprite").gameObject;
		this.beamRenderer = sprite.GetComponent<SpriteRenderer> ();
		this.hilightRenderer = sprite.transform.FindChild("hilight").GetComponent<SpriteRenderer> ();

		this.impact = this.transform.FindChild ("lazer_impact").gameObject;
		DisableImpact ();

		SetColor(lazerColor);

		if (length == 0.0f) {
			length = this.beamRenderer.bounds.size.x;
		}
	}

	public void SetColor(Color color){
		this.lazerColor = color;
		this.hilightRenderer.color = color;
	}

	public void SetVisibility(float alpha){
		var beam = this.beamRenderer.color;
		beam.a = alpha;
		this.beamRenderer.color = beam;

		var hilight = this.hilightRenderer.color;
		hilight.a = alpha;
		this.hilightRenderer.color = hilight;
	}

	public void DisableImpact(){
		if(impact != null)
			this.impact.SetActive (false);
	}

	public void AddImpact(Positional hit, Vector2 position, bool preview){
		AddImpact (hit, position);

		var hilightRenderer = this.impact.transform.FindChild ("hilight").GetComponent<SpriteRenderer> ();
		var hilight = hilightRenderer.color;
		var circleRenderer = this.impact.GetComponent<SpriteRenderer> ();
		var circle = circleRenderer.color;

		if (preview) {
			hilight.a = Constants.LAZER_PREVIEW_ALPHA;
			circle.a = Constants.LAZER_PREVIEW_ALPHA;
		} else {
			hilight.a = 1;
			circle.a = 1;
		}

		hilightRenderer.color = hilight;
		circleRenderer.color = circle;
	}

	public GameObject AddImpact(){
		if (this.impact == null) {
			impact = Instantiate(lazerImpactPrefab);
			impact.transform.parent = this.transform;
		}
		
		var hilightRenderer = this.impact.transform.FindChild ("hilight").GetComponent<SpriteRenderer>();
		hilightRenderer.color = lazerColor;
		
		this.impact.transform.position = hit.transform.position;
		
		this.impact.SetActive (true);
		
		SetImpactLayerOrder (hit);
		
		return this.impact;
	}

	private GameObject AddImpact(Positional hit, Vector2 position){
		if (this.impact == null) {
			impact = Instantiate(lazerImpactPrefab);
			impact.transform.parent = this.transform;
		}

		var hilightRenderer = this.impact.transform.FindChild ("hilight").GetComponent<SpriteRenderer>();
		hilightRenderer.color = lazerColor;

		this.impact.transform.position = position;

		this.impact.SetActive (true);
		
		SetImpactLayerOrder (hit);

		return this.impact;
	}

	public void AddImpact(Positional hit, bool preview){
		AddImpact (hit, this.transform.position, preview);
	}

	public void DisableOffset(){
		sprite.transform.localPosition = Vector2.zero;
		sprite.transform.Translate (new Vector3 (Lazer.length / 2,0,0));
	}

	public void EnableOffset(){
		sprite.transform.localPosition = Vector2.zero;
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

	public void SetLayerOrder (Positional hit, bool incoming)
	{
		SendTo (IsFront(this.facing, hit, incoming));
	}

	public static bool IsFront (Coordinate direction, Positional hit, bool incoming)
	{
		if (!hit.tag.Equals(Constants.MIRROR_TAG) )
			return true;
		else {
			var flipped = hit.GetComponent<Mirror>().IsFlipped();

			var front = false;
			
			if(direction.y != 0){
				front = direction.y > 0;
			} else {
				front = direction.x > 0;
				front = flipped ? !front : front;
			}
			front = incoming ? front : !front;
			return front;
		}
	}

	void SetImpactLayerOrder (Positional hit)
	{
		// Lazer is INCOMING here
		var circleRenderer = this.impact.GetComponent<SpriteRenderer> ();
		var hilightRenderer = this.impact.transform.FindChild("hilight").GetComponent<SpriteRenderer> ();
		
		var front = IsFront (facing, hit, true);
		circleRenderer.sortingOrder = front ? layerOrderMultiplier * 4 - 1 : 2 - layerOrderMultiplier * 4;
		hilightRenderer.sortingOrder = front ? layerOrderMultiplier * 4 : 3 - layerOrderMultiplier * 4;
	}

	void SendTo(bool front){
		if (front)
			BringToFront ();
		else
			SendToBack ();
	}

	public void SendToBack ()
	{
		this.beamRenderer.sortingOrder = -layerOrderMultiplier * 4;
		this.hilightRenderer.sortingOrder = 1 - layerOrderMultiplier * 4;
	}

	public void BringToFront ()
	{
		this.beamRenderer.sortingOrder = layerOrderMultiplier * 4 - 3;
		this.hilightRenderer.sortingOrder = layerOrderMultiplier * 4 - 2;
	}

	public void ResetLayer(){
		SetLayer ("LazerLevel");
	}

	public void SetLayer(string name){
		this.beamRenderer.sortingLayerName = name;
		this.hilightRenderer.sortingLayerName = name;
	}
}

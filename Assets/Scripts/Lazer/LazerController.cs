using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class LazerController : MonoBehaviour {

	public Turret turret;

	public GameObject lazerPrefab;
	public GameObject lazerHitPrefab;

	private List<LazerDirect> sections = new List<LazerDirect>();
	private List<LazerDirect> previewSections = new List<LazerDirect>();

	private static List<LazerStraight> LAZER_POOL_STRAIGHT = new List<LazerStraight> ();
	private static List<LazerHit> LAZER_POOL_HIT = new List<LazerHit> ();

	public Color lazerColor;
	private int layer;

	private Color previewColor;

	void Awake(){
		var lazerSprite = lazerPrefab.transform.FindChild ("sprite");
		lazerSprite.localPosition = Vector2.zero;

		var renderer = lazerSprite.GetComponent<SpriteRenderer> ();
		Lazer.LENGTH = renderer.bounds.size.x;
		lazerSprite.transform.Translate (new Vector3 (Lazer.LENGTH / 2,0,0));

		this.previewColor = new Color(lazerColor.r, lazerColor.g, lazerColor.b);
		this.previewColor.a = Constants.LAZER_PREVIEW_ALPHA;
	}

	void FixedUpdate () {

	}

	public void CeaseFire(){
		ClearLazer (sections);
		ClearLazer (previewSections);
	}

	private void ClearLazer(List<LazerDirect> container){
		container.ForEach (l => MoveToPool(l));
		container.Clear ();
	}

	private void MoveToPool(LazerDirect lazer){
		lazer.ResetLayer ();
		lazer.gameObject.SetActive (false);

		if (lazer is LazerStraight)
			LAZER_POOL_STRAIGHT.Add((LazerStraight)lazer);
		else if (lazer is LazerHit)
			LAZER_POOL_HIT.Add ((LazerHit)lazer);
	}
	
	private L GetFromPool<L>(List<L> pool) where L : LazerDirect {
		var lazer = pool[0];
		lazer.gameObject.SetActive(true);
		pool.RemoveAt (0);
		return lazer;
	}

	private L GetInstance<L>(GameObject prefab, List<L> pool) where L : LazerDirect
	{
		if (pool.Count <= 0)
			return Instantiate (lazerPrefab).GetComponent<L>();
		else {
			return GetFromPool (pool);
		}
	}

	bool ObjectExists(LazerStraight lazer){
		return ObjectExists (lazer.position, lazer.facing);
	}
	
	bool ObjectExists (Coordinate pos, Direction dir)
	{
		List<Coordinate> coordinates = new List<Coordinate>(turret.grid.objects.Keys);
		Coordinate trans = turret.TranslateCoordinate (pos);
		if (dir.y != 0)
			return coordinates.Exists (c => trans.x == c.x && dir.y * c.y > dir.y * trans.y);
		else if (dir.x != 0)
			return coordinates.Exists (c => trans.y == c.y && dir.x * c.x > dir.x * trans.x);
		
		return false;
	}

	public void ClearPreview(){
		ClearLazer (previewSections);
	}
	
	public bool firing = false;

	public void Fire(){
		if(this.turret.IsActive())
			DrawLazer (this.turret.position, this.turret.GetFacing(), 1, OnHit, sections);
	}

	public void Preview(){
		var previewPiece = turret.grid.previewPiece;
		var range = 0;
		var facing = this.turret.GetFacing();
		var position = this.turret.position;

		while (range < Grid.LAZER_MAX_RANGE) {
			position += facing;

			Positional hit;
			while (range < Grid.LAZER_MAX_RANGE && (hit = FindHit(position)) == null) {
				if(!previewPiece.tag.Equals("SafeZone") && position.Equals(previewPiece.position)){
					DrawPreview (position, facing);
					return;
				}
				range++;
				position += facing;
			}

			if (hit != null) {
				switch (hit.tag){
				case "Mirror":
					var mirror = hit.GetComponent<Mirror>();
					facing = facing.Mirror(mirror.IsFlipped());
					break;
				case "Turret":
					return;
				}

				range = 0;
			}
		}
	}

	private void DrawPreview (Coordinate position, Direction facing)
	{
		var previewPiece = turret.grid.previewPiece;
		DrawLazer (position, facing.Mirror (previewPiece.GetComponent<Mirror> ().IsFlipped ()), Constants.LAZER_PREVIEW_ALPHA, p =>  {}, previewSections);
	}

	private void DrawLazer (Coordinate pos, Direction dir, float alpha, Action<Positional> onHit, List<LazerDirect> container)
	{
		var facing = dir;
		var position = pos;

		ClearLazer (container);
		StartLazer (position, facing, alpha, container);

		var range = 0;
		var rayStart = pos;

		while (range < Grid.LAZER_MAX_RANGE) {
			position += facing;
			var start = position;

			Positional hit;
			while(range < Grid.LAZER_MAX_RANGE && (hit = FindHit(position)) == null){
				range++;
				position += facing;
			}

			if(range > 0) {
				InstantiateAt(start, facing, alpha, range, container);
				rayStart = (start + facing * (range - 1));
			}
			
			if(hit != null){
				var lazerHit = InstantiateHitAt(position, facing, alpha, hit, container);
	
				switch (hit.tag){
				case "Mirror":
					var mirror = hit.GetComponent<Mirror>();
					facing = facing.Mirror(mirror.IsFlipped());
					lazerHit.RotateTurn(facing);
					lazerHit.SetTurn (true);
					rayStart = position;
					break;
				default:
					RaycastHit2D raycast = Physics2D.Raycast(rayStart.asVec2() * Lazer.LENGTH, facing.asVec2(), Mathf.Infinity, 1 << LayerMask.NameToLayer("Obstacle"));
					var distance = (raycast.distance / Lazer.LENGTH) - 0.5f;
					lazerHit.SetLength(distance);
					lazerHit.SetTurn(false);
					onHit(hit);
					return;
				}

				range = 0;
			}
		}
	}

	Positional FindHit (Coordinate position)
	{
		Positional hit;
		if (turret.grid.objects.ContainsKey (position) && !(hit = turret.grid.objects [position]).tag.Equals ("SafeZone"))
			return hit;
		return  null;
	}

	void OnHit (Positional hit)
	{
		if(hit.tag.Equals("Turret")) {
		   hit.GetComponent<Turret> ().Explode();
		}
	}

	private LazerStraight InstantiateAt(Coordinate pos, Direction facing, float alpha, int length, List<LazerDirect> container){
		LazerStraight lazer = Instantiate (lazerPrefab).GetComponent<LazerStraight> ();
		
		InitLazer (lazer, pos, facing, alpha, length, container);

		return lazer;
	}

	private LazerHit InstantiateHitAt(Coordinate pos, Direction facing, float alpha, List<LazerDirect> container){
		return InstantiateHitAt (pos, facing, alpha, null, container);
	}

	private LazerHit InstantiateHitAt(Coordinate pos, Direction facing, float alpha, Positional hit, List<LazerDirect> container){
		LazerHit lazer = Instantiate (lazerHitPrefab).GetComponent<LazerHit> ();

		InitLazer (lazer, pos, facing, alpha, 0.5f, container);

		lazer.SetLayerOrder (hit, turret.player.number);

		return lazer;
	}

	private void InitLazer(LazerDirect lazer, Coordinate pos, Direction facing, float alpha, float length, List<LazerDirect> container){
		Coordinate translated = turret.ReverseTranslate (pos);
		
		lazer.transform.parent = this.transform;
		lazer.transform.localPosition = Vector2.zero;
		lazer.transform.Translate(translated.asVec3() * Lazer.LENGTH);

		lazer.SetColor (lazerColor);
		lazer.Rotate (facing);
		lazer.SetLength (length);
		lazer.SetVisibility (alpha);

		lazer.position = pos;

		container.Add (lazer);
	}

	private LazerHit StartLazer (Coordinate position, Direction direction, float alpha, List<LazerDirect> container)
	{
		var lazer = InstantiateHitAt (position, direction.Reverse(), alpha, container);
		lazer.SetTurn (false);
		lazer.SetLayer ("AccessoryBelow");
		lazer.SendToBack (turret.player.number);
		return lazer;
	}
}


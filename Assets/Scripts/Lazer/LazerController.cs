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

	private static Queue<LazerStraight> LAZER_POOL_STRAIGHT = new Queue<LazerStraight> ();
	private static Queue<LazerHit> LAZER_POOL_HIT = new Queue<LazerHit> ();

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
		if (container.Count > 0) {
			container.ForEach (l => MoveToPool (l));
			container.Clear ();
		}
	}

	private void MoveToPool(LazerDirect lazer){
		lazer.ResetLayer ();
		lazer.gameObject.SetActive (false);

		if (lazer is LazerStraight)
			LAZER_POOL_STRAIGHT.Enqueue((LazerStraight)lazer);
		else if (lazer is LazerHit)
			LAZER_POOL_HIT.Enqueue ((LazerHit)lazer);
	}
	
	private L GetFromPool<L>(Queue<L> pool) where L : LazerDirect {
		var lazer = pool.Dequeue ();
		lazer.gameObject.SetActive(true);
		return lazer;
	}

	private L GetInstance<L>(GameObject prefab, Queue<L> pool) where L : LazerDirect
	{
		if (pool.Count <= 0)
			return Instantiate (prefab).GetComponent<L>();
		else {
			return GetFromPool (pool);
		}
	}

	bool ObjectExists(LazerStraight lazer){
		return ObjectExists (lazer.position, lazer.GetFacing());
	}
	
	bool ObjectExists (Coordinate pos, Direction dir)
	{
		List<Coordinate> coordinates = new List<Coordinate>(turret.grid.objects.Keys);
		//Coordinate trans = turret.TranslateCoordinate (pos);
		if (dir.y != 0)
			return coordinates.Exists (c => pos.x == c.x && dir.y * c.y > dir.y * pos.y);
		else if (dir.x != 0)
			return coordinates.Exists (c => pos.y == c.y && dir.x * c.x > dir.x * pos.x);
		
		return false;
	}

	public void ClearPreview(){
		ClearLazer (previewSections);
	}
	
	public bool firing = false;

	public void Fire(){
		if(this.turret.IsActive())
			DrawLazer (this.turret.position, this.turret.GetFacing(), 1, DestroyTurret, FindRealHit, sections);
	}

	public void Preview(){

		var range = 0;
		var facing = this.turret.GetFacing();
		var position = this.turret.position;

		while (range < Grid.LAZER_MAX_RANGE || ObjectExists (position, facing)) {
			position += facing;

			Positional hit, preview;
			while((preview = FindGhostHit(position)) == null 
			      && (hit = FindRealHit(position)) == null 
			      && (range < Grid.LAZER_MAX_RANGE || ObjectExists (position, facing))){
				range++;
				position += facing;
			}

			if(preview != null) {
				DrawPreview (position, facing);
				return;
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

	Piece FindGhostHit (Coordinate position)
	{
		var previewPiece = turret.grid.previewPiece;
		if(!previewPiece.tag.Equals("SafeZone") && position.Equals(previewPiece.position)){
			return previewPiece;
		}
		return null;
	}

	private void DrawPreview (Coordinate position, Direction facing)
	{
		var previewPiece = turret.grid.previewPiece;
		Func<Coordinate, Piece> hitReg = p => 
		{ 
			Piece hit = null;
			if ((hit = FindGhostHit (p)) == null)
				hit = FindRealHit (p);
			return hit;
		};
		DrawLazer (position, facing.Mirror (previewPiece.GetComponent<Mirror> ().IsFlipped ()), Constants.LAZER_PREVIEW_ALPHA, p =>  {}, hitReg,previewSections);
	}

	private void DrawLazer (Coordinate pos, Direction dir, float alpha, Action<Positional> onHit, Func<Coordinate, Piece> hitReg, List<LazerDirect> container)
	{
		var facing = dir;
		var position = pos;

		ClearLazer (container);
		StartLazer (position, facing, alpha, container);

		var range = 0;
		var rayStart = pos;

		while (range < Grid.LAZER_MAX_RANGE || ObjectExists (position, facing)) {
			position += facing;
			var start = position;

			Positional hit;
			while((hit = hitReg(position)) == null 
			      && (range < Grid.LAZER_MAX_RANGE || ObjectExists (position, facing))){
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

	Piece FindRealHit (Coordinate position)
	{
		Piece hit;
		if (turret.grid.objects.ContainsKey (position) && !(hit = turret.grid.objects [position]).tag.Equals ("SafeZone"))
			return hit;
		return  null;
	}

	void DestroyTurret (Positional hit)
	{
		if(hit.tag.Equals("Turret")) {
		   hit.GetComponent<Turret> ().Explode();
		}
	}

	private LazerStraight InstantiateAt(Coordinate pos, Direction facing, float alpha, int length, List<LazerDirect> container){
		LazerStraight lazer = GetInstance<LazerStraight>(lazerPrefab, LAZER_POOL_STRAIGHT);
		//LazerStraight lazer = Instantiate(lazerPrefab).GetComponent<LazerStraight>();
		InitLazer (lazer, pos, facing, alpha, length, container);

		lazer.BringToFront (turret.player.number);

		return lazer;
	}

	private LazerHit InstantiateHitAt(Coordinate pos, Direction facing, float alpha, List<LazerDirect> container){
		return InstantiateHitAt (pos, facing, alpha, null, container);
	}

	private LazerHit InstantiateHitAt(Coordinate pos, Direction facing, float alpha, Positional hit, List<LazerDirect> container){
		LazerHit lazer = GetInstance<LazerHit>(lazerHitPrefab, LAZER_POOL_HIT);
		//LazerHit lazer = Instantiate(lazerHitPrefab).GetComponent<LazerHit>();
		InitLazer (lazer, pos, facing, alpha, 0.5f, container);

		lazer.SetLayerOrder (hit, turret.player.number);

		return lazer;
	}

	private void InitLazer(LazerDirect lazer, Coordinate pos, Direction facing, float alpha, float length, List<LazerDirect> container){
		Coordinate translated = turret.ReverseTranslate (pos);
		
		lazer.transform.parent = this.transform;
		lazer.transform.localPosition = Vector2.zero;
		lazer.transform.Translate(translated.asVec3() * Lazer.LENGTH, Space.World);

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



using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public abstract class LazerController : PlayerObject {

	public GameObject lazerPrefab;
	public GameObject lazerHitPrefab;

	private List<LazerDirect> sections = new List<LazerDirect>();
	private List<LazerDirect> previewSections = new List<LazerDirect>();

	private static Queue<LazerStraight> LAZER_POOL_STRAIGHT = new Queue<LazerStraight> ();
	private static Queue<LazerHit> LAZER_POOL_HIT = new Queue<LazerHit> ();

	public Color lazerColor;
	private int layer;
	
	protected bool active = true;

	public void Init(){
		var lazerSprite = lazerPrefab.transform.FindChild ("sprite");
		lazerSprite.localPosition = Vector2.zero;

		var renderer = lazerSprite.GetComponent<SpriteRenderer> ();
		Lazer.LENGTH = renderer.bounds.size.x;
		lazerSprite.transform.Translate (new Vector3 (Lazer.LENGTH / 2,0,0));

		this.previewColor = new Color(lazerColor.r, lazerColor.g, lazerColor.b);
		this.previewColor.a = Constants.LAZER_PREVIEW_ALPHA;
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

	private bool ObjectExists(LazerStraight lazer){
		return ObjectExists (lazer.Position, lazer.GetFacing());
	}
	
	private bool ObjectExists (Coordinate pos, Direction dir)
	{
		List<Coordinate> coordinates = new List<Coordinate>(Singletons.GRID.objects.Keys);
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

	public void FireLazer(Direction direction){
		if (IsActive ())
			DrawLazer (this.Position, direction, 1, DestroyTurret, FindRealHit, sections);
	}

	private bool IsActive ()
	{
		return this.active && !this.destroyed;
	}

	public void FirePreview(Direction direction){
		if (!IsActive())
			return;
		var range = 0;
		var facing = direction;
		var position = this.Position;

		while (range < Grid.LAZER_MAX_RANGE || ObjectExists (position, facing)) {
			position += facing;

			PieceObject hit = null;
			PieceObject preview = null;
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

	private PieceObject FindGhostHit (Coordinate position)
	{
		var previewPiece = Singletons.GRID.previewPiece;
		if(!previewPiece.tag.Equals("SafeZone") && position.Equals(previewPiece.Position)){
			return previewPiece;
		}
		return null;
	}

	private void DrawPreview (Coordinate position, Direction facing)
	{
		var previewPiece = Singletons.GRID.previewPiece;
		Func<Coordinate, PieceObject> hitReg = p => 
		{ 
			PieceObject hit = null;
			if ((hit = FindGhostHit (p)) == null)
				hit = FindRealHit (p);
			return hit;
		};
		DrawLazer (position, facing.Mirror (previewPiece.GetComponent<Mirror> ().IsFlipped ()), Constants.LAZER_PREVIEW_ALPHA, p =>  {}, hitReg,previewSections);
	}

	private void DrawLazer (Coordinate pos, Direction dir, float alpha, Action<PieceObject> onHit, Func<Coordinate, PieceObject> hitReg, List<LazerDirect> container)
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

			PieceObject hit;
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

	private PieceObject FindRealHit (Coordinate position)
	{
		PieceObject hit;
		if (Singletons.GRID.objects.ContainsKey (position) && !(hit = Singletons.GRID.objects [position]).tag.Equals ("SafeZone"))
			return hit;
		return  null;
	}

	private void DestroyTurret (PieceObject hit)
	{
		if(hit.GetPieceType() == Piece.PieceType.Turret) {
		   hit.GetComponent<Turret> ().Explode();
		}
	}

	private LazerStraight InstantiateAt(Coordinate pos, Direction facing, float alpha, int length, List<LazerDirect> container){
		LazerStraight lazer = GetInstance<LazerStraight>(lazerPrefab, LAZER_POOL_STRAIGHT);
		//LazerStraight lazer = Instantiate(lazerPrefab).GetComponent<LazerStraight>();
		InitLazer (lazer, pos, facing, alpha, length, container);

		lazer.BringToFront (this.player.number);

		return lazer;
	}

	private LazerHit InstantiateHitAt(Coordinate pos, Direction facing, float alpha, List<LazerDirect> container){
		return InstantiateHitAt (pos, facing, alpha, null, container);
	}

	private LazerHit InstantiateHitAt(Coordinate pos, Direction facing, float alpha, PieceObject hit, List<LazerDirect> container){
		LazerHit lazer = GetInstance<LazerHit>(lazerHitPrefab, LAZER_POOL_HIT);
		//LazerHit lazer = Instantiate(lazerHitPrefab).GetComponent<LazerHit>();
		InitLazer (lazer, pos, facing, alpha, 0.5f, container);

		lazer.SetLayerOrder (hit, this.player.number);

		return lazer;
	}

	private void InitLazer(LazerDirect lazer, Coordinate pos, Direction facing, float alpha, float length, List<LazerDirect> container){
		Coordinate translated = this.ReverseTranslate (pos);
		
		lazer.transform.parent = this.transform;
		lazer.transform.localPosition = Vector2.zero;
		lazer.transform.Translate(translated.asVec3() * Lazer.LENGTH, Space.World);

		lazer.SetColor (lazerColor);
		lazer.Rotate (facing);
		lazer.SetLength (length);
		lazer.SetVisibility (alpha);

		lazer.Position = pos;

		container.Add (lazer);
	}

	private LazerHit StartLazer (Coordinate position, Direction direction, float alpha, List<LazerDirect> container)
	{
		var lazer = InstantiateHitAt (position, direction.Reverse(), alpha, container);
		lazer.SetTurn (false);
		lazer.SetLayer ("AccessoryBelow");
		lazer.SendToBack (this.player.number);
		return lazer;
	}

	private Coordinate TranslateCoordinate(Coordinate pos){
		return pos + this.Position;
	}
	
	private Coordinate ReverseTranslate(Coordinate pos){
		return pos - this.Position;
	}
}



using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class LazerController : MonoBehaviour {

	public Turret turret;

	public GameObject lazerPrefab;
	private List<Lazer> lazerPath = new List<Lazer>();
	private List<Lazer> previewPath = new List<Lazer>();
	private static List<GameObject> LAZER_POOL = new List<GameObject> ();

	public Color lazerColor;
	private Color previewColor;

	private float impactLazerLength = 0.5f;

	private Lazer currentLazerSection;


	private bool firing = false;

	void Awake(){
		var lazerSprite = lazerPrefab.transform.FindChild ("sprite");
		lazerSprite.localPosition = Vector2.zero;

		var renderer = lazerSprite.GetComponent<SpriteRenderer> ();
		Lazer.length = renderer.bounds.size.x;
		lazerSprite.transform.Translate (new Vector3 (Lazer.length / 2,0,0));

		this.previewColor = new Color(lazerColor.r, lazerColor.g, lazerColor.b);
		this.previewColor.a = Constants.LAZER_PREVIEW_ALPHA;
	}

	void FixedUpdate () {
		HandleFiring ();
	}

	public void Fire(){
		ClearLazer ();
		firing = turret.active;
	}

	public void CeaseFire(){
		ClearLazer ();
		firing = false;
	}

	private void ClearLazer(){
		lazerPath.ForEach (l => MoveToPool(l));
		lazerPath.Clear ();
		lazerHit = null;
		currentLazerSection = null;
		currentRange = 0;
	}

	void EndGame ()
	{
		lazerHit.GetComponent<Turret> ().Explode();
	}

	private void MoveToPool(Lazer lazer){
		lazer.transform.localRotation = Quaternion.identity;
		lazer.transform.localPosition = Vector2.zero;
		
		lazer.ResetLayer ();
		
		lazer.DisableOffset ();
		lazer.DisableImpact ();
		
		lazer.gameObject.SetActive (false);
		
		LAZER_POOL.Add (lazer.gameObject);
	}
	
	private GameObject GetFromPool(){
		var lazer = LAZER_POOL[0].gameObject;
		lazer.SetActive(true);
		LAZER_POOL.RemoveAt (0);
		
		return lazer;
	}

	public void Reroute (Coordinate change, bool flipped)
	{
		if (lazerPath.Count <= 0)
			return;
		
		Coordinate translated = turret.ReverseTranslate (change);
		int index = lazerPath.FindIndex(s => s.position.Equals(translated));
		if (index < 0) {
			var lastLazer = lazerPath[lazerPath.Count - 1];
			if(lazerHit == null && ObjectExists(turret.TranslateCoordinate(lastLazer.position), lastLazer.facing)){
				currentLazerSection = lastLazer;
				lazerPosition = GetNextLazerPosition(currentLazerSection.facing);
				lazerDirection = currentLazerSection.facing;
				currentRange = 0;
				firing = true;
			}
			return;
		}
		
		index++;
		List<Lazer> destroy = lazerPath.GetRange(index, lazerPath.Count - index);
		lazerPath.RemoveRange(index, lazerPath.Count - index);
		destroy.ForEach(s => MoveToPool(s));
		
		if (lazerPath.Count > 0) {
			currentLazerSection = lazerPath [lazerPath.Count - 1];
			currentLazerSection.maxLength = impactLazerLength;
			currentLazerSection.SetLength (impactLazerLength);
			
			lazerPosition = currentLazerSection.position;
			
			lazerDirection = currentLazerSection.facing.mirror (flipped);
			lazerHit = this.turret.grid.objects[change];
			
			currentLazerSection.SetLayerOrder (lazerHit, true);
			currentLazerSection.AddImpact(lazerHit, false);
			
			currentRange = 0;
			firing = true;
		} else {
			Fire ();
		}
	}

	private Lazer StartLazerAt(Coordinate pos, Direction facing, bool preview){
		return StartLazerAt (pos, facing, 1, preview);
	}

	private Lazer StartLazerAt(Coordinate pos, Direction facing, float maxLength, bool preview){
		Lazer lazer;
		if (preview) {
			lazer = StartLazerAt (pos, facing, maxLength, previewPath);
			lazer.SetVisibility (Constants.LAZER_PREVIEW_ALPHA);
			lazer.SetLength (maxLength);
		} else {
			lazer = StartLazerAt (pos, facing, maxLength, this.lazerPath);
			lazer.SetVisibility(1);
		}

		return lazer;
	}

	GameObject GetLazerInstance ()
	{
		if (LAZER_POOL.Count <= 0)
			return Instantiate (lazerPrefab);
		else {
			return GetFromPool ();
		}
	}

	private Lazer StartLazerAt(Coordinate pos, Direction facing, float maxLength, List<Lazer> path){
		GameObject lazer = GetLazerInstance ();
		
		lazer.transform.parent = this.transform;
		lazer.transform.localPosition = Vector2.zero;
		lazer.transform.Translate(pos.asVec3() * Lazer.length);
		
		var lazerScript = lazer.GetComponent<Lazer> ();
		lazerScript.SetColor (lazerColor);
		lazerScript.Rotate (facing);
		lazerScript.SetLength (0);
		lazerScript.maxLength = maxLength;
		lazerScript.position = pos;
		lazerScript.layerOrderMultiplier = turret.playerNumber;
		lazerScript.SendToBack ();
		
		path.Add (lazerScript);
		
		return lazerScript;
	}

	bool ObjectExists(Lazer lazer){
		return ObjectExists (lazer.position, lazer.facing);
	}
	
	bool ObjectExists (Coordinate pos, Direction dir)
	{
		List<Coordinate> coordinates = new List<Coordinate>(turret.grid.objects.Keys);
		if (dir.y != 0)
			return coordinates.Exists (c => pos.x == c.x && dir.y * c.y > dir.y * pos.y);
		else if (dir.x != 0)
			return coordinates.Exists (c => pos.y == c.y && dir.x * c.x > dir.x * pos.x);
		
		return false;
	}
	
	private Coordinate GetNextLazerPosition(Coordinate dir){
		return currentLazerSection.position + dir;
	}

	private Direction lazerDirection;
	private Coordinate lazerPosition;
	

	
	private Positional lazerHit = null;
	
	/*
	 * THE CHURCH OF THE FLYING SPAGHETTI MONSTER
	 * 
	*/
	void HandleFiring ()
	{
		if (firing) {	
			currentLazerSection = FireLazer(currentLazerSection);		
		}
	}

	public void ClearPreview(){
		if (previewPath.Count <= 0)
			return;

		var startPos = previewPath[0].position;
		lazerPath.Find (l => l.position.Equals(startPos)).DisableImpact();

		currentLazerSection = null;
		lazerDirection = null;
		lazerPosition = null;
		currentRange = 0;
		lazerHit = null;

		previewPath.ForEach (p => MoveToPool (p));
		previewPath.Clear ();
	}

	public void Preview(Coordinate position, GameObject obj){
	
	}

	private float currentRange = 0f;
	private float distance = 0f;

	private Lazer FireLazer (Lazer section)
	{
		if (section == null)
			section = InitLazer ();

		distance += Time.deltaTime * Constants.LAZER_SPEED;

		if (section.GetCurrentLength() < section.maxLength) {
			distance = IncreaseLength (section, distance);
		} else if(section.hit != null){
 			section = HandleHit(section);
			distance = IncreaseLength(section, distance);
		}

		if(distance > 0 && currentRange < Grid.LAZER_MAX_RANGE){
			var nextPos = section.position + section.facing;
			var nextDir = section.facing;
			var hit = FindHit(section.position, nextDir);

			var range = CalculateRange(nextPos, hit);
			section = StartLazerAt(nextPos, nextDir, range, false);

			section.hit = hit;

			currentRange += range;
			distance = IncreaseLength(section, distance);
		}

		return section;
	}

	float IncreaseLength (Lazer section, float distance)
	{
		var length = section.GetCurrentLength ();
		var max = section.maxLength;
		var increase = distance + length <= max ? distance : max - length;
		section.SetLength (increase + length);
		return distance - increase;
	}

	Lazer HandleHit (Lazer section)
	{
		section.SetLayerOrder(section.hit, true);
		section.AddImpact ();

		Coordinate nextPos = Coordinate.ZERO;
		Direction nextDir = Direction.LEFT;

		switch (section.hit.tag) {
		case "Mirror":
			bool flipped = section.hit.GetComponent<Mirror>().IsFlipped();
			nextPos = turret.ReverseTranslate(section.hit.position);
			nextDir = section.facing.mirror(flipped);
			break;
		case "Turret":
			EndGame();
			break;
		default:
			nextPos = section.position + section.facing;
			nextDir = section.facing;
			break;
		}
	
		var range = impactLazerLength;
		currentRange += range;

		var next = StartLazerAt(nextPos, nextDir, range, false);
		next.EnableOffset ();
		next.SetLayerOrder (section.hit, false);
		return next;
	}

	Positional FindHit(Coordinate position, Direction dir){
		List<Coordinate> coordinates = new List<Coordinate> (turret.grid.objects.Keys);
		var pos = turret.TranslateCoordinate (position);
		IOrderedEnumerable<Coordinate> coords = null;
		if (dir.x != 0) {
			Predicate<Coordinate> criteria = c => c.y == pos.y && dir.x * c.x > dir.x * pos.x && !turret.grid.objects[c].tag.Equals("SafeZone");
			coords = coordinates.FindAll (criteria).OrderBy (c => dir.x * c.x);
		} else if (dir.y != 0) {
			Predicate<Coordinate> criteria = c => c.x == pos.x && dir.y * c.y > dir.y * pos.y && !turret.grid.objects[c].tag.Equals("SafeZone");
			coords = coordinates.FindAll (criteria).OrderBy (c => dir.y * c.y);
		}
		if (coords != null && coords.Count() > 0)
			return turret.grid.objects [coords.First()];
		else
			return null;
	}

	float CalculateRange (Coordinate pos, Positional hit)
	{
		if(hit == null)
			return Grid.LAZER_MAX_RANGE;
		var range = pos.distance (hit.position) - 1;
		switch (hit.tag) {
		case "Mirror":
			range += impactLazerLength;
			break;
		}
		return range;
	}

	Lazer InitLazer ()
	{
		var lazer = StartLazerAt (Direction.ZERO, turret.GetFacing (), impactLazerLength, false);
		lazer.EnableOffset ();
		lazer.SetLayer ("AccessoryBelow");
		return lazer;
	}
}



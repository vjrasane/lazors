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

		currentLazerSection = null;
		currentRange = 0;
	}

	void EndGame (Positional hit)
	{
		hit.GetComponent<Turret> ().Explode();
	}

	private void MoveToPool(Lazer lazer){
		lazer.transform.localRotation = Quaternion.identity;
		lazer.transform.localPosition = Vector2.zero;

		lazer.hit = null;
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

	public void Reroute (Positional change)
	{
		if (lazerPath.Count <= 0)
			return;
		
		Coordinate translated = turret.ReverseTranslate (change.position);
		int index = lazerPath.FindIndex(s => s.position.Equals(translated));
		if (index < 0) {
			var lastLazer = lazerPath[lazerPath.Count - 1];
			if(lastLazer.hit == null && ObjectExists(turret.TranslateCoordinate(lastLazer.position), lastLazer.facing)){
				currentLazerSection = lastLazer;
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
			var section = lazerPath [lazerPath.Count - 1];
			section.hit = change;
			section.maxLength = impactLazerLength;
			section.SetLength(impactLazerLength);
			section.SetLayerOrder(change, true);
			currentLazerSection = HandleHit(section, false);
		
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
		Coordinate trans = turret.TranslateCoordinate (pos);
		if (dir.y != 0)
			return coordinates.Exists (c => trans.x == c.x && dir.y * c.y > dir.y * trans.y);
		else if (dir.x != 0)
			return coordinates.Exists (c => trans.y == c.y && dir.x * c.x > dir.x * trans.x);
		
		return false;
	}
	
	private Coordinate GetNextLazerPosition(Coordinate dir){
		return currentLazerSection.position + dir;
	}


	void HandleFiring ()
	{
		if (firing) {	
			currentLazerSection = FireLazer (currentLazerSection);		
		}
	}

	public void ClearPreview(){
		if (previewPath.Count <= 0)
			return;

		if (lazerPath.Count > 0) {
			var startPos = previewPath [0].position;
			lazerPath.Find (l => l.position.Equals (startPos)).DisableImpact ();

			currentLazerSection = null;
		}
		currentRange = 0;

		previewPath.ForEach (p => MoveToPool (p));
		previewPath.Clear ();
	}

	public void Preview(Positional change){
		Coordinate translated = turret.ReverseTranslate (change.position);
		var section = lazerPath.Find(l => l.position.Equals(translated));
		if (section != null) {
			previewing = true;
			section.hit = change;
			section = HandleHit (section, previewing);
			while(previewing)
				section = FireLazer(section);
		}
	}

	private int currentRange = 0;
	private float distance = 0f;
	public bool firing = false;
	private bool previewing = false;

	/*
	 * THE CHURCH OF THE FLYING SPAGHETTI MONSTER
	*/
	private Lazer FireLazer (Lazer section)
	{
		if (section == null)
			section = InitLazer (previewing);

		distance += Time.deltaTime * Constants.LAZER_SPEED;

		if (section.GetCurrentLength () < section.maxLength) {
			distance = IncreaseLength (section, distance);
		} else if (section.hit != null) {
			section = HandleHit (section, previewing);
			distance = IncreaseLength (section, distance);
		} else {

			var exists = ObjectExists (section);
			while (section.hit == null && (distance > 0 || previewing || exists) && (currentRange < Grid.LAZER_MAX_RANGE)) {
				var nextPos = section.position + section.facing;
				var nextDir = section.facing;

				var translated = turret.TranslateCoordinate (nextPos);
				Positional hit = null;
				if (turret.grid.objects.ContainsKey (translated) && !turret.grid.objects [translated].tag.Equals ("SafeZone"))
					hit = turret.grid.objects [translated];

				var range = CalculateLength (section.transform.position, section.facing.asVec2 (), hit);
				section = StartLazerAt (nextPos, nextDir, range, previewing);
		
				section.maxLength = range;
				section.hit = hit;
				section.SetLayerOrder (section.hit, true);
				currentRange++;
				distance = IncreaseLength (section, distance);

				exists = ObjectExists (section);
			}
			if (currentRange >= Grid.LAZER_MAX_RANGE && !exists) {
				firing = false;
				previewing = false;
			}

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

	Lazer HandleHit (Lazer section, bool preview)
	{
		Coordinate nextPos = Coordinate.ZERO;
		Direction nextDir = Direction.LEFT;

		switch (section.hit.tag) {
		case "Mirror":
			bool flipped = section.hit.GetComponent<Mirror>().IsFlipped();
			nextPos = turret.ReverseTranslate(section.hit.position);
			nextDir = section.facing.mirror(flipped);
			section.AddImpact (preview, section.hit.transform.position);
			break;
		case "Turret":
			if(!preview) {
				EndGame(section.hit);
				firing = false;
			} else {
				section.AddImpact (preview);
				previewing = false;
			}
			return section;
		default:
			nextPos = section.position + section.facing;
			nextDir = section.facing;
			break;
		}
	
		var range = impactLazerLength;
		currentRange = 0;

		var next = StartLazerAt(nextPos, nextDir, range, preview);
		next.EnableOffset ();
		next.SetLayerOrder (section.hit, false);
		return next;
	}

	float CalculateLength (Vector2 pos, Vector2 dir, Positional hit)
	{
		if(hit == null)
			return 1;
		switch (hit.tag) {
		case "Mirror":
			return impactLazerLength;
		default:
			RaycastHit2D raycast = Physics2D.Raycast(pos, dir, Mathf.Infinity, 1 << LayerMask.NameToLayer("Obstacle"));
			return (raycast.distance / Lazer.length) - impactLazerLength;
		}
	}

	Lazer InitLazer (bool preview)
	{
		var lazer = StartLazerAt (Direction.ZERO, turret.GetFacing (), impactLazerLength, preview);
		lazer.EnableOffset ();
		lazer.SetLayer ("AccessoryBelow");
		return lazer;
	}
}



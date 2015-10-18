using UnityEngine;
using System.Collections.Generic;
using System;

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
	private int currentRange = 0;

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
	
	private float excess = 0f;
	
	private GameObject lazerHit = null;
	
	/*
	 * THE CHURCH OF THE FLYING SPAGHETTI MONSTER
	 * 
	*/
	void HandleFiring ()
	{
		if (firing) {	
			FireLazer(EndGame, false);		
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
		var lazer = lazerPath.Find (l => l.position.Equals(turret.ReverseTranslate(position)));
		if (lazer != null) {
			currentLazerSection = lazer;
			if (obj.tag.Equals (Constants.MIRROR_TAG)) {
				currentLazerSection.AddImpact (obj, true);
				bool flipped = obj.GetComponent<Mirror> ().IsFlipped ();
				lazerDirection = currentLazerSection.facing.mirror (flipped);
				lazerPosition = currentLazerSection.position;

				currentRange = 0;
				lazerHit = obj;
			}

			FireLazer(() => {}, true);
		}
	}

	private void FireLazer (Action onImpact, bool preview)
	{
		InitLazer (preview);

		var previewing = preview;

		var currentLength = currentLazerSection.GetCurrentLength ();
		var maxLength = currentLazerSection.maxLength;
		bool exists = ObjectExists (turret.TranslateCoordinate (lazerPosition), lazerDirection);
		if (currentLength < maxLength) {
			currentLazerSection.SetLength (Mathf.Min (maxLength, currentLength + Constants.LAZER_SPEED + excess));
			excess = currentLength + Constants.LAZER_SPEED - maxLength;
		}
		else {
			do {
				if (lazerHit != null) {
					if (lazerHit.tag.Equals ("Turret")) {
						firing = false;
						previewing = false;
						onImpact();

						// TODO END GAME HERE;
					}
					else {
						currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, impactLazerLength, preview);
						currentLazerSection.EnableOffset ();
						currentLazerSection.SetLayerOrder (lazerHit, false);
						lazerPosition = GetNextLazerPosition (currentLazerSection.facing);
						lazerHit = null;
					}
				}
				else if (exists || currentRange < Grid.LAZER_MAX_RANGE) {
						GameObject obj;
						turret.grid.objects.TryGetValue (turret.TranslateCoordinate (lazerPosition), out obj);
						if (obj != null && !obj.tag.Equals ("SafeZone")) {
							lazerHit = obj;
							if (obj.tag.Equals (Constants.MIRROR_TAG)) {
								currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, impactLazerLength, preview);
								currentLazerSection.AddImpact (lazerHit, preview);
								bool flipped = obj.GetComponent<Mirror> ().IsFlipped ();
								lazerDirection = lazerDirection.mirror (flipped);
								exists = ObjectExists (turret.TranslateCoordinate (lazerPosition), lazerDirection);
							}
							else {
								var raycast = Physics2D.Raycast (currentLazerSection.transform.position, lazerDirection.asVec2 (), Mathf.Infinity, 1 << LayerMask.NameToLayer (Constants.OBSTACLE_LAYER));
								var distance = (raycast.distance - Lazer.length / 2) / Lazer.length;
								currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, distance, preview);
								currentLazerSection.AddImpact (lazerHit, raycast.point, preview);
							}
							currentLazerSection.SetLayerOrder (lazerHit, true);
							currentRange = 0;
						}
						else {
							currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, 1, preview);
							lazerPosition = GetNextLazerPosition (currentLazerSection.facing);
							currentRange++;
						}
					}
					else {
						firing = false;
						previewing = false;
					}

				if(!preview) {
					currentLazerSection.SetLength (Mathf.Min (currentLazerSection.maxLength, excess));
					excess -= currentLazerSection.GetCurrentLength ();

					excess = Mathf.Max (excess, 0);
				}
			}
			while (previewing || (excess > 0 && firing));
		}

	}

	void InitLazer (bool preview)
	{
		if (currentLazerSection == null) {
			if (lazerPath.Count <= 0) {
				currentLazerSection = StartLazerAt (Direction.ZERO, turret.GetFacing (), impactLazerLength, preview);
				currentLazerSection.EnableOffset ();
				currentLazerSection.SetLayer ("AccessoryBelow");
			}
			else {
				currentLazerSection = StartLazerAt (turret.GetFacing (), turret.GetFacing (), preview);
			}
			lazerPosition = GetNextLazerPosition (currentLazerSection.facing);
			lazerDirection = currentLazerSection.facing;
		}
	}
}



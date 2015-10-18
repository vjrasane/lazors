using UnityEngine;
using System.Collections.Generic;
using System;

public class LazerController : MonoBehaviour {

	public Turret turret;

	public GameObject lazerPrefab;
	private List<Lazer> lazerPath = new List<Lazer>();
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
		firing = false;
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
			currentLazerSection.AddImpact(lazerHit, 1);
			
			currentRange = 0;
			firing = true;
		} else {
			Fire ();
		}
	}

	private Lazer StartLazerAt(Coordinate pos, Direction facing){
		return StartLazerAt (pos, facing, 1);
	}

	private Lazer StartLazerAt(Coordinate pos, Direction facing, float maxLength, float alpha){
		var lazer = StartLazerAt (pos, facing, maxLength);
		lazer.SetVisibility (alpha);
		return lazer;
	}

	private Lazer StartLazerAt(Coordinate pos, Direction facing, float maxLength){
		GameObject lazer;
		
		if (LAZER_POOL.Count <= 0)
			lazer = Instantiate (lazerPrefab);
		else {
			lazer = GetFromPool();
		}
		
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
		
		this.lazerPath.Add (lazerScript);
		
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
		return lazerPath[lazerPath.Count - 1].position + dir;
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
			FireLazer (EndGame, Constants.LAZER_PREVIEW_ALPHA);		
		}
	}

	void FireLazer (Action onImpact, float alpha)
	{
		InitLazer (alpha);
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
						onImpact();
						break;
						// TODO END GAME HERE;
					}
					else {
						currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, impactLazerLength, alpha);
						currentLazerSection.EnableOffset ();
						currentLazerSection.SetLayerOrder (lazerHit, false);
						lazerPosition = GetNextLazerPosition (currentLazerSection.facing);
						lazerHit = null;
					}
				}
				else
					if (exists || currentRange < Grid.LAZER_MAX_RANGE) {
						GameObject obj;
						turret.grid.objects.TryGetValue (turret.TranslateCoordinate (lazerPosition), out obj);
						if (obj != null && !obj.tag.Equals ("SafeZone")) {
							lazerHit = obj;
							if (obj.tag.Equals (Constants.MIRROR_TAG)) {
								currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, impactLazerLength, alpha);
								currentLazerSection.AddImpact (lazerHit, alpha);
								bool flipped = obj.GetComponent<Mirror> ().IsFlipped ();
								lazerDirection = lazerDirection.mirror (flipped);
								exists = ObjectExists (turret.TranslateCoordinate (lazerPosition), lazerDirection);
							}
							else {
								var raycast = Physics2D.Raycast (currentLazerSection.transform.position, lazerDirection.asVec2 (), Mathf.Infinity, 1 << LayerMask.NameToLayer (Constants.OBSTACLE_LAYER));
								var distance = (raycast.distance - Lazer.length / 2) / Lazer.length;
								currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, distance, alpha);
								currentLazerSection.AddImpact (lazerHit, raycast.point, alpha);
							}
							currentLazerSection.SetLayerOrder (lazerHit, true);
							currentRange = 0;
						}
						else {
						currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, 1, alpha);
							lazerPosition = GetNextLazerPosition (currentLazerSection.facing);
							currentRange++;
						}
					}
					else {
						firing = false;
					}
				currentLazerSection.SetLength (Mathf.Min (currentLazerSection.maxLength, excess));
				excess -= currentLazerSection.GetCurrentLength ();
			}
			while (excess > 0 && firing);
		}
		excess = Mathf.Max (excess, 0);
	}

	void InitLazer (float alpha)
	{
		if (currentLazerSection == null) {
			if (lazerPath.Count <= 0) {
				currentLazerSection = StartLazerAt (Direction.ZERO, turret.GetFacing (), impactLazerLength, alpha);
				currentLazerSection.EnableOffset ();
				currentLazerSection.SetLayer ("AccessoryBelow");
			}
			else {
				currentLazerSection = StartLazerAt (turret.GetFacing (), turret.GetFacing (), 1, alpha);
			}
			lazerPosition = GetNextLazerPosition (currentLazerSection.facing);
			lazerDirection = currentLazerSection.facing;
		}
	}
}



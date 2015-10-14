using UnityEngine;
using System.Collections.Generic;

public class Turret : Positional {

	private Transform gun;
	private Direction facing = Direction.LEFT;

	public GameObject lazerPrefab;
	public GameObject lazerImpactPrefab;
	public GameObject explosionPrefab;
	public GameObject smokePrefab;

	public Color destroyedColor;

	private bool destroyed = false;
	private bool firing = false;
	private bool active = true;

	private Lazer currentLazerSection;
	private int currentRange = 0;

	private List<Lazer> lazer = new List<Lazer>();

	private float impactLazerLength;

	// Use this for initialization
	void Awake () {
		gun = this.transform.FindChild ("gun");

		var lazerSprite = lazerPrefab.transform.FindChild ("sprite");
		lazerSprite.localPosition = Vector2.zero;

		Lazer.length = lazerSprite.GetComponent<SpriteRenderer>().bounds.size.x;
		lazerSprite.Translate (new Vector3 (Lazer.length / 2,0,0));

		impactLazerLength = 0.5f;
	}

	void Start(){
		Fire ();
	}

	void Fire(){
		ClearLazer ();
		firing = active;
	}

	public void RotateGun(Direction facing){
		gun.Rotate (new Vector3(0,0,facing.angle));
		this.facing = facing;
	}

	private void ClearLazer(){
		lazer.ForEach (l => Destroy (l.gameObject));
		lazer.Clear ();
		lazerState = Lazer.State.Straight;
		currentLazerSection = null;
		currentRange = 0;
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.Space)) {
			active = !active;
			Fire ();
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		HandleFiring ();
	}

	private Direction lazerDirection;
	private Coordinate lazerPosition;

	private Lazer.State lazerState = Lazer.State.Straight;

	private float excess = 0f;

	/*
	 * THE CHURCH OF THE FLYING SPAGHETTI MONSTER
	 * 
	*/
	void HandleFiring ()
	{
		if (firing) {

			if (currentLazerSection == null) {
				currentLazerSection = StartLazerAt (facing, facing);
				lazerPosition = GetNextLazerPosition (currentLazerSection.facing);
				lazerDirection = currentLazerSection.facing;
			}

			var currentLength = currentLazerSection.GetCurrentLength ();
			var maxLength = currentLazerSection.maxLength;

			bool exists = ObjectExists(TranslateCoordinate(lazerPosition), lazerDirection);

			if (currentLength < maxLength) {
				currentLazerSection.SetLength (Mathf.Min (maxLength, currentLength + Constants.LAZER_SPEED + excess));
				excess = currentLength + Constants.LAZER_SPEED - maxLength;
			} else {
				do {
					if (lazerState != Lazer.State.Straight) {
						AddLazerImpact (currentLazerSection, lazerState);
						if(lazerState == Lazer.State.Impact){
							firing = false;
							EndGame();
							break;
							// TODO END GAME HERE;
						} else {
							currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, impactLazerLength);

							currentLazerSection.transform.FindChild ("sprite").localPosition = Vector2.zero;
							currentLazerSection.SetLayerOrder (lazerState, false);
							
							lazerPosition = GetNextLazerPosition (currentLazerSection.facing);
							lazerState = Lazer.State.Straight;
						}
					} else if (exists || currentRange < Grid.LAZER_MAX_RANGE) {
						GameObject obj;
						grid.objects.TryGetValue (TranslateCoordinate (lazerPosition), out obj);
						if (obj != null && !obj.tag.Equals("SafeZone")) {

							currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, impactLazerLength);

							if(obj.tag.Equals ("Mirror")){
								bool flipped = obj.GetComponent<Mirror> ().IsFlipped ();
								lazerDirection = lazerDirection.mirror (flipped);
								lazerState = flipped ? Lazer.State.FlippedTurn : Lazer.State.Turn;

								exists = ObjectExists(TranslateCoordinate(lazerPosition), lazerDirection);

							} else if(obj.tag.Equals ("Turret")) {
								lazerState = Lazer.State.Impact;
							}

							currentLazerSection.SetLayerOrder (lazerState, true);
							
							currentRange = 0;
						} else {
							currentLazerSection = StartLazerAt (lazerPosition, lazerDirection);
							lazerPosition = GetNextLazerPosition (currentLazerSection.facing);
							currentRange++;
						}
					} else {
						firing = false;
					}
					currentLazerSection.SetLength(Mathf.Min (currentLazerSection.maxLength, excess));
					excess -= currentLazerSection.GetCurrentLength();	
					
				} while(excess > 0 && firing);
			}
			excess = Mathf.Max (excess, 0);
		}
	}

	void EndGame ()
	{
		var obj = grid.objects[TranslateCoordinate (lazerPosition)];
		obj.GetComponent<Turret> ().Explode();

	}

	public void Explode(){
		if (this.destroyed)
			return;

		var explosion = Instantiate (explosionPrefab);
		explosion.transform.position = this.transform.position;

		var smoke = Instantiate (smokePrefab);
		Vector3 pos = this.transform.position;
		pos.z = -1;
		smoke.transform.position = pos;

		this.GetComponent<SpriteRenderer> ().color = destroyedColor;
		this.gun.GetComponent<SpriteRenderer> ().color = destroyedColor;

		this.destroyed = true;
	}

	bool ObjectExists(Lazer lazer){
		return ObjectExists (lazer.position, lazer.facing);
	}

	bool ObjectExists (Coordinate pos, Direction dir)
	{
		List<Coordinate> coordinates = new List<Coordinate>(grid.objects.Keys);
		if (dir.y != 0)
			return coordinates.Exists (c => pos.x == c.x && dir.y * c.y > dir.y * pos.y);
		else if (dir.x != 0)
			return coordinates.Exists (c => pos.y == c.y && dir.x * c.x > dir.x * pos.x);

		return false;
	}

	private Coordinate GetNextLazerPosition(Coordinate dir){
		return lazer[lazer.Count - 1].position + dir;
	}

	private Coordinate TranslateCoordinate(Coordinate pos){
		return pos + this.position;
	}

	private Coordinate ReverseTranslate(Coordinate pos){
		return pos - this.position;
	}

	private Lazer StartLazerAt(Coordinate pos, Direction facing){
		return StartLazerAt (pos, facing, 1);
	}

	private GameObject AddLazerImpact(Lazer parent, Lazer.State state){
		var lazerImpact = Instantiate (lazerImpactPrefab);
		lazerImpact.transform.parent = parent.transform;
		lazerImpact.transform.localPosition = Vector2.zero;

		SetImpactLayerOrder (lazerImpact, parent.facing, state);

		return lazerImpact;
	}
	
	private Lazer StartLazerAt(Coordinate pos, Direction facing, float maxLength){
		var lazer = Instantiate (lazerPrefab);
		lazer.transform.parent = this.transform;
		lazer.transform.localPosition = Vector2.zero;
		lazer.transform.Translate(pos.asVec3() * Lazer.length);
		
		var lazerScript = lazer.GetComponent<Lazer> ();
		lazerScript.Rotate (facing);
		lazerScript.SetLength (0);
		lazerScript.maxLength = maxLength;
		lazerScript.position = pos;
		
		this.lazer.Add (lazerScript);
		
		return lazerScript;
	}

	void SetImpactLayerOrder (GameObject lazerImpact, Coordinate facing, Lazer.State state)
	{
		// Lazer is INCOMING here
		var renderer = lazerImpact.GetComponent<SpriteRenderer> ();
		var front = Lazer.IsFront (facing, state, true);
		renderer.sortingOrder = front ? 2 : -1;
	}
	 
	public void Reroute (Coordinate change, bool flipped)
	{
		if (lazer.Count <= 0)
			return;

		Coordinate translated = ReverseTranslate (change);
		int index = lazer.FindIndex(s => s.position.Equals(translated));
		if (index < 0) {
			var lastLazer = lazer[lazer.Count - 1];
			if(lazerState != Lazer.State.Impact && ObjectExists(TranslateCoordinate(lastLazer.position), lastLazer.facing)){
				currentLazerSection = lastLazer;
				lazerPosition = GetNextLazerPosition(currentLazerSection.facing);
				lazerDirection = currentLazerSection.facing;
				currentRange = 0;
				firing = true;
			}
			return;
		}
			

		index++;
		List<Lazer> destroy = lazer.GetRange(index, lazer.Count - index);
		lazer.RemoveRange(index, lazer.Count - index);
		destroy.ForEach(s => Destroy(s.gameObject));

		if (lazer.Count > 0) {
			currentLazerSection = lazer [lazer.Count - 1];
			currentLazerSection.maxLength = impactLazerLength;
			currentLazerSection.SetLength (impactLazerLength);

			lazerPosition = currentLazerSection.position;
	
			lazerDirection = currentLazerSection.facing.mirror (flipped);
			lazerState = flipped ? Lazer.State.FlippedTurn : Lazer.State.Turn;

			currentLazerSection.SetLayerOrder (lazerState, true);
			foreach (Transform child in currentLazerSection.transform)
				if (!child.tag.Equals ("Sprite"))
					Destroy (child.gameObject);

			currentRange = 0;
			firing = true;
		} else {
			Fire ();
		}
	}
}

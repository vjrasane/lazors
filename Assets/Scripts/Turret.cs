using UnityEngine;
using System.Collections.Generic;

public class Turret : Positional {

	private Transform gun;
	private Direction facing = Direction.LEFT;

	public GameObject lazerPrefab;
	public GameObject explosionPrefab;
	public GameObject smokePrefab;

	public Color playerColor;
	public Color destroyedColor;

	private SpriteRenderer baseRenderer;
	private SpriteRenderer gunRenderer;

	private bool destroyed = false;
	private bool firing = false;
	private bool active = true;

	private Lazer currentLazerSection;
	private int currentRange = 0;

	private List<Lazer> lazerPath = new List<Lazer>();
	private List<GameObject> lazerPool = new List<GameObject> ();

	private float impactLazerLength;

	public int playerNumber = 0;

	// Use this for initialization
	void Awake () {
		gun = this.transform.FindChild ("gun");

		this.baseRenderer = this.GetComponent<SpriteRenderer> ();
		this.gunRenderer = this.gun.GetComponent<SpriteRenderer> ();

		baseRenderer.color = playerColor;
		gunRenderer.color = playerColor;

		var lazerSprite = lazerPrefab.transform.FindChild ("sprite");
		lazerSprite.localPosition = Vector2.zero;

		Lazer.length = lazerSprite.GetComponent<SpriteRenderer>().bounds.size.x;
		lazerSprite.transform.Translate (new Vector3 (Lazer.length / 2,0,0));

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
		lazerPath.ForEach (l => MoveToPool(l));
		lazerPath.Clear ();
		lazerHit = null;
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

	private float excess = 0f;

	private GameObject lazerHit = null;

	/*
	 * THE CHURCH OF THE FLYING SPAGHETTI MONSTER
	 * 
	*/
	void HandleFiring ()
	{
		if (firing) {

			if (currentLazerSection == null) {
				if(lazerPath.Count <= 0){
					currentLazerSection = StartLazerAt ( Direction.ZERO, facing);
					currentLazerSection.EnableOffset();
					currentLazerSection.maxLength = impactLazerLength;
					currentLazerSection.SetLayer("AccessoryBelow");
				} else {
					currentLazerSection = StartLazerAt (facing, facing);
				}
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
					if (lazerHit != null) {
						currentLazerSection.AddImpact(lazerHit);
						if(lazerHit.tag.Equals("Turret")){
							firing = false;
							EndGame();
							break;
							// TODO END GAME HERE;
						} else {
							currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, impactLazerLength);

							currentLazerSection.EnableOffset();
							currentLazerSection.SetLayerOrder (lazerHit, false);
							
							lazerPosition = GetNextLazerPosition (currentLazerSection.facing);
							lazerHit = null;
						}
					} else if (exists || currentRange < Grid.LAZER_MAX_RANGE) {
						GameObject obj;
						grid.objects.TryGetValue (TranslateCoordinate (lazerPosition), out obj);
						if (obj != null && !obj.tag.Equals("SafeZone")) {
							lazerHit = obj;
							currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, impactLazerLength);

							if(obj.tag.Equals (Constants.MIRROR_TAG)){
								bool flipped = obj.GetComponent<Mirror> ().IsFlipped ();
								lazerDirection = lazerDirection.mirror (flipped);

								exists = ObjectExists(TranslateCoordinate(lazerPosition), lazerDirection);

							}
							currentLazerSection.SetLayerOrder (lazerHit, true);
							
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

		Camera.main.GetComponent<CameraController> ().Shake ();

		var smoke = Instantiate (smokePrefab);
		Vector3 pos = this.transform.position;
		pos.z = -1;
		smoke.transform.position = pos;

		baseRenderer.color = destroyedColor;
		gunRenderer.color = destroyedColor;

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
		return lazerPath[lazerPath.Count - 1].position + dir;
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

	private Lazer StartLazerAt(Coordinate pos, Direction facing, float maxLength){
		GameObject lazer;

		if (lazerPool.Count <= 0)
			lazer = Instantiate (lazerPrefab);
		else {
			lazer = GetFromPool();
		}

		lazer.transform.parent = this.transform;
		lazer.transform.localPosition = Vector2.zero;
		lazer.transform.Translate(pos.asVec3() * Lazer.length);
		
		var lazerScript = lazer.GetComponent<Lazer> ();
		lazerScript.Rotate (facing);
		lazerScript.SetLength (0);
		lazerScript.maxLength = maxLength;
		lazerScript.position = pos;
		lazerScript.layerOrderMultiplier = this.playerNumber;
		lazerScript.SendToBack ();
		
		this.lazerPath.Add (lazerScript);
		
		return lazerScript;
	}
	 
	public void Reroute (Coordinate change, bool flipped)
	{
		if (lazerPath.Count <= 0)
			return;

		Coordinate translated = ReverseTranslate (change);
		int index = lazerPath.FindIndex(s => s.position.Equals(translated));
		if (index < 0) {
			var lastLazer = lazerPath[lazerPath.Count - 1];
			if(lazerHit == null && ObjectExists(TranslateCoordinate(lastLazer.position), lastLazer.facing)){
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
			lazerHit = grid.objects[change];

			currentLazerSection.SetLayerOrder (lazerHit, true);
			currentLazerSection.DisableImpact();

			currentRange = 0;
			firing = true;
		} else {
			Fire ();
		}
	}

	private void MoveToPool(Lazer lazer){
		lazer.transform.localRotation = Quaternion.identity;
		lazer.transform.localPosition = Vector2.zero;

		lazer.ResetLayer ();

		lazer.DisableOffset ();
		lazer.DisableImpact ();

		lazer.gameObject.SetActive (false);

		lazerPool.Add (lazer.gameObject);
	}

	private GameObject GetFromPool(){
		var lazer = lazerPool[0].gameObject;
		lazer.SetActive(true);
		lazerPool.RemoveAt (0);

		return lazer;
	}
}

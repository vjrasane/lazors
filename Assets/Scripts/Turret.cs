using UnityEngine;
using System.Collections.Generic;

public class Turret : Positional {

	private Transform gun;
	private Direction facing = Direction.LEFT;

	public GameObject lazerPrefab;
	public GameObject explosionPrefab;
	public GameObject smokePrefab;

	public Color lazerColor;
	public Color playerColor;
	public Color destroyedColor;

	private SpriteRenderer baseRenderer;
	private SpriteRenderer gunRenderer;

	private SpriteRenderer baseShadowRenderer;
	private SpriteRenderer gunShadowRenderer;

	private bool destroyed = false;
	private bool firing = false;
	private bool active = true;

	private Lazer currentLazerSection;
	private int currentRange = 0;

	private List<Lazer> lazerPath = new List<Lazer>();
	private static List<GameObject> LAZER_POOL = new List<GameObject> ();

	private float impactLazerLength = 0.5f;

	public int playerNumber = 0;

	// Use this for initialization
	void Awake () {
		gun = this.transform.FindChild ("gun");

		this.baseRenderer = this.GetComponent<SpriteRenderer> ();
		this.gunRenderer = this.gun.GetComponent<SpriteRenderer> ();

		this.baseShadowRenderer = this.transform.FindChild("base_shadow").GetComponent<SpriteRenderer> ();
		this.gunShadowRenderer = this.gun.transform.FindChild("gun_shadow").GetComponent<SpriteRenderer> ();

		baseRenderer.color = playerColor;
		gunRenderer.color = playerColor;

		var lazerSprite = lazerPrefab.transform.FindChild ("sprite");
		lazerSprite.localPosition = Vector2.zero;

		Lazer.length = lazerSprite.GetComponent<SpriteRenderer>().bounds.size.x;
		lazerSprite.transform.Translate (new Vector3 (Lazer.length / 2,0,0));
	}

	void Start(){
		Fire ();
	}

	void Fire(){
		ClearLazer ();
		firing = active;
	}

	void CeaseFire(){
		ClearLazer ();
		firing = false;
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
			active = !active && !destroyed;
			Debug.Log (LAZER_POOL.Count);
			Fire ();
		}

		HandleExplode ();
	}

	void HandleExplode ()
	{
		if (exploding) {
			if (explodeTime < Constants.EXPLOSION_CHARGEUP_DURATION) {
				var color = Color.Lerp (playerColor, Color.white, explodeTime / Constants.EXPLOSION_CHARGEUP_DURATION);
				this.baseRenderer.color = color;
				this.gunRenderer.color = color;
				var c = Color.Lerp (Color.white, Constants.COLOR_TRANSPARENT, explodeTime / Constants.EXPLOSION_CHARGEUP_DURATION);
				this.baseShadowRenderer.color = c;
				this.gunShadowRenderer.color = c;
				explodeTime += Time.deltaTime;
			}
			else {
				var explosion = Instantiate (explosionPrefab);
				explosion.transform.position = this.transform.position;
				Camera.main.GetComponent<CameraController> ().Shake ();
				var smoke = Instantiate (smokePrefab);
				Vector3 pos = this.transform.position;
				pos.z = -1;
				smoke.transform.position = pos;
				baseRenderer.color = destroyedColor;
				gunRenderer.color = destroyedColor;

				CeaseFire();

				exploding = false;
				destroyed = true;
			}
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

							if(obj.tag.Equals (Constants.MIRROR_TAG)){
								currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, impactLazerLength);
								currentLazerSection.AddImpact(lazerHit);

								bool flipped = obj.GetComponent<Mirror> ().IsFlipped ();
								lazerDirection = lazerDirection.mirror (flipped);

								exists = ObjectExists(TranslateCoordinate(lazerPosition), lazerDirection);

							} else {
								var raycast = Physics2D.Raycast(currentLazerSection.transform.position, lazerDirection.asVec2(), Mathf.Infinity, 1 << LayerMask.NameToLayer(Constants.OBSTACLE_LAYER));
								var distance = (raycast.distance - Lazer.length / 2) / Lazer.length;
								currentLazerSection = StartLazerAt (lazerPosition, lazerDirection, distance);
								currentLazerSection.AddImpact(lazerHit, raycast.point);
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

	private float explodeTime = 0.0f;
	private bool exploding = false;

	public void Explode(){
		if (this.destroyed || this.exploding)
			return;

		this.exploding = true;
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
			currentLazerSection.AddImpact(lazerHit);

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

		LAZER_POOL.Add (lazer.gameObject);
	}

	private GameObject GetFromPool(){
		var lazer = LAZER_POOL[0].gameObject;
		lazer.SetActive(true);
		LAZER_POOL.RemoveAt (0);

		return lazer;
	}
}

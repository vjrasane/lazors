using UnityEngine;
using System.Collections.Generic;

public class Turret : Piece {

	public Player player;

	private Transform gun;
	private LazerController lazer;
	private Direction facing = Direction.LEFT;
	
	public GameObject explosionPrefab;
	public GameObject smokePrefab;
	
	public Color playerColor;
	public Color destroyedColor;

	private SpriteRenderer baseRenderer;
	private SpriteRenderer gunRenderer;

	private SpriteRenderer baseShadowRenderer;
	private SpriteRenderer gunShadowRenderer;

	private bool destroyed = false;

	private bool active = true;

	// Use this for initialization
	void Awake () {
		gun = this.transform.FindChild ("gun");

		this.lazer = this.GetComponent<LazerController> ();
		this.lazer.turret = this;

		this.baseRenderer = this.GetComponent<SpriteRenderer> ();
		this.gunRenderer = this.gun.GetComponent<SpriteRenderer> ();

		this.baseShadowRenderer = this.transform.FindChild("base_shadow").GetComponent<SpriteRenderer> ();
		this.gunShadowRenderer = this.gun.transform.FindChild("gun_shadow").GetComponent<SpriteRenderer> ();

		baseRenderer.color = playerColor;
		gunRenderer.color = playerColor;

	}

	public void Fire(){
		lazer.Fire ();
	}

	public void RotateGun(Direction facing){
		gun.Rotate (new Vector3(0,0,this.facing.angle(facing)));
		this.facing = facing;
	}

	public Direction GetFacing(){
		return facing;
	}
	
	void Update(){
		if (Input.GetKeyDown (KeyCode.Space)) {
			active = !active && !destroyed;
			lazer.Fire ();
		}

		HandleExplode ();
	}

	public bool IsActive(){
		return active && !destroyed;
	}

	public void Preview ()
	{
		this.lazer.Preview ();
	}

	public void ClearPreview ()
	{
		this.lazer.ClearPreview ();
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
				Camera.main.GetComponent<CameraController> ().Shake (Constants.EXPLOSION_SHAKE_DURATION, Constants.EXPLOSION_SHAKE_MAGNITUDE);
				var smoke = Instantiate (smokePrefab);
				Vector3 pos = this.transform.position;
				//pos.z = -1;
				smoke.transform.position = pos;
				baseRenderer.color = destroyedColor;
				gunRenderer.color = destroyedColor;

				lazer.CeaseFire();
				lazer.ClearPreview();

				exploding = false;
				destroyed = true;
			}
		}
	}

	public Coordinate TranslateCoordinate(Coordinate pos){
		return pos + this.position;
	}

	public Coordinate ReverseTranslate(Coordinate pos){
		return pos - this.position;
	}

	private float explodeTime = 0.0f;
	private bool exploding = false;

	public void Explode(){
		if (this.destroyed || this.exploding)
			return;

		this.exploding = true;
	}

	public override void OnClick ()
	{
		
	}

	public override void OnHover ()
	{
		
	}

	public override void OnExit ()
	{
		
	}

}

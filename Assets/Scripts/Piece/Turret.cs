using UnityEngine;
using System.Collections.Generic;

public class Turret : LazerController {

	private Transform gun;
	private Direction facing = Direction.LEFT;
	
	public GameObject explosionPrefab;
	public GameObject smokePrefab;

	public Color destroyedColor;

	private SpriteRenderer baseRenderer;
	private SpriteRenderer gunRenderer;

	private SpriteRenderer baseShadowRenderer;
	private SpriteRenderer gunShadowRenderer;

	void Awake () {
		base.Init ();

		gun = this.transform.FindChild ("gun");

		this.baseRenderer = this.GetComponent<SpriteRenderer> ();
		this.gunRenderer = this.gun.GetComponent<SpriteRenderer> ();

		this.baseShadowRenderer = this.transform.FindChild("base_shadow").GetComponent<SpriteRenderer> ();
		this.gunShadowRenderer = this.gun.transform.FindChild("gun_shadow").GetComponent<SpriteRenderer> ();

	}

	void Start(){
		this.normalColor = player.color;
		
		baseRenderer.color = normalColor;
		gunRenderer.color = normalColor;
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
			this.active = !active && !destroyed;
			this.Fire ();
		}

		HandleExplode ();
	}

	public void Fire ()
	{
		this.FireLazer (this.facing);
	}

	public void ShowPreview ()
	{
		this.FirePreview (this.facing);
	}

	void HandleExplode ()
	{
		if (exploding) {
			if (explodeTime < Constants.EXPLOSION_CHARGEUP_DURATION) {
				var color = Color.Lerp (normalColor, Color.white, explodeTime / Constants.EXPLOSION_CHARGEUP_DURATION);
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

				this.CeaseFire();
				this.ClearPreview();

				exploding = false;
				destroyed = true;
			}
		}
	}

	private float explodeTime = 0.0f;
	private bool exploding = false;

	public void Explode(){
		if (this.destroyed || this.exploding)
			return;

		this.exploding = true;
	}

	#region implemented abstract members of PieceObject

	public override Piece.PieceType GetPieceType ()
	{
		return Piece.PieceType.Turret;
	}

	public override Piece AsPiece ()
	{
		return new Piece.Turret(facing, player.number);
	}

	#endregion

}

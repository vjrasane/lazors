using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Grid : MonoBehaviour {

	public Scenario scenario;

	public GameObject squarePrefab;
	public GameObject safeZonePrefab;
	public GameObject turretPrefab;
	public GameObject mirrorPrefab;




	public Color previewColor;

	private int maxX = 0;
	private int minX = 0;
	private int maxY = 0;
	private int minY = 0;

	public static int LAZER_MAX_RANGE = Constants.LAZER_MIN_RANGE;

	public Dictionary<Coordinate, Piece> objects = new Dictionary<Coordinate, Piece> ();
	private Dictionary<Coordinate, GridSquare> squares = new Dictionary<Coordinate, GridSquare> ();

	private List<Player> players = new List<Player> ();

	private Player inTurn;
	private bool turnDone = false;

	private List<Turret> turrets = new List<Turret> ();

	public static float SQUARE_SIZE = 0.0f;
	private Coordinate center;

	public Mirror previewPiece;

	// INDICATORS
	public GameObject arrowsPrefab;
	public GameObject arrows;
	public GameObject blockedPrefab;
	public GameObject blocked;

	void Awake(){
		center = new Coordinate((int)(Constants.GRID_MAX_SIZE / 2), (int)(Constants.GRID_MAX_SIZE / 2));
		var squareBlock = InstantiateAt<Positional>(TranslateCoordinate(center), squarePrefab);

		previewPiece = Instantiate (mirrorPrefab).GetComponent<Mirror>();
		previewPiece.GetComponent<SpriteRenderer> ().color = previewColor;
		previewPiece.preview = true;
		previewPiece.gameObject.SetActive (false);
		previewPiece.grid = this;

		SQUARE_SIZE = squareBlock.GetComponent<BoxCollider2D> ().bounds.size.x;

		Destroy (squareBlock.gameObject);

		arrows = Instantiate (arrowsPrefab);
		arrows.SetActive (false);
		blocked = Instantiate (blockedPrefab);
		blocked.SetActive (false);
	}

	// Use this for initialization
	void Start () {
		CreateScenario ();

		DrawSquares ();

		inTurn = players.First ();

		UIController.DisplayWelcomeText ();
		UIController.DisplayTurnText (inTurn.name);

		FireTurrets ();
	}

	public void FireTurrets(){
		turrets.ForEach (t => t.Fire ());
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.Tab) && previewPiece.gameObject.activeSelf) {
			previewPiece.Flip();
			ClearPreviews();
			PreviewLazers();
		}

		CheckTurn ();
	}

	public void ChangeTurn(){
		turnDone = true;
	}

	void CheckTurn ()
	{
		if (turnDone) {
			var current = this.players.IndexOf(inTurn);
			inTurn = this.players[++current % this.players.Count];
			turnDone = false;
			UIController.DisplayTurnText (inTurn.name);
		}
	}

	void DrawSquares(){
		var startX = minX - Constants.GRID_SQUARE_RANGE;
		var endX = maxX + Constants.GRID_SQUARE_RANGE;

		var startY = minY - Constants.GRID_SQUARE_RANGE;
		var endY = maxY + Constants.GRID_SQUARE_RANGE;
		for(var x = startX; x <= endX; x++){
			for(var y = startY; y <= endY; y++){

				var coordinate = new Coordinate (x, y);
				if(!squares.ContainsKey(coordinate)){
					var squareObj = InstantiateAt<Positional>(coordinate, squarePrefab);

					var gridSquare = squareObj.GetComponent<GridSquare>();

					gridSquare.grid = this;
					gridSquare.position = coordinate;
					gridSquare.piece = objects.ContainsKey(coordinate) ? objects[coordinate] : null;

					squares.Add(coordinate, gridSquare);
				}
			}
		}
	}

	public Piece PutSafeZone(Coordinate pos){
		return Put (pos, safeZonePrefab);
	}

	public Piece PutMirror (Coordinate pos, bool flipped)
	{
		ClearPreviews ();
		var mirror = Put (pos, mirrorPrefab);
		if (flipped)
			mirror.GetComponent<Mirror> ().Flip ();

		ChangeTurn ();
		FireTurrets();

		return mirror;
	}

	private Piece Put(Coordinate pos, GameObject prefab){
		var obj = InstantiateAt<Piece> (pos, prefab, true);

		objects.Add (pos, obj);

		DrawSquares ();

		return obj;
	}

	public void PreviewAt(Positional pos){
		previewPiece.transform.position = pos.transform.position;
		previewPiece.position = pos.position;
		previewPiece.gameObject.SetActive (true);
	}

	public void HidePreview(){
		previewPiece.position = null;
		previewPiece.gameObject.SetActive (false);
	}

	public void PreviewLazers(){
		turrets.ForEach(t => t.Preview());
	}

	public void ClearPreviews(){
		turrets.ForEach(t => t.ClearPreview());
	}

	private P InstantiateAt<P>(Coordinate position, GameObject prefab) where P : Positional{
		return InstantiateAt<P> (position, prefab, false);
	}

	private P InstantiateAt<P>(Coordinate position, GameObject prefab, bool checkBounds) where P : Positional{
		var obj = Instantiate (prefab);
		obj.transform.parent = this.transform;
		obj.transform.localPosition = GetPosition (position);

		P positional = obj.GetComponent<P>();
		positional.position = position;
		positional.grid = this;

		if (checkBounds)
			CheckBounds (position);

		return positional;
	}

	void CheckBounds (Coordinate position)
	{
		maxX = Mathf.Max (position.x, maxX);
		minX = Mathf.Min (position.x, minX);
		maxY = Mathf.Max (position.y, maxY);
		minY = Mathf.Min (position.y, minY);

		LAZER_MAX_RANGE = Mathf.Max (maxX - minX, maxY - minY) + Constants.LAZER_MIN_RANGE;
	}

	Vector2 GetPosition (Coordinate pos)
	{
		if (center == null)
			return Vector2.zero;
		return Vector2.zero + pos.asVec2() * SQUARE_SIZE;
	}

	void CreateScenario ()
	{
		if (scenario == null) {
			scenario = new Scenario();
		}

		if (this.players.Count < scenario.playerCount)
			GeneratePlayers (scenario.playerCount);

		foreach (Coordinate coord in scenario.pieces.Keys) {
			var piece = scenario.pieces[coord];

			if(piece.GetType() == typeof(Scenario.SafeZone)){
				// No need to translate here
				objects.Add (coord, InstantiateAt<Piece>(coord, safeZonePrefab, true));
			} else if(piece.GetType() == typeof(Scenario.Turret)){
				Scenario.Turret turretPiece = ((Scenario.Turret)piece);
				var facing = turretPiece.facing;
				var turret = InstantiateAt<Piece>(coord, turretPrefab, true);

				var turretScript = turret.GetComponent<Turret>();
				turretScript.RotateGun(facing);

				if(turretPiece.playerNum > 0) {
					var player = players[turretPiece.playerNum - 1];
					UIController.AddPlayerLabel(player.name, turret.transform.position);
					turretScript.player = player;
				}
				
				turrets.Add(turretScript);
				objects.Add (coord, turret);
			}
		}
	}

	void GeneratePlayers (int count)
	{
		for (var i = 0; i < count; i++) {
			var num = i + 1;
			players.Add(new Player("Player " + num, num));
		}
	}

	Coordinate TranslateCoordinate (Coordinate pos)
	{
		return pos - center;
	}
}

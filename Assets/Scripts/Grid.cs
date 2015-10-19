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

	public static int LAZER_MAX_RANGE = Constants.LAZER_MAX_RANGE;

	public Dictionary<Coordinate, Positional> objects = new Dictionary<Coordinate, Positional> ();
	private Dictionary<Coordinate, GridSquare> squares = new Dictionary<Coordinate, GridSquare> ();

	private List<Player> players = new List<Player> ();

	private Player inTurn;
	private bool turnDone = false;

	private List<Turret> turrets = new List<Turret> ();

	public static float SQUARE_SIZE = 0.0f;
	private Coordinate center;

	public Mirror selectedPiece;

	void Awake(){
		center = new Coordinate((int)(Constants.GRID_MAX_SIZE / 2), (int)(Constants.GRID_MAX_SIZE / 2));
		var squareBlock = InstantiateAt(TranslateCoordinate(center), safeZonePrefab);

		selectedPiece = Instantiate (mirrorPrefab).GetComponent<Mirror>();
		selectedPiece.GetComponent<SpriteRenderer> ().color = previewColor;
		selectedPiece.GetComponent<BoxCollider2D> ().enabled = false;
		selectedPiece.preview = true;
		selectedPiece.gameObject.SetActive (false);

		SQUARE_SIZE = squareBlock.GetComponent<BoxCollider2D> ().bounds.size.x;

		Destroy (squareBlock.gameObject);
	}

	// Use this for initialization
	void Start () {
		CreateScenario ();

		DrawSquares ();

		inTurn = players.First ();

		UIController.DisplayWelcomeText ();
		UIController.DisplayTurnText (inTurn.name);

		Invoke("Fire", 1f);
	}

	void Fire(){
		turrets.ForEach (t => t.Fire ());
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.Tab) && selectedPiece.gameObject.activeSelf) {
			selectedPiece.Flip();
			ClearPreviews();
			PreviewLazers(selectedPiece);
		}

		CheckTurn ();
	}

	public void ChangeTurn(){
		turnDone = true;
	}

	void CheckTurn ()
	{
		if (turnDone && !Firing ()) {
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
					var squareObj = InstantiateAt(coordinate, squarePrefab);

					var gridSquare = squareObj.GetComponent<GridSquare>();

					gridSquare.grid = this;
					gridSquare.position = coordinate;
					gridSquare.SetDisabled(objects.ContainsKey(coordinate));

					squares.Add(coordinate, gridSquare);
				}
			}
		}
	}

	public void PutSafeZone(Coordinate pos){
		Put (pos, safeZonePrefab);
	}

	public void PutMirror (Coordinate pos, bool flipped)
	{
		ClearPreviews ();
		var mirror = Put (pos, mirrorPrefab);
		if (flipped)
			mirror.GetComponent<Mirror> ().Flip ();

		ChangeTurn ();
		RerouteLazers(mirror);
	}

	private Positional Put(Coordinate pos, GameObject prefab){
		var obj = InstantiateAt (pos, prefab, true);

		objects.Add (pos, obj);

		DrawSquares ();

		return obj;
	}

	public void RerouteLazers(Positional change){
		turrets.ForEach(t => t.Reroute(change));
	}

	public void PreviewLazers(Positional change){
		turrets.ForEach(t => t.Preview(change));
	}

	public void ClearPreviews(){
		turrets.ForEach(t => t.ClearPreview());
	}

	public bool Firing(){
		return turrets.Exists (t => t.IsFiring());
	}

	private Positional InstantiateAt(Coordinate position, GameObject prefab) {
		return InstantiateAt (position, prefab, false);
	}

	private Positional InstantiateAt(Coordinate position, GameObject prefab, bool checkBounds){
		var obj = Instantiate (prefab);
		obj.transform.parent = this.transform;
		obj.transform.localPosition = GetPosition (position);

		Positional positional = obj.GetComponent<Positional>();
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

		LAZER_MAX_RANGE = GetMaxValue (maxX, -minX, maxY, -minY, LAZER_MAX_RANGE);
	}

	int GetMaxValue(params int[] values){
		return values.Max ();
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
				objects.Add (coord, InstantiateAt(coord, safeZonePrefab, true));
			} else if(piece.GetType() == typeof(Scenario.Turret)){
				Scenario.Turret turretPiece = ((Scenario.Turret)piece);
				var facing = turretPiece.facing;
				var turret = InstantiateAt(coord, turretPrefab, true);

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

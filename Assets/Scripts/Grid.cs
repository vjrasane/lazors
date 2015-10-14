using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

	public Scenario scenario;

	public GameObject squarePrefab;
	public GameObject safeZonePrefab;
	public GameObject turretPrefab;
	public GameObject mirrorPrefab;

	//private GameObject[,] squares = new GameObject[Constants.GRID_MAX_SIZE, Constants.GRID_MAX_SIZE];
	public Dictionary<Coordinate, GameObject> objects = new Dictionary<Coordinate, GameObject> ();
	private Dictionary<Coordinate, GridSquare> squares = new Dictionary<Coordinate, GridSquare> ();

	private List<Turret> turrets = new List<Turret> ();

	public static float SQUARE_SIZE = 0.0f;
	private Coordinate center;

	void Awake(){
		center = new Coordinate((int)(Constants.GRID_MAX_SIZE / 2), (int)(Constants.GRID_MAX_SIZE / 2));
		var squareBlock = InstantiateAt(TranslateCoordinate(center), safeZonePrefab);

		SQUARE_SIZE = squareBlock.GetComponent<BoxCollider2D> ().bounds.size.x;

		Destroy (squareBlock.gameObject);
	}

	// Use this for initialization
	void Start () {
		CreateScenario ();

		DrawSquares ();
	}

	// Update is called once per frame
	void Update () {
		
	}

	void DrawSquares(){
		foreach(KeyValuePair<Coordinate, GameObject> pair in objects){
			DrawSquaresAround(pair.Key);
		}
	}

	void DrawSquaresAround (Coordinate pos)
	{
		var centerX = pos.x;
		var centerY = pos.y;

		var startX = centerX - Constants.GRID_SQUARE_RANGE;
		var endX = centerX + Constants.GRID_SQUARE_RANGE;

		var startY = centerY - Constants.GRID_SQUARE_RANGE;
		var endY = centerY + Constants.GRID_SQUARE_RANGE;

		for(var x = startX; x <= endX; x++){
			for(var y = startY; y <= endY; y++){

				var coordinate = new Coordinate (x, y);
				if(!objects.ContainsKey(coordinate) && !squares.ContainsKey(coordinate)){
					var squareObj = InstantiateAt(coordinate, squarePrefab);

					var gridSquare = squareObj.GetComponent<GridSquare>();
					gridSquare.grid = this;

					gridSquare.position = coordinate;

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
		var mirror = Put (pos, mirrorPrefab);
		if (flipped)
			mirror.GetComponent<Mirror> ().Flip ();

		RerouteLazers(pos, flipped);
	}

	private GameObject Put(Coordinate pos, GameObject prefab){
		var obj = InstantiateAt (pos, prefab);
		
		squares.Remove (pos);
		objects.Add (pos, obj);
		
		DrawSquaresAround (pos);

		return obj;
	}

	public void RerouteLazers(Coordinate change, bool flipped){
		turrets.ForEach(t => t.Reroute(change, flipped));
	}

	private GameObject InstantiateAt(Coordinate position, GameObject prefab){
		var obj = Instantiate (prefab);
		obj.transform.parent = this.transform;
		obj.transform.localPosition = GetPosition (position);

		Positional positional = obj.GetComponent<Positional>();
		if (positional != null) {
			positional.position = position;
			positional.grid = this;
		}

		return obj;
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

		foreach (KeyValuePair<Coordinate, Scenario.GamePiece> pair in scenario.pieces) {
			var piece = pair.Value;
			var target = pair.Key;

			if(piece.GetType() == typeof(Scenario.SafeZone)){
				// No need to translate here
				objects.Add (target, InstantiateAt(target, safeZonePrefab));
			} else if(piece.GetType() == typeof(Scenario.Turret)){
				var facing = ((Scenario.Turret)piece).facing;
				var turret = InstantiateAt(target, turretPrefab);
				var turretScript = turret.GetComponent<Turret>();
				turretScript.RotateGun(facing);

				turrets.Add(turretScript);
				objects.Add (target, turret);
			}
		}
	}

	Coordinate TranslateCoordinate (Coordinate pos)
	{
		return pos - center;
	}
}

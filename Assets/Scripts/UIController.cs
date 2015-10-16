using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

	private static UIController INSTANCE;
	
	public Text playerLabelPrefab;

	public float labelOffset = 0.0f;

	private Dictionary<Text, Vector3> playerLabelPositions = new Dictionary<Text, Vector3>();

	void Awake () {
		INSTANCE = this;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		foreach (Text label in playerLabelPositions.Keys) {
			var pos = Camera.main.WorldToScreenPoint(playerLabelPositions[label]);
			pos.y += Constants.PLAYER_LABEL_OFFSET;
			label.transform.position = pos;
		}
	}

	public static void AddPlayerLabel(string playerName, Vector3 position){
		var label = Instantiate<Text> (INSTANCE.playerLabelPrefab);
		label.text = playerName;
		label.transform.SetParent(INSTANCE.transform, true);
		INSTANCE.playerLabelPositions.Add (label, position);
	}
}

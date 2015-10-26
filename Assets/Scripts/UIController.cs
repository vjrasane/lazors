using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

	private static UIController INSTANCE;
	
	public Text playerLabelPrefab;

	private TurnText turnText;

	private Animator welcomeTextAnim;
	private Animator turnTextAnim;


	public float labelOffset = 0.0f;

	private Dictionary<Text, Transform> playerLabelPositions = new Dictionary<Text, Transform>();

	void Awake () {
		INSTANCE = this;
	}
	// Use this for initialization
	void Start () {
		turnText = this.transform.FindChild ("turn_text").GetComponent<TurnText> ();

		turnTextAnim = turnText.GetComponent<Animator> ();
		welcomeTextAnim = this.transform.FindChild ("welcome_text").GetComponent<Animator>();

	}

	public static void DisplayWelcomeText(){
		INSTANCE.welcomeTextAnim.SetTrigger ("Enter");

	}

	public static void DisplayTurnText(string playerName){
		INSTANCE.turnTextAnim.SetTrigger ("Enter");
		INSTANCE.turnText.playerName = playerName;
	}

	// Update is called once per frame
	void Update () {
		foreach (Text label in playerLabelPositions.Keys) {
			var pos = Camera.main.WorldToScreenPoint(playerLabelPositions[label].position);
			pos.y += Constants.PLAYER_LABEL_OFFSET;
			label.transform.position = pos;
		}
	}

	public static void AddPlayerLabel(string playerName, Transform transform){
		var label = Instantiate<Text> (INSTANCE.playerLabelPrefab);
		label.text = playerName;
		label.transform.SetParent(INSTANCE.transform, true);
		INSTANCE.playerLabelPositions.Add (label, transform);
	}
}

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
		Singletons.UI = this;
	}
	// Use this for initialization
	void Start () {
		turnText = this.transform.FindChild ("turn_text").GetComponent<TurnText> ();

		turnTextAnim = turnText.GetComponent<Animator> ();
		welcomeTextAnim = this.transform.FindChild ("welcome_text").GetComponent<Animator>();
	}

	public void DisplayWelcomeText(){
		welcomeTextAnim.SetTrigger ("Enter");
	}

	public void DisplayTurnText(string playerName){
		turnTextAnim.SetTrigger ("Enter");
		turnText.playerName = playerName;
	}

	// Update is called once per frame
	void Update () {
		foreach (Text label in playerLabelPositions.Keys) {
			var pos = Camera.main.WorldToScreenPoint(playerLabelPositions[label].position);
			pos.y += Constants.PLAYER_LABEL_OFFSET;
			label.transform.position = pos;
		}
	}

	public void AddPlayerLabel(string playerName, Transform transform){
		var label = Instantiate<Text> (playerLabelPrefab);
		label.text = playerName;
		label.transform.SetParent(transform, true);
		playerLabelPositions.Add (label, transform);
	}
}

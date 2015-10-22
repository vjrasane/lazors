using UnityEngine;
using System.Collections;

public class SafeZone : Piece {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override void OnClick ()
	{

	}

	public override void OnHover ()
	{
		grid.blocked.transform.position = this.transform.position;
		grid.blocked.SetActive (true);
	}

	public override void OnExit ()
	{
		grid.blocked.SetActive (false);
	}
}

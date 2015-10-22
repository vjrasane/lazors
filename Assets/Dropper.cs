using UnityEngine;
using System;
using System.Collections;

public class Dropper : MonoBehaviour {

	public GameObject dustPrefab;

	public Action onDone = () => {};

	private bool done = false;

	private Grid grid;
	public Piece obj;

	private Transform drop;
	private Animator animator;

	// Use this for initialization
	void Awake () {
		this.drop = this.transform.FindChild ("drop");
		this.animator = this.GetComponent<Animator> ();
	}

	public void Insert(Piece obj, Grid grid){
		this.grid = grid;
		this.obj = obj;
		obj.gameObject.SetActive (false);
		obj.transform.parent = this.drop.transform;
		obj.transform.localPosition = Vector2.zero;
	}

	public void Done() {
		this.done = true;
		Clear ();
		this.onDone ();
	}

	public bool IsDone() {
		return this.done;
	}
	
	public void Clear(){
		obj.transform.parent = grid.transform;
	}

	public void Drop(){
		animator.SetTrigger ("Enter");
	}

	public void Show(){
		obj.gameObject.SetActive (true);
	}

	public void SetTransparency(float alpha){
		var renderer = this.GetComponentInChildren<SpriteRenderer> ();
		var color = renderer.color;
		color.a = alpha;
		renderer.color = color;
	}

	public void SpawnDust(){
		var dust = Instantiate (dustPrefab);

		dust.transform.position = this.transform.position;
	}

	// Update is called once per frame
	void Update () {
	
	}
}

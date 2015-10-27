using UnityEngine;
using System.Collections.Generic;

public class Player
{
	public string name;
	public int number;
	public Color color;
	public bool defeated = false;

	public Player(string name){
		this.name = name;
	}

	public Player(string name, int number){
		this.name = name;
		this.number = number;
	}

	public Player(string name, int number, Color color){
		this.name = name;
		this.number = number;
		this.color = color;
	}

}


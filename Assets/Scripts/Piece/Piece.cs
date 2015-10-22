using UnityEngine;
using System.Collections;

public abstract class Piece : Positional {

	public abstract void OnClick();

	public abstract void OnHover();

	public abstract void OnExit();
}

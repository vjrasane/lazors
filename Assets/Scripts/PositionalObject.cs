using UnityEngine;
using System.Collections;

public class PositionalObject : MonoBehaviour, Positional {

	private Coordinate position;
	private bool preview;

	#region Positional implementation
	public Coordinate Position {
		get {
			return position;
		}
		set {
			position = value;
		}
	}
	public bool Preview {
		get {
			return preview;
		}
		set {
			preview = value;
		}
	}
	#endregion

}

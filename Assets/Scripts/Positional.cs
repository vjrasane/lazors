using UnityEngine;
using System.Collections;

public interface Positional {

	Coordinate Position { get; set; }
	bool Preview { get; set; }

}

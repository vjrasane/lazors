using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static class Utils  {

	public static IEnumerable<T> GetValues<T>(){
		return Enum.GetValues (typeof(T)).Cast<T> ();
	}
}

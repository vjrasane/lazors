using UnityEngine;
using System.Collections;

public abstract class Lazer : Positional {

	public static float LENGTH = 0.0f;

	public Color lazerColor;

	public void SendTo(bool front, int layer){
		if (front)
			BringToFront (layer);
		else
			SendToBack (layer);
	}

	public void ResetLayer(){
		SetLayer ("LazerLevel");
	}

	public abstract void BringToFront (int layer);

	public abstract void SendToBack(int layer);

	public void SetColor(Color color){
		this.lazerColor = color;
		SetRenderColor (color);
	}

	public abstract void SetRenderColor (Color color);

	public abstract void SetLayer(string name);

	public abstract void SetVisibility(float alpha);

}

using UnityEngine;
using System.Collections;

public class Constants {

	public static int CAMERA_DEFAULT_ZOOM = 4;

	public static float CAMERA_MIN_ZOOM = 2f;
	public static float CAMERA_MAX_ZOOM = 13f;

	public static float CAMERA_ZOOM_SPEED = 0.5f;
	public static float CAMERA_ZOOM_DRAG = 0.03f;

	public static float CAMERA_MOVE_SPEED = 0.25f;
	public static float CAMERA_MOVE_DRAG = 10f;

	public static int GRID_MAX_SIZE = 101;

	public static int GRID_SQUARE_RANGE = 3;

	public static int LAZER_MAX_RANGE = 20;
	public static float LAZER_SPEED = 10f;

	public static float EXPLOSION_SHAKE_DURATION = 0.3f;
	public static float EXPLOSION_SHAKE_MAGNITUDE = 0.1f;
	public static float EXPLOSION_CHARGEUP_DURATION = 0.5f;

	public static Color COLOR_TRANSPARENT = Color.white;

	static Constants(){
		COLOR_TRANSPARENT.a = 0;
	}

	/*
	 * LAYERS
	 */

	public static string OBSTACLE_LAYER = "Obstacle";

	/*
	 * TAGS
	 */

	public static string MIRROR_TAG = "Mirror";
}

using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	private float zoomSpeed = Constants.CAMERA_ZOOM_SPEED;
	private float zoomDrag = Constants.CAMERA_ZOOM_DRAG;

	private float currentZoomSpeed = 0.0f;

	private Vector3 oldPosition;

	private Rigidbody2D rigidBody;

	// Use this for initialization
	void Start () {
		Camera.main.orthographicSize = Constants.CAMERA_DEFAULT_ZOOM;

		rigidBody = this.gameObject.GetComponent<Rigidbody2D> ();
		rigidBody.drag = Constants.CAMERA_MOVE_DRAG;
	}
	
	void FixedUpdate () {
		HandleZoom ();
		HandleMove ();
	}

	void HandleZoom ()
	{
		if (Input.GetAxis ("Mouse ScrollWheel") < 0)// back
		{
			currentZoomSpeed = zoomSpeed;
		}
		if (Input.GetAxis ("Mouse ScrollWheel") > 0)// forward
		{
			currentZoomSpeed = -zoomSpeed;
		}
		if (currentZoomSpeed < 0) {
			Camera.main.orthographicSize = Mathf.Max (Camera.main.orthographicSize + currentZoomSpeed, Constants.CAMERA_MIN_ZOOM);
			currentZoomSpeed = Mathf.Min (currentZoomSpeed + zoomDrag, 0);
		} else if (currentZoomSpeed > 0) {
				Camera.main.orthographicSize = Mathf.Min (Camera.main.orthographicSize + currentZoomSpeed, Constants.CAMERA_MAX_ZOOM);
				currentZoomSpeed = Mathf.Max (currentZoomSpeed - zoomDrag, 0);
		}
	}

	void HandleMove ()
	{
		var pos = Input.mousePosition;
		
		if (Input.GetMouseButton (2)) {
			var move = oldPosition - pos;

			rigidBody.velocity = move * Constants.CAMERA_MOVE_SPEED * Camera.main.orthographicSize;
		}

		oldPosition = pos;
	}
}

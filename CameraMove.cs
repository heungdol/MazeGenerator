using UnityEngine;

public class CameraMove : MonoBehaviour {

	private Vector3 desiredPosition;
	private Vector3 desiredPositionVelocity;

	private Camera _camera;

	private float desiredPositionDelay = 0.5f;

	void Start () {
		desiredPosition = transform.position;
		_camera = GetComponent<Camera> ();
	}

	void Update () {
		transform.position = Vector3.SmoothDamp (transform.position, desiredPosition, ref desiredPositionVelocity, desiredPositionDelay);
	}

	public void SetDesiredPosition (int totalWidth, int totalHeight) {
		int biggestLength = (totalWidth > totalHeight) ? totalWidth : totalHeight;
		desiredPosition = new Vector3 (totalWidth / 2, totalHeight / 2, (biggestLength/2) * Mathf.Tan (_camera.fieldOfView/2) * 0.5f);
	}
}

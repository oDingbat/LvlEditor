using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Designer : MonoBehaviour {

	// Created: September 24th 2019 - Thomas Carrella

	public Transform directionals;

	// References
	Camera camera;
	Camera raycastCamera;

	// Variables
	Quaternion rotation = Quaternion.identity;
	Vector3 angularVelocity;
	Vector3 velocity;
	Vector3 position;

	// Camera
	float zoomCurrent = 5;
	float zoomMin = 1f;
	float zoomMax = 10f;
	float zoomIncrement = 0.5f;

	#region // Initial Methods
	private void Start () {
		FetchReferences();

		rotation = transform.rotation;
	}
	private void FetchReferences () {
		// Fetches all references necessary for this script to function
		camera = transform.Find("[Camera]").GetComponent<Camera>();
		raycastCamera = camera.transform.Find("[RaycastCamera]").GetComponent<Camera>();
	}
	#endregion

	#region // Update Methods
	private void Update () {
		UpdateCamera();
		UpdateOther();
	}
	private void UpdateCamera() {
		// Zooming
		if (Input.mouseScrollDelta.y > 0) {
			// Zoom Out
			zoomCurrent = Mathf.Clamp(zoomCurrent - zoomIncrement, zoomMin, zoomMax);
		} else if (Input.mouseScrollDelta.y < 0) {
			// Zoom In
			zoomCurrent = Mathf.Clamp(zoomCurrent + zoomIncrement, zoomMin, zoomMax);
		}

		if (Input.GetMouseButton(2)) {
			if (Input.GetKey(KeyCode.LeftShift)) {
				// Movement
				position += ((camera.transform.right * -Input.GetAxis("Mouse X")) + (camera.transform.up * -Input.GetAxis("Mouse Y"))) * (zoomCurrent * 0.05f);
			} else {
				// Rotation
				rotation = Quaternion.AngleAxis(Input.GetAxis("Mouse X"), Vector3.up) * Quaternion.AngleAxis(-Input.GetAxis("Mouse Y"), camera.transform.right) * rotation;
				angularVelocity = new Vector3(0, 0, 0);
			}
		}

		if (Input.GetMouseButtonUp(2) && Input.GetKey(KeyCode.LeftShift) == false) {
			if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.5f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.5f)
			angularVelocity = new Vector3(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0);
		}

		angularVelocity = Vector3.Lerp(angularVelocity, Vector3.zero, 5 * Time.deltaTime);
		rotation = Quaternion.AngleAxis(angularVelocity.x, Vector3.up) * Quaternion.AngleAxis(angularVelocity.y, camera.transform.right) * rotation;

		// Set Camera
		transform.position = position;
		transform.rotation = rotation;
		camera.orthographicSize = zoomCurrent;
		raycastCamera.orthographicSize = zoomCurrent;
	}
	private void UpdateOther () {
		directionals.transform.position = camera.transform.position + ((camera.transform.right * -1.5f * camera.orthographicSize) + (camera.transform.forward * 5) + (camera.transform.up * -0.8f * camera.orthographicSize));

		directionals.transform.rotation = Quaternion.identity;
		directionals.transform.localScale = Vector3.one * 0.01f * camera.orthographicSize;
	}
	#endregion
	}

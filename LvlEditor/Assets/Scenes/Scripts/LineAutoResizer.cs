using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(LineRenderer))]
public class LineAutoResizer : MonoBehaviour {

	// Created: October 2nd 2019 - Thomas Carrella

	public Camera camera;

	LineRenderer lineRenderer;

	float widthMultiplier = 0.0125f;

	private void Start () {
		lineRenderer = GetComponent<LineRenderer>();
	}
	
	private void Update () {
		lineRenderer.widthMultiplier = camera.orthographicSize * widthMultiplier;
	}


}

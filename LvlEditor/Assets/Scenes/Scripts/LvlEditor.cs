using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LvlEditor : MonoBehaviour {

	// Created: September 24th 2019 - Thomas Carrella
	public LayerMask levelMask;
	public Camera cam;
	public Level[] levels;
	
	Vector3 selectionPositionStart;		// The selection position (To nearest whole numbers) in world position of the selection start
	Vector3 selectionPositionEnd;       // The selection position (To nearest whole numbers) in world position of the selection end
	Vector3 selectionNormalStart;       // The selection normal of the selection start
	Vector3 selectionNormalEnd;          // The selection normal of the selection end

	public Transform indicatorPosStart;
	public Transform indicatorPosEnd;
	public Transform indicatorNormalStart;
	public Transform indicatorNormalEnd;

	private void Update () {
		UpdateSelection();
	}

	private void UpdateSelection () {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			if (CheckMouseRaycast(out hit)) {
				Vector3 faceNormal = new Vector3(Mathf.Round(hit.normal.x), Mathf.Round(hit.normal.y), Mathf.Round(hit.normal.z));
				Vector3 blockPos = hit.point + (hit.normal * 0.5f);
				blockPos = new Vector3(Mathf.Round(blockPos.x), Mathf.Round(blockPos.y), Mathf.Round(blockPos.z));
				Vector3 facePos = blockPos - (faceNormal * 0.5f);
				
				indicatorPosStart.transform.position = blockPos;
				indicatorNormalStart.transform.position = facePos;
				indicatorNormalStart.transform.rotation = Quaternion.LookRotation(faceNormal);
			}
		}

		if (Input.GetMouseButton(0)) {
			RaycastHit hit;
			if (CheckMouseRaycast(out hit)) {
				Vector3 faceNormal = new Vector3(Mathf.Round(hit.normal.x), Mathf.Round(hit.normal.y), Mathf.Round(hit.normal.z));
				Vector3 blockPos = hit.point + (hit.normal * 0.5f);
				blockPos = new Vector3(Mathf.Round(blockPos.x), Mathf.Round(blockPos.y), Mathf.Round(blockPos.z));
				Vector3 facePos = blockPos - (faceNormal * 0.5f);

				indicatorPosEnd.transform.position = blockPos;
				indicatorNormalEnd.transform.position = facePos;
				indicatorNormalEnd.transform.rotation = Quaternion.LookRotation(faceNormal);
			}
		}
	}

	private bool CheckMouseRaycast (out RaycastHit hit) {
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		return (Physics.Raycast(ray, out hit, Mathf.Infinity, levelMask));
	}

}

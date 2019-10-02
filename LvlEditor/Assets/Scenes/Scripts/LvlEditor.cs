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

	// Selection
	Vector3 selectionFaceNormalA;
	Vector3 selectionFaceNormalB;
	Vector3 selectionBlockPosA;
	Vector3 selectionBlockPosB;
	Vector3 selectionFacePosA;
	Vector3 selectionFacePosB;
	Vector3 selectionFaceInwardSelectedBlockPosA;
	public SelectionType selectionType;
	public Vector3 selectionBounds;
	
	public Transform indicatorSelection;
	public LineRenderer[] indicatorFramePieces;

	public GameObject indicatorFramePiecePrefab;

	private void Start () {
		CreateSelectionFramePieces();
	}

	private void CreateSelectionFramePieces () {
		indicatorFramePieces = new LineRenderer[12];

		for (int i = 0; i < 12; i++) {
			indicatorFramePieces[i] = Instantiate(indicatorFramePiecePrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<LineRenderer>();
			indicatorFramePieces[i].transform.GetComponent<LineAutoResizer>().camera = cam;
		}
	}

	private void Update () {
		UpdateSelection();
	}

	private void UpdateSelection () {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			if (CheckMouseRaycast(out hit)) {
				selectionFaceNormalA = new Vector3(Mathf.Round(hit.normal.x), Mathf.Round(hit.normal.y), Mathf.Round(hit.normal.z));
				selectionBlockPosA = hit.point + (hit.normal * 0.5f);
				selectionBlockPosA = new Vector3(Mathf.Round(selectionBlockPosA.x), Mathf.Round(selectionBlockPosA.y), Mathf.Round(selectionBlockPosA.z));
				selectionFacePosA = selectionBlockPosA - (selectionFaceNormalA * 0.5f);
			}
		}

		if (Input.GetMouseButton(0)) {
			RaycastHit hit;
			if (CheckMouseRaycast(out hit)) {
				selectionFaceNormalB = new Vector3(Mathf.Round(hit.normal.x), Mathf.Round(hit.normal.y), Mathf.Round(hit.normal.z));
				selectionBlockPosB = hit.point + (hit.normal * 0.5f);
				selectionBlockPosB = new Vector3(Mathf.Round(selectionBlockPosB.x), Mathf.Round(selectionBlockPosB.y), Mathf.Round(selectionBlockPosB.z));
				selectionFacePosB = selectionBlockPosB - (selectionFaceNormalB * 0.5f);
			}
		}
		
		selectionType = GetSelectionType();
		selectionBounds = GetSelectionBounds();

		// Indicator
		Vector3 indicatorPos;
		Vector3 indicatorScale;
		if (selectionType == SelectionType.Block) {
			indicatorPos = selectionFaceInwardSelectedBlockPosA + (selectionBounds * 0.5f);
			indicatorScale = new Vector3(Mathf.Abs(selectionBounds.x), Mathf.Abs(selectionBounds.y), Mathf.Abs(selectionBounds.z)) + (Vector3.one * 1.001f);	// Make indicator scale equal to absolute value of selectionBounds
		} else {
			indicatorPos = selectionFacePosA + (selectionBounds * 0.5f) + (selectionFaceNormalA * 0.001f);
			indicatorScale = new Vector3(Mathf.Abs(selectionBounds.x), Mathf.Abs(selectionBounds.y), Mathf.Abs(selectionBounds.z)) + new Vector3(selectionFaceNormalA.x == 0 ? 1 : 0, selectionFaceNormalA.y == 0 ? 1 : 0, selectionFaceNormalA.z == 0 ? 1 : 0);
		}
		indicatorSelection.transform.position = indicatorPos;
		indicatorSelection.transform.localScale = indicatorScale;

		// Indicator Frame
		int i = 0;
		Vector3 halfExtends = indicatorScale * 0.5f;
		float lineColWidth = 0.1f;
		float boundsDecrease = 0.0125f;
		halfExtends -= new Vector3(halfExtends.x != 0 ? boundsDecrease : 0, halfExtends.y != 0 ? boundsDecrease : 0, halfExtends.z != 0 ? boundsDecrease : 0);


		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				for (int z = -1; z <= 1; z++) {
					if ((x == 0 && y != 0 && z != 0) || (y == 0 && x != 0 && z != 0) || (z == 0 && y != 0 && x != 0)) {
						Vector3 framePiecePos = indicatorPos;
						Vector3 offset = new Vector3(Mathf.Abs(halfExtends.x) * x, Mathf.Abs(halfExtends.y) * y, Mathf.Abs(halfExtends.z) * z);
						framePiecePos += offset;

						indicatorFramePieces[i].transform.position = framePiecePos;

						Vector3 lineScale = new Vector3(offset.x == 0 ? (halfExtends * 2).x : 0, offset.y == 0 ? (halfExtends * 2).y : 0, offset.z == 0 ? (halfExtends * 2).z : 0);

						indicatorFramePieces[i].transform.position = framePiecePos;
						indicatorFramePieces[i].transform.localScale = lineScale + (Vector3.one * lineColWidth);
						
						indicatorFramePieces[i].SetPosition(0, framePiecePos + (lineScale * 0.5f));
						indicatorFramePieces[i].SetPosition(1, framePiecePos + (lineScale * -0.5f));

						i += 1;
					}
				}
			}
		}
		Debug.Log("i" + i);
		
	}

	private bool CheckMouseRaycast (out RaycastHit hit) {
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		return (Physics.Raycast(ray, out hit, Mathf.Infinity, levelMask));
	}

	public enum SelectionType { Block, Face }

	public SelectionType GetSelectionType () {
		// Figures out what type of selection is currently being done (Face / Block) by checking if face normals are parallel and if the face selections have a differing depth
		SelectionType currentSelectionType;

		bool facesAreParallel = (selectionFaceNormalA == selectionFaceNormalB);
		Vector3 blockOffset = selectionBlockPosA - selectionBlockPosB;
		Vector3 blockOffsetProjectedOntoFaceNormal = Vector3.Project(blockOffset, selectionFaceNormalA);
		bool selectionHasDepth = (blockOffsetProjectedOntoFaceNormal.magnitude != 0);

		if (facesAreParallel && !selectionHasDepth) {     // Check if face normals are parallel && if the selection DOESNT have face normal depth;
			currentSelectionType = SelectionType.Face;
		} else {
			currentSelectionType = SelectionType.Block;
		}
		
		return currentSelectionType;
	}
	public Vector3 GetSelectionBounds () {
		Vector3 newSelectionBounds = Vector3.zero;

		if (selectionType == SelectionType.Block) {
			// Block Selection
			Vector3 blockOffset = selectionBlockPosA - selectionBlockPosB;

			Vector3 blockOffsetProjectedOntoFaceNormalA = Vector3.Project(-blockOffset, selectionFaceNormalA) * 0.5f;
			Vector3 blockOffsetProjectedOntoFaceNormalB = Vector3.Project(blockOffset, selectionFaceNormalB) * 0.5f;

			blockOffsetProjectedOntoFaceNormalA = (blockOffsetProjectedOntoFaceNormalA == Vector3.zero ? selectionFaceNormalA * 0.5f : blockOffsetProjectedOntoFaceNormalA);
			blockOffsetProjectedOntoFaceNormalB = (blockOffsetProjectedOntoFaceNormalB == Vector3.zero ? selectionFaceNormalB * 0.5f : blockOffsetProjectedOntoFaceNormalB);

			Debug.DrawRay(selectionFacePosA, blockOffsetProjectedOntoFaceNormalA, Color.yellow);
			Debug.DrawRay(selectionFacePosB, blockOffsetProjectedOntoFaceNormalB, Color.Lerp(Color.yellow, Color.red, 0.5f));

			Debug.Log("BLOCK OFFSETS: (A: " + blockOffsetProjectedOntoFaceNormalA + ") (B: " + blockOffsetProjectedOntoFaceNormalB + ")");

			Vector3 modifiedBlockSelectionPosA = (selectionFacePosA + blockOffsetProjectedOntoFaceNormalA);
			Vector3 modifiedBlockSelectionPosB = (selectionFacePosB + blockOffsetProjectedOntoFaceNormalB);

			selectionFaceInwardSelectedBlockPosA = modifiedBlockSelectionPosA;

			newSelectionBounds = (modifiedBlockSelectionPosB - modifiedBlockSelectionPosA);

			Debug.Log("SelectionBounds: " + newSelectionBounds);
		} else {
			// Face Selection
			newSelectionBounds = (selectionFacePosB - selectionFacePosA);
		}

		return newSelectionBounds;
	}

}

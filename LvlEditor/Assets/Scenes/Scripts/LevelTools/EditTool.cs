using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditTool : LevelTool {

	[Header("Edit Tool"), Space(10)]
	public LayerMask fullMask;
	public LayerMask selectionMask;
	public LayerMask manipulationMask;
	public LayerMask manipulatorPlanesMask;

	[Header("Manipulator Planes"), Space(10)]
	public MeshCollider manipulatorPlaneX;
	public MeshCollider manipulatorPlaneY;
	public MeshCollider manipulatorPlaneZ;

	// Selection
	Vector3 selectionFaceNormalA;
	Vector3 selectionFaceNormalB;
	Vector3 selectionBlockPosA;
	Vector3 selectionBlockPosB;
	Vector3 selectionFacePosA;
	public Vector3 selectionFacePosB;
	Vector3 selectionFaceInwardSelectedBlockPosA;
	public SelectionType selectionType;
	public enum SelectionType { Block, Face }
	public Vector3 selectionBounds;

	// Manipulation
	Vector3 manipulationPointStart = Vector3.zero;
	Vector3 manipulationSelectionPosInitial = Vector3.zero;
	Vector3 manipulationSelectionPosCurrent = Vector3.zero;
	public Directional manipulationBlockDirectional = Directional.Unspecified;

	// GameObjects
	public Transform indicatorSelection;
	public LineRenderer[] uiSelectionFramePiecesInnerSolid;
	public LineRenderer[] uiSelectionFramePiecesInnerTransparent;
	public LineRenderer[] uiManipulationFramePiecesInnerSolid;
	public LineRenderer[] uiManipulationFramePiecesInnerTransparent;

	[Header("Prefabs"), Space(10)]
	public GameObject uiSelectionFrameSolidPrefab;
	public GameObject uiSelectionFrameTransparentPrefab;
	public GameObject uiManipulationFrameSolidPrefab;
	public GameObject uiManipulationFrameTransparentPrefab;

	Vector3 selectionPositionStart;     // The selection position (To nearest whole numbers) in world position of the selection start
	Vector3 selectionPositionEnd;       // The selection position (To nearest whole numbers) in world position of the selection end
	Vector3 selectionNormalStart;       // The selection normal of the selection start
	Vector3 selectionNormalEnd;          // The selection normal of the selection end

	Vector3 selectionPos;
	Vector3 selectionScale;

	public EditToolMode editToolMode;
	public enum EditToolMode { Selecting, Manipulating }

	#region // Initial Methods
	public override void InitializeTool (LvlEditor _lvlEditor) {
		lvlEditor = _lvlEditor;
		manipulatorPlaneX.gameObject.SetActive(false);
		manipulatorPlaneY.gameObject.SetActive(false);
		manipulatorPlaneZ.gameObject.SetActive(false);
		CreateSelectionFramePieces();
	}
	private void CreateSelectionFramePieces() {
		uiSelectionFramePiecesInnerSolid = new LineRenderer[12];
		uiSelectionFramePiecesInnerTransparent = new LineRenderer[12];
		uiManipulationFramePiecesInnerSolid = new LineRenderer[12];
		uiManipulationFramePiecesInnerTransparent = new LineRenderer[12];

		for (int i = 0; i < 12; i++) {
			uiSelectionFramePiecesInnerSolid[i] = Instantiate(uiSelectionFrameSolidPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<LineRenderer>();
			uiSelectionFramePiecesInnerSolid[i].transform.GetComponent<LineAutoResizer>().camera = lvlEditor.cam;

			uiSelectionFramePiecesInnerTransparent[i] = Instantiate(uiSelectionFrameTransparentPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<LineRenderer>();
			uiSelectionFramePiecesInnerTransparent[i].transform.GetComponent<LineAutoResizer>().camera = lvlEditor.cam;
			
			uiManipulationFramePiecesInnerSolid[i] = Instantiate(uiManipulationFrameSolidPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<LineRenderer>();
			uiManipulationFramePiecesInnerSolid[i].transform.GetComponent<LineAutoResizer>().camera = lvlEditor.cam;

			uiManipulationFramePiecesInnerTransparent[i] = Instantiate(uiManipulationFrameTransparentPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<LineRenderer>();
			uiManipulationFramePiecesInnerTransparent[i].transform.GetComponent<LineAutoResizer>().camera = lvlEditor.cam;
		}
	}
	#endregion

	#region // Input Methods
	public override void GetMouse1Down() {
		RaycastHit hit;
		if (CheckMouseRaycast(out hit, fullMask)) {
			switch (LayerMask.LayerToName(hit.transform.gameObject.layer)) {
				case ("Level"):
					SelectionStart(hit);
					break;
				case ("Manipulators"):
					ManipulationStart(hit);
					break;
			}
			
		}
	}
	public override void GetMouse1() {
		// Figure out which layermask to use
		LayerMask currentLayerMask = fullMask;
		switch (editToolMode) {
			case (EditToolMode.Selecting):
				currentLayerMask = selectionMask;
				break;
			case (EditToolMode.Manipulating):
				currentLayerMask = manipulatorPlanesMask;
				break;
		}
		
		// Check mouse raycast collision
		RaycastHit hit;
		if (CheckMouseRaycast(out hit, currentLayerMask)) {
			switch (editToolMode) {
				case (EditToolMode.Selecting):
					SelectionContinue(hit);
					break;
				case (EditToolMode.Manipulating):
					ManipulationContinue();
					break;
			}
		}


		// Adjust Selection UI
		indicatorSelection.transform.position = selectionPos;
		indicatorSelection.transform.localScale = selectionScale;

		Vector3 uiSelectionFramePosOffset = (selectionType == SelectionType.Block ? Vector3.zero : selectionFaceNormalA * 0.0125f);
		FormWiredCube(selectionPos + uiSelectionFramePosOffset, selectionScale, uiSelectionFramePiecesInnerSolid, -0.025f);
		FormWiredCube(selectionPos + uiSelectionFramePosOffset, selectionScale, uiSelectionFramePiecesInnerTransparent, -0.025f);
	}
	public override void GetMouse1Up() {
		switch (editToolMode) {
			case (EditToolMode.Selecting):
				
				break;
			case (EditToolMode.Manipulating):
				ManipulationEnd();
				break;
		}
	}
	#endregion

	#region // Selection Methods
	private void SelectionStart (RaycastHit hit) {
		editToolMode = EditToolMode.Selecting;

		// Start Selection
		selectionFaceNormalA = new Vector3(Mathf.Round(hit.normal.x), Mathf.Round(hit.normal.y), Mathf.Round(hit.normal.z));
		selectionBlockPosA = hit.point + (hit.normal * 0.5f);
		selectionBlockPosA = new Vector3(Mathf.Round(selectionBlockPosA.x), Mathf.Round(selectionBlockPosA.y), Mathf.Round(selectionBlockPosA.z));
		selectionFacePosA = selectionBlockPosA - (selectionFaceNormalA * 0.5f);

		Debug.Log("Block World Pos: " + selectionBlockPosA);
	}
	private void SelectionContinue(RaycastHit hit) {

		// Selection
		selectionFaceNormalB = new Vector3(Mathf.Round(hit.normal.x), Mathf.Round(hit.normal.y), Mathf.Round(hit.normal.z));
		selectionBlockPosB = hit.point + (hit.normal * 0.5f);
		selectionBlockPosB = new Vector3(Mathf.Round(selectionBlockPosB.x), Mathf.Round(selectionBlockPosB.y), Mathf.Round(selectionBlockPosB.z));
		selectionFacePosB = selectionBlockPosB - (selectionFaceNormalB * 0.5f);

		selectionType = GetSelectionType();
		selectionBounds = GetSelectionBounds();

		if (selectionType == SelectionType.Block) {
			selectionPos = selectionFaceInwardSelectedBlockPosA + (selectionBounds * 0.5f);
			selectionScale = new Vector3(Mathf.Abs(selectionBounds.x), Mathf.Abs(selectionBounds.y), Mathf.Abs(selectionBounds.z)) + (Vector3.one * 1.001f);    // Make indicator scale equal to absolute value of selectionBounds
		} else {
			selectionPos = selectionFacePosA + (selectionBounds * 0.5f) + (selectionFaceNormalA * 0.001f);
			selectionScale = new Vector3(Mathf.Abs(selectionBounds.x), Mathf.Abs(selectionBounds.y), Mathf.Abs(selectionBounds.z)) + new Vector3(selectionFaceNormalA.x == 0 ? 1 : 0, selectionFaceNormalA.y == 0 ? 1 : 0, selectionFaceNormalA.z == 0 ? 1 : 0);
		}
	}
	#endregion

	#region // Manipulation Methods
	private void ManipulationStart(RaycastHit hit) {
		editToolMode = EditToolMode.Manipulating;

		manipulationBlockDirectional = Directional.Unspecified;
		manipulationPointStart = hit.point;
		manipulationSelectionPosInitial = selectionPos;
		manipulationSelectionPosCurrent = manipulationSelectionPosInitial;

		// Adjust Manipulator Planes
		manipulatorPlaneX.transform.position = hit.point;
		manipulatorPlaneY.transform.position = hit.point;
		manipulatorPlaneZ.transform.position = hit.point;

		if (selectionType == SelectionType.Face) {
			if (selectionFaceNormalA.x != 0) {
				manipulatorPlaneX.gameObject.SetActive(false);
				manipulatorPlaneY.gameObject.SetActive(true);
				manipulatorPlaneZ.gameObject.SetActive(true);
			} else if (selectionFaceNormalA.y != 0) {
				manipulatorPlaneX.gameObject.SetActive(true);
				manipulatorPlaneY.gameObject.SetActive(false);
				manipulatorPlaneZ.gameObject.SetActive(true);
			} else if (selectionFaceNormalA.z != 0) {
				manipulatorPlaneX.gameObject.SetActive(true);
				manipulatorPlaneY.gameObject.SetActive(true);
				manipulatorPlaneZ.gameObject.SetActive(false);
			}
		} else {
			manipulatorPlaneX.gameObject.SetActive(true);
			manipulatorPlaneY.gameObject.SetActive(true);
			manipulatorPlaneZ.gameObject.SetActive(true);
		}
	}
	private void ManipulationContinue() {
		RaycastHit hitX;
		RaycastHit hitY;
		RaycastHit hitZ;

		Vector3 manipulationDeltaX = Vector3.zero;
		Vector3 manipulationDeltaY = Vector3.zero;
		Vector3 manipulationDeltaZ = Vector3.zero;

		Directional manipulationDirectional = GetManipulationDirectional();
		Directional cameraWeakestDirectional = GetWeakestDirectionalOfVector(lvlEditor.cam.gameObject.transform.forward);
		Vector3 cameraAbsolute = Vector3Absolute(lvlEditor.cam.gameObject.transform.forward);

		if (selectionType == SelectionType.Block) {
			// Block Selection (Show 2 Manipulator Planes)
			if (manipulationBlockDirectional == Directional.Unspecified) {
				switch (cameraWeakestDirectional) {
					case (Directional.X):
						manipulatorPlaneX.gameObject.SetActive(false);
						manipulatorPlaneY.gameObject.SetActive(true);
						manipulatorPlaneZ.gameObject.SetActive(true);
						break;
					case (Directional.Y):
						manipulatorPlaneX.gameObject.SetActive(true);
						manipulatorPlaneY.gameObject.SetActive(false);
						manipulatorPlaneZ.gameObject.SetActive(true);
						break;
					case (Directional.Z):
						manipulatorPlaneX.gameObject.SetActive(true);
						manipulatorPlaneY.gameObject.SetActive(true);
						manipulatorPlaneZ.gameObject.SetActive(false);
						break;
				}
			} else {
				switch (manipulationDirectional) {
					case (Directional.X):
						manipulatorPlaneX.gameObject.SetActive(true);
						manipulatorPlaneY.gameObject.SetActive(false);
						manipulatorPlaneZ.gameObject.SetActive(false);
						break;
					case (Directional.Y):
						manipulatorPlaneX.gameObject.SetActive(false);
						manipulatorPlaneY.gameObject.SetActive(true);
						manipulatorPlaneZ.gameObject.SetActive(false);
						break;
					case (Directional.Z):
						manipulatorPlaneX.gameObject.SetActive(false);
						manipulatorPlaneY.gameObject.SetActive(false);
						manipulatorPlaneZ.gameObject.SetActive(true);
						break;
				}
			}
		} else {
			// Face Selection (Show 1 Manipulator Plane)
			switch (manipulationDirectional) {
				case (Directional.X):
					manipulatorPlaneX.gameObject.SetActive(true);
					manipulatorPlaneY.gameObject.SetActive(false);
					manipulatorPlaneZ.gameObject.SetActive(false);
					break;
				case (Directional.Y):
					manipulatorPlaneX.gameObject.SetActive(false);
					manipulatorPlaneY.gameObject.SetActive(true);
					manipulatorPlaneZ.gameObject.SetActive(false);
					break;
				case (Directional.Z):
					manipulatorPlaneX.gameObject.SetActive(false);
					manipulatorPlaneY.gameObject.SetActive(false);
					manipulatorPlaneZ.gameObject.SetActive(true);
					break;
			}
		}
		
		if (manipulatorPlaneX.gameObject.activeSelf) {
			if (CheckMouseColliderRaycast(manipulatorPlaneX, out hitX)) {
				manipulationDeltaX = hitX.point - manipulationPointStart;
			}
		}

		if (manipulatorPlaneY.gameObject.activeSelf) {
			if (CheckMouseColliderRaycast(manipulatorPlaneY, out hitY)) {
				manipulationDeltaY = hitY.point - manipulationPointStart;
			}
		} 

		if (manipulatorPlaneZ.gameObject.activeSelf) {
			if (CheckMouseColliderRaycast(manipulatorPlaneZ, out hitZ)) {
				manipulationDeltaZ = hitZ.point - manipulationPointStart;
			}
		}

		manipulationDeltaX = Vector3Round(manipulationDeltaX);
		manipulationDeltaY = Vector3Round(manipulationDeltaY);
		manipulationDeltaZ = Vector3Round(manipulationDeltaZ);
		
		if (selectionType == SelectionType.Face) {
			manipulationDeltaX = Vector3.Project(manipulationDeltaX, selectionFaceNormalA);
			manipulationDeltaY = Vector3.Project(manipulationDeltaY, selectionFaceNormalA);
			manipulationDeltaZ = Vector3.Project(manipulationDeltaZ, selectionFaceNormalA);
		} else if (selectionType == SelectionType.Block && manipulationBlockDirectional != Directional.Unspecified) {
			Vector3 normal = Vector3.zero;
			if (manipulationBlockDirectional == Directional.X) {
				normal = new Vector3(1, 0, 0);
			} else if (manipulationBlockDirectional == Directional.Y) {
				normal = new Vector3(0, 1, 0);
			} else if (manipulationBlockDirectional == Directional.Z) {
				normal = new Vector3(0, 0, 1);
			}
			
			manipulationDeltaX = Vector3.Project(manipulationDeltaX, normal);
			manipulationDeltaY = Vector3.Project(manipulationDeltaY, normal);
			manipulationDeltaZ = Vector3.Project(manipulationDeltaZ, normal);
		}

		Vector3 manipulationDeltaFinal = manipulationDeltaX;
		if (manipulationDeltaY.magnitude > manipulationDeltaFinal.magnitude) {
			manipulationDeltaFinal = manipulationDeltaY;
		}
		if (manipulationDeltaZ.magnitude > manipulationDeltaFinal.magnitude) {
			manipulationDeltaFinal = manipulationDeltaZ;
		}

		// Make sure delta only has 1 direction
		if (manipulationDeltaFinal.x != 0) {
			manipulationDeltaFinal.y = 0;
			manipulationDeltaFinal.z = 0;

			manipulationDeltaFinal.x = Mathf.Sign(manipulationDeltaFinal.x);
		} else if (manipulationDeltaFinal.y != 0) {
			manipulationDeltaFinal.x = 0;
			manipulationDeltaFinal.z = 0;

			manipulationDeltaFinal.y = Mathf.Sign(manipulationDeltaFinal.y);
		} else if (manipulationDeltaFinal.z != 0) {
			manipulationDeltaFinal.x = 0;
			manipulationDeltaFinal.y = 0;

			manipulationDeltaFinal.z = Mathf.Sign(manipulationDeltaFinal.z);
		}


		// Manipulate!
		if (manipulationDeltaFinal.magnitude > 0) {
			Debug.DrawRay(manipulationPointStart, manipulationDeltaFinal, Color.red, 0.125f);

			manipulationPointStart += manipulationDeltaFinal;
			selectionPos += manipulationDeltaFinal;
			manipulationSelectionPosCurrent += manipulationDeltaFinal;

			// If this is block selection and manipulation directional is unspecified, specify it!
			if (manipulationBlockDirectional == Directional.Unspecified && selectionType == SelectionType.Block) {
				if (manipulationDeltaFinal.x != 0) {
					manipulationBlockDirectional = Directional.X;
				} else if (manipulationDeltaFinal.y != 0) {
					manipulationBlockDirectional = Directional.Y;
				} else if (manipulationDeltaFinal.z != 0) {
					manipulationBlockDirectional = Directional.Z;
				}
			}
		}

		Vector3 manipulationDeltaTotal = (manipulationSelectionPosCurrent - manipulationSelectionPosInitial);
		Vector3 manipulationVisualPositionStart = manipulationSelectionPosInitial + (Vector3Multiply(manipulationDeltaTotal.normalized, -selectionScale) * 0.5f);
		Vector3 manipulationVisualPositionEnd = manipulationVisualPositionStart + manipulationDeltaTotal;
		Vector3 manipulationVisualPos = (manipulationVisualPositionStart + manipulationVisualPositionEnd) * 0.5f;
		Vector3 manipulationVisualScale = new Vector3((manipulationDeltaTotal.x == 0 ? selectionScale.x : manipulationDeltaTotal.x), (manipulationDeltaTotal.y == 0 ? selectionScale.y : manipulationDeltaTotal.y), (manipulationDeltaTotal.z == 0 ? selectionScale.z : manipulationDeltaTotal.z));

		Vector3 uiSelectionFramePosOffset = (selectionType == SelectionType.Block ? Vector3.zero : selectionFaceNormalA * 0.0125f);
		FormWiredCube(manipulationVisualPos + uiSelectionFramePosOffset, manipulationVisualScale, uiManipulationFramePiecesInnerSolid, -0.025f);
		FormWiredCube(manipulationVisualPos + uiSelectionFramePosOffset, manipulationVisualScale, uiManipulationFramePiecesInnerTransparent, -0.025f);

		//Debug.DrawRay(manipulationVisualPositionStart, Vector3.one * 0.25f, Color.magenta);
		Debug.DrawLine(manipulationVisualPositionStart, manipulationVisualPositionEnd, Color.magenta);
	}
	private void ManipulationEnd () {
		manipulatorPlaneX.gameObject.SetActive(false);
		manipulatorPlaneY.gameObject.SetActive(false);
		manipulatorPlaneZ.gameObject.SetActive(false);

		FormWiredCube(Vector3.zero, Vector3.zero, uiManipulationFramePiecesInnerSolid, 0);
		FormWiredCube(Vector3.zero, Vector3.zero, uiManipulationFramePiecesInnerTransparent, 0);
	}
	#endregion

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
			Vector3 blockOffsetClamped = selectionBlockPosA - selectionBlockPosB;
			blockOffsetClamped = new Vector3(Mathf.Clamp(blockOffsetClamped.x, -1f, 1f), Mathf.Clamp(blockOffsetClamped.y, -1f, 1f), Mathf.Clamp(blockOffsetClamped.z, -1f, 1f));

			Vector3 blockOffsetProjectedOntoFaceNormalA = Vector3.Project(-blockOffsetClamped, selectionFaceNormalA) * 0.5f;
			Vector3 blockOffsetProjectedOntoFaceNormalB = Vector3.Project(blockOffsetClamped, selectionFaceNormalB) * 0.5f;

			blockOffsetProjectedOntoFaceNormalA = (blockOffsetProjectedOntoFaceNormalA == Vector3.zero ? selectionFaceNormalA * 0.5f : blockOffsetProjectedOntoFaceNormalA);
			blockOffsetProjectedOntoFaceNormalB = (blockOffsetProjectedOntoFaceNormalB == Vector3.zero ? selectionFaceNormalB * 0.5f : blockOffsetProjectedOntoFaceNormalB);

			Debug.DrawRay(selectionFacePosA, blockOffsetProjectedOntoFaceNormalA, Color.yellow);
			Debug.DrawRay(selectionFacePosB, blockOffsetProjectedOntoFaceNormalB, Color.Lerp(Color.yellow, Color.red, 0.5f));
			
			Vector3 modifiedBlockSelectionPosA = (selectionFacePosA + blockOffsetProjectedOntoFaceNormalA);
			Vector3 modifiedBlockSelectionPosB = (selectionFacePosB + blockOffsetProjectedOntoFaceNormalB);

			selectionFaceInwardSelectedBlockPosA = modifiedBlockSelectionPosA;

			newSelectionBounds = (modifiedBlockSelectionPosB - modifiedBlockSelectionPosA);
		} else {
			// Face Selection
			newSelectionBounds = (selectionFacePosB - selectionFacePosA);
		}

		return newSelectionBounds;
	}

	public Directional GetManipulationDirectional () {
		Directional cameraDirectional = GetDirectionalOfVector(lvlEditor.cam.gameObject.transform.forward);
		Directional faceDirectional = (selectionType == SelectionType.Block ? manipulationBlockDirectional : GetDirectionalOfVector(selectionFaceNormalA));
		Vector3 cameraAbsolute = Vector3Absolute(lvlEditor.cam.gameObject.transform.forward);
		
		if (cameraDirectional != faceDirectional) {
			// Not Equal? Just return camera Directional
			return cameraDirectional;
		} else {
			if (cameraDirectional == Directional.X) {
				if (cameraAbsolute.y > cameraAbsolute.z) {
					return Directional.Y;
				} else {
					return Directional.Z;
				}
			} else if (cameraDirectional == Directional.Y) {
				if (cameraAbsolute.x > cameraAbsolute.z) {
					return Directional.X;
				} else {
					return Directional.Z;
				}
			} else if (cameraDirectional == Directional.Z) {
				if (cameraAbsolute.x > cameraAbsolute.y) {
					return Directional.X;
				} else {
					return Directional.Y;
				}
			} else {
				return Directional.X;
			}
		}
	}
	public Directional GetDirectionalOfVector (Vector3 vector) {
		vector = Vector3Absolute(vector);

		Directional newDirectional = Directional.X;

		if (vector.y > vector.x && vector.y > vector.z) {
			newDirectional = Directional.Y;
		}

		if (vector.z > vector.x && vector.z > vector.y) {
			newDirectional = Directional.Z;
		}
		
		return newDirectional;
	}
	public Directional GetWeakestDirectionalOfVector (Vector3 vector) {
		vector = Vector3Absolute(vector);

		Directional newDirectional = Directional.X;

		if (vector.y < vector.x && vector.y < vector.z) {
			newDirectional = Directional.Y;
		}

		if (vector.z < vector.x && vector.z < vector.y) {
			newDirectional = Directional.Z;
		}

		return newDirectional;
	}

	public enum Directional { X, Y, Z, Unspecified }

}

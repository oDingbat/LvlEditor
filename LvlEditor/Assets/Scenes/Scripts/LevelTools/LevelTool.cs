using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelTool : MonoBehaviour {

	[Header("Tool Settings")]
	public string name;
	public LvlEditor lvlEditor;

	public abstract void InitializeTool(LvlEditor lvlEditor);

	public abstract void GetMouse1Down ();
	public abstract void GetMouse1 ();
	public abstract void GetMouse1Up ();

	#region // Level Tool Methods
	public bool CheckMouseRaycast(out RaycastHit hit, LayerMask mask) {
		Ray ray = lvlEditor.cam.ScreenPointToRay(Input.mousePosition);
		return (Physics.Raycast(ray, out hit, Mathf.Infinity, mask));
	}
	public bool CheckMouseColliderRaycast (Collider collider, out RaycastHit hit) {
		Ray ray = lvlEditor.cam.ScreenPointToRay(Input.mousePosition);
		return (collider.Raycast(ray, out hit, Mathf.Infinity));
	}
	public void FormWiredCube(Vector3 pos, Vector3 scale, LineRenderer[] lineRenderers, float scaleOffset) {
		scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));    // Absolute Value scale
		scale += new Vector3(scale.x == 0 ? 0 : scaleOffset, scale.y == 0 ? 0 : scaleOffset, scale.z == 0 ? 0 : scaleOffset);       // Add Scale Offset

		Vector3 halfScale = scale * 0.5f;

		// Forms a wired cube using the provided lineRenderers
		int i = 0;
		float lineColWidth = 0.125f;
		if (lineRenderers.Length == 12) {
			for (int x = -1; x <= 1; x++) {
				for (int y = -1; y <= 1; y++) {
					for (int z = -1; z <= 1; z++) {
						if (IntEqualsXOR(new int[] { x, y, z }, 0)) {
							Vector3 linePos = pos;
							Vector3 offset = new Vector3(Mathf.Abs(halfScale.x) * x, Mathf.Abs(halfScale.y) * y, Mathf.Abs(halfScale.z) * z);
							linePos += offset;

							lineRenderers[i].transform.position = linePos;

							Vector3 lineScale = new Vector3(offset.x == 0 ? (halfScale * 2).x : 0, offset.y == 0 ? (halfScale * 2).y : 0, offset.z == 0 ? (halfScale * 2).z : 0);

							lineRenderers[i].transform.position = linePos;
							lineRenderers[i].transform.localScale = lineScale + (Vector3.one * lineColWidth);

							lineRenderers[i].SetPosition(0, linePos + (lineScale * 0.5f));
							lineRenderers[i].SetPosition(1, linePos + (lineScale * -0.5f));

							i += 1;
						}
					}
				}
			}
		} else {
			Debug.LogError("Error: Incorrect number of LineRenderers in array");
		}
	}
	public bool IntEqualsXOR(int[] values, int desiredValue) {
		// Returns true if only 1 int in array 'values' == 'desiredValue'
		int a = 0;

		for (int i = 0; i < values.Length; i++) {
			if (values[i] == desiredValue) {
				a++;
			}
		}

		return (a == 1);
	}
	public Vector3 Vector3Round (Vector3 vector) {
		return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
	}
	public Vector3 Vector3Absolute(Vector3 vector) {
		return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
	}
	public Vector3 Vector3Multiply (Vector3 vectorA, Vector3 vectorB) {
		return new Vector3(vectorA.x * vectorB.x, vectorA.y * vectorB.y, vectorA.z * vectorB.z);
	}
	#endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LvlEditor : MonoBehaviour {

	// Created: September 24th 2019 - Thomas Carrella

	[Header("References"), Space(10)]
	public Camera cam;
	public Level level;

	[Header("Level Editor Tools"), Space(10)]
	public Transform levelToolsContainer;
	public List<LevelTool> levelTools;
	public LevelTool levelToolCurrent;

	private void Start () {
		// Initialize all of the Level Tools
		levelTools = new List<LevelTool>();

		foreach (Transform t in levelToolsContainer) {
			if (t.GetComponent<LevelTool>()) {
				levelTools.Add(t.GetComponent<LevelTool>());
			}
		}

		foreach (LevelTool levelTool in levelTools) {
			levelTool.InitializeTool(this);
		}

		levelToolCurrent = levelTools[0];
	}

	private void Update () {
		UpdateToolInput();
	}

	private void UpdateToolInput () {
		if (Input.GetMouseButtonDown(0)) {
			levelToolCurrent.GetMouse1Down();
		}

		if (Input.GetMouseButton(0)) {
			levelToolCurrent.GetMouse1();
		}
		
		if (Input.GetMouseButtonUp(0)) {
			levelToolCurrent.GetMouse1Up();
		}
	}
	
}

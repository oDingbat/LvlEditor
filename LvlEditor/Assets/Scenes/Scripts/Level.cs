using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {

	// Created: September 24th, 2019 - Thomas Carrella

	// References
	MeshFilter meshFilter;
	MeshCollider meshCollider;

	public LvlEditor lvlEditor;

	public int[,,] blocks;
	public bool meshUpdatePending;

	private void Start() {
		meshFilter = gameObject.GetComponent<MeshFilter>();
		meshCollider = gameObject.GetComponent<MeshCollider>();

		blocks = new int[32, 32, 32];

		for (int a = 0; a < 32; a++) {
			for (int b = 0; b < 32; b++) {
				for (int c = 0; c < 32; c++) {
					blocks[a, b, c] = 1;
				}
			}
		}

		blocks[15, 15, 15] = 0;
		blocks[15, 15, 16] = 0;
		blocks[15, 16, 15] = 0;
		blocks[15, 16, 16] = 0;
		blocks[16, 15, 15] = 0;
		blocks[16, 15, 16] = 0;
		blocks[16, 16, 15] = 0;
		blocks[16, 16, 16] = 0;
		blocks[15, 15, 17] = 0;
		blocks[15, 17, 15] = 0;
		blocks[15, 17, 17] = 0;
		blocks[17, 15, 15] = 0;
		blocks[17, 15, 17] = 0;
		blocks[17, 17, 15] = 0;
		blocks[17, 17, 17] = 0;
		blocks[16, 16, 17] = 0;
		blocks[16, 17, 16] = 0;
		blocks[16, 17, 17] = 0;
		blocks[17, 16, 16] = 0;
		blocks[17, 16, 17] = 0;
		blocks[17, 17, 16] = 0;
		blocks[17, 17, 17] = 0;

		blocks[15, 16, 17] = 0;
		blocks[16, 15, 17] = 0;
		blocks[17, 16, 15] = 0;
		blocks[15, 17, 16] = 0;
		blocks[16, 17, 15] = 0;
		blocks[17, 15, 16] = 0;

		blocks[15, 15, 18] = 0;
		blocks[15, 16, 18] = 0;
		blocks[15, 17, 18] = 0;
		blocks[14, 15, 15] = 0;
		blocks[14, 15, 16] = 0;
		

		GenerateMesh();
	}

	public void GenerateMesh() {
		meshUpdatePending = false;

		int width = blocks.GetLength(0);
		int height = blocks.GetLength(1);
		int depth = blocks.GetLength(2);

		// Create Mesh Pieces
		Mesh newMesh = new Mesh();
		List<int> triangles = new List<int>();
		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		int vertCount = 0;

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				for (int z = 0; z < depth; z++) {
					if (blocks[x, y, z] != 0) {     // Check if this block is NOT air
													// Top Face

						if (y < height - 1 && blocks[x, y + 1, z] == 0) {
							vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
							vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
							vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
							vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));

							triangles.Add(vertCount + 0);
							triangles.Add(vertCount + 1);
							triangles.Add(vertCount + 2);
							triangles.Add(vertCount + 0);
							triangles.Add(vertCount + 2);
							triangles.Add(vertCount + 3);

							uvs.Add(new Vector2(0, 0));
							uvs.Add(new Vector2(1, 0));
							uvs.Add(new Vector2(1, 1));
							uvs.Add(new Vector2(0, 0));

							vertCount += 4;
						}

						// Bottom Face
						if (y > 0 && blocks[x, y - 1, z] == 0) {
							vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
							vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
							vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
							vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));

							triangles.Add(vertCount + 0);
							triangles.Add(vertCount + 1);
							triangles.Add(vertCount + 2);
							triangles.Add(vertCount + 0);
							triangles.Add(vertCount + 2);
							triangles.Add(vertCount + 3);

							uvs.Add(new Vector2(0, 0));
							uvs.Add(new Vector2(1, 0));
							uvs.Add(new Vector2(1, 1));
							uvs.Add(new Vector2(0, 0));

							vertCount += 4;
						}

						// Left Face
						if (x > 0 && blocks[x - 1, y, z] == 0) {
							vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
							vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
							vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
							vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));

							triangles.Add(vertCount + 0);
							triangles.Add(vertCount + 1);
							triangles.Add(vertCount + 2);
							triangles.Add(vertCount + 0);
							triangles.Add(vertCount + 2);
							triangles.Add(vertCount + 3);

							uvs.Add(new Vector2(0, 0));
							uvs.Add(new Vector2(1, 0));
							uvs.Add(new Vector2(1, 1));
							uvs.Add(new Vector2(0, 0));

							vertCount += 4;
						}

						// Right Face
						if (x < width - 1 && blocks[x + 1, y, z] == 0) {
							vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
							vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
							vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
							vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));

							triangles.Add(vertCount + 0);
							triangles.Add(vertCount + 1);
							triangles.Add(vertCount + 2);
							triangles.Add(vertCount + 0);
							triangles.Add(vertCount + 2);
							triangles.Add(vertCount + 3);

							uvs.Add(new Vector2(0, 0));
							uvs.Add(new Vector2(1, 0));
							uvs.Add(new Vector2(1, 1));
							uvs.Add(new Vector2(0, 0));

							vertCount += 4;
						}

						// Front Face
						if (z > 0 && blocks[x, y, z - 1] == 0) {
							vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
							vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
							vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
							vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));

							triangles.Add(vertCount + 0);
							triangles.Add(vertCount + 1);
							triangles.Add(vertCount + 2);
							triangles.Add(vertCount + 0);
							triangles.Add(vertCount + 2);
							triangles.Add(vertCount + 3);

							uvs.Add(new Vector2(0, 0));
							uvs.Add(new Vector2(1, 0));
							uvs.Add(new Vector2(1, 1));
							uvs.Add(new Vector2(0, 0));

							vertCount += 4;
						}

						// Back Face
						if (z < depth - 1 && blocks[x, y, z + 1] == 0) {
							vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
							vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
							vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
							vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));

							triangles.Add(vertCount + 0);
							triangles.Add(vertCount + 1);
							triangles.Add(vertCount + 2);
							triangles.Add(vertCount + 0);
							triangles.Add(vertCount + 2);
							triangles.Add(vertCount + 3);

							uvs.Add(new Vector2(0, 0));
							uvs.Add(new Vector2(1, 0));
							uvs.Add(new Vector2(1, 1));
							uvs.Add(new Vector2(0, 0));

							vertCount += 4;
						}
					}
				}
			}
		}

		Debug.Log(vertices.Count);

		// Set Mesh
		newMesh.name = "Chunk Mesh";
		newMesh.vertices = vertices.ToArray();
		newMesh.triangles = triangles.ToArray();
		newMesh.uv = uvs.ToArray();
		newMesh.RecalculateNormals();

		meshFilter.sharedMesh = newMesh;
		meshCollider.sharedMesh = newMesh;
	}

	public void ManipulateBlocks(Vector3 blockZeroWorldPos, Vector3 manipulationField, Vector3 manipulationDirection) {
		// Manipulates blocks starting at blockZeroWorldPos through the manipulationField as a for loop, in the direction of manipulationDirection normalized

		// Get Block ZERO coordinates
		BlockCoordinates blockZeroCoords = GetRelativeBlockCoordinates(blockZeroWorldPos);

		// Set Array Sizes
		int[,,] blocksManipulated = new int[Mathf.RoundToInt(manipulationField.x), Mathf.RoundToInt(manipulationField.x), Mathf.RoundToInt(manipulationField.x)];
		int[,,] blocksPushed = new int[Mathf.RoundToInt(manipulationField.x), Mathf.RoundToInt(manipulationField.x), Mathf.RoundToInt(manipulationField.x)];
		int[,,] blocksPulled = new int[Mathf.RoundToInt(manipulationField.x), Mathf.RoundToInt(manipulationField.x), Mathf.RoundToInt(manipulationField.x)];

		// Get Pulled Blocks
		int width = blocksManipulated.GetLength(0);
		int height = blocksManipulated.GetLength(1);
		int depth = blocksManipulated.GetLength(2);
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				for (int z = 0; z < depth; z++) {

				}
			}
		}

		manipulationDirection = manipulationDirection.normalized;   // Make sure this is normalized


		GenerateMesh();
	}

	public void SetBlock (int blockID, Vector3 worldPosition) {
		BlockCoordinates blockCoordinates = GetRelativeBlockCoordinates(worldPosition);

		blocks[blockCoordinates.x, blockCoordinates.y, blockCoordinates.z] = blockID;

		GenerateMesh();
	}

	public BlockCoordinates GetRelativeBlockCoordinates (Vector3 blockWorldPosition) {
		Vector3 relativePosition = blockWorldPosition - transform.position;
		return BlockCoordinates.Vector3ToBlockCoordinates(relativePosition);
	}

	public class BlockCoordinates {
		public int x;
		public int y;
		public int z;

		public BlockCoordinates (int _x, int _y, int _z) {
			x = _x;
			y = _y;
			z = _z;
		}

		public static BlockCoordinates Vector3ToBlockCoordinates (Vector3 pos) {
			return new BlockCoordinates(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
		}
	}

}

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), (typeof(MeshRenderer)))]
public class MarchingSquares : MonoBehaviour
{
	public Material material;
	[Header("Tilemap Settings")]
	[Range(0, 100)]
	public int tilemapSize = 10;
	[Min(0.1f)]
	public float tilemapScale = 1f;
	[Min(0.25f)]
	public float noiseScale = 5f;
	[Header("Offset")]
	public Vector2 step;
	[Min(0.01f)]
	public float delay = 1f;
	[Header("Debug")]
	public bool drawGizmos;
	public bool drawHandles;
	public bool drawVerticies;

	private int[,] weights;
	private Vector3[] vertices;
	private Vector2[] uvs;
	private List<int> triangles;
	private Mesh mesh;
	private float timer;
	private Vector2 offset;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	private void Start()
	{
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();

		mesh = new Mesh
		{
			name = "Mesh"
		};

		weights = new int[tilemapSize + 1, tilemapSize + 1];
		vertices = new Vector3[tilemapSize * tilemapSize * 8];
		triangles = new List<int>();

		Generate();
	}

	private void Update()
	{
		if (step == Vector2.zero)
		{
			return;
		}

		timer += Time.deltaTime;

		if (timer >= delay)
		{
			offset += step;
			Generate();
			timer = 0f;
		}
	}

	private void Generate()
	{
		GenerateWeights();
		GenerateVerticies();
		GenerateTriangles();
		CalculateUVs();

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		meshFilter.mesh = mesh;
		meshRenderer.material = material;

		var center = tilemapSize * tilemapScale / 2f;
		transform.position = new Vector3(-center, 0f, -center);
	}

	private void GenerateWeights()
	{
		for (int i = 0; i < weights.GetLength(0); i++)
		{
			for (int j = 0; j < weights.GetLength(1); j++)
			{
				float weight = Mathf.PerlinNoise(
					offset.x + (i + 1) / noiseScale,
					offset.y + (j + 1) / noiseScale
				);
				weights[i, j] = Mathf.RoundToInt(weight);
			}
		}
	}

	private void GenerateVerticies()
	{
		for (int i = 0; i < weights.GetLength(0) - 1; i++)
		{
			for (int j = 0; j < weights.GetLength(1) - 1; j++)
			{
				Vector3 a = new Vector3(i, 0f, j) * tilemapScale;
				Vector3 b = new Vector3(i, 0f, j + 1f) * tilemapScale;
				Vector3 c = new Vector3(i + 1f, 0f, j + 1f) * tilemapScale;
				Vector3 d = new Vector3(i + 1f, 0f, j) * tilemapScale;

				Vector3 ab = Vector3.Lerp(a, b, 0.5f);
				Vector3 bc = Vector3.Lerp(b, c, 0.5f);
				Vector3 cd = Vector3.Lerp(c, d, 0.5f);
				Vector3 da = Vector3.Lerp(d, a, 0.5f);

				var step = i * tilemapSize * 8 + j * 8;

				vertices[0 + step] = a;
				vertices[1 + step] = ab;
				vertices[2 + step] = b;
				vertices[3 + step] = bc;
				vertices[4 + step] = c;
				vertices[5 + step] = cd;
				vertices[6 + step] = d;
				vertices[7 + step] = da;
			}
		}

		mesh.vertices = vertices;
	}

	private void GenerateTriangles()
	{
		triangles.Clear();

		for (int i = 0; i < weights.GetLength(0) - 1; i++)
		{
			for (int j = 0; j < weights.GetLength(1) - 1; j++)
			{
				var aw = weights[i, j];
				var bw = weights[i, j + 1];
				var cw = weights[i + 1, j + 1];
				var dw = weights[i + 1, j];

				var tile = CalcTile(aw, bw, cw, dw);
				var step = i * tilemapSize * 8 + j * 8;

				switch (tile)
				{
					case 1:
						triangles.Add(step);
						triangles.Add(1 + step);
						triangles.Add(7 + step);
						break;
					case 2:
						triangles.Add(1 + step);
						triangles.Add(2 + step);
						triangles.Add(3 + step);
						break;
					case 3:
						triangles.Add(step);
						triangles.Add(2 + step);
						triangles.Add(7 + step);
						triangles.Add(2 + step);
						triangles.Add(3 + step);
						triangles.Add(7 + step);
						break;
					case 4:
						triangles.Add(3 + step);
						triangles.Add(4 + step);
						triangles.Add(5 + step);
						break;
					case 5:
						triangles.Add(step);
						triangles.Add(1 + step);
						triangles.Add(7 + step);
						triangles.Add(3 + step);
						triangles.Add(4 + step);
						triangles.Add(5 + step);
						break;
					case 6:
						triangles.Add(1 + step);
						triangles.Add(2 + step);
						triangles.Add(4 + step);
						triangles.Add(1 + step);
						triangles.Add(4 + step);
						triangles.Add(5 + step);
						break;
					case 7:
						triangles.Add(step);
						triangles.Add(2 + step);
						triangles.Add(7 + step);
						triangles.Add(2 + step);
						triangles.Add(5 + step);
						triangles.Add(7 + step);
						triangles.Add(2 + step);
						triangles.Add(4 + step);
						triangles.Add(5 + step);
						break;
					case 8:
						triangles.Add(5 + step);
						triangles.Add(6 + step);
						triangles.Add(7 + step);
						break;
					case 9:
						triangles.Add(step);
						triangles.Add(1 + step);
						triangles.Add(5 + step);
						triangles.Add(0 + step);
						triangles.Add(5 + step);
						triangles.Add(6 + step);
						break;
					case 10:
						triangles.Add(1 + step);
						triangles.Add(2 + step);
						triangles.Add(3 + step);
						triangles.Add(5 + step);
						triangles.Add(6 + step);
						triangles.Add(7 + step);
						break;
					case 11:
						triangles.Add(step);
						triangles.Add(2 + step);
						triangles.Add(3 + step);
						triangles.Add(0 + step);
						triangles.Add(3 + step);
						triangles.Add(5 + step);
						triangles.Add(0 + step);
						triangles.Add(5 + step);
						triangles.Add(6 + step);
						break;
					case 12:
						triangles.Add(3 + step);
						triangles.Add(4 + step);
						triangles.Add(6 + step);
						triangles.Add(3 + step);
						triangles.Add(6 + step);
						triangles.Add(7 + step);
						break;
					case 13:
						triangles.Add(step);
						triangles.Add(1 + step);
						triangles.Add(6 + step);
						triangles.Add(1 + step);
						triangles.Add(3 + step);
						triangles.Add(6 + step);
						triangles.Add(3 + step);
						triangles.Add(4 + step);
						triangles.Add(6 + step);
						break;
					case 14:
						triangles.Add(1 + step);
						triangles.Add(2 + step);
						triangles.Add(4 + step);
						triangles.Add(1 + step);
						triangles.Add(4 + step);
						triangles.Add(7 + step);
						triangles.Add(4 + step);
						triangles.Add(6 + step);
						triangles.Add(7 + step);
						break;
					case 15:
						triangles.Add(step);
						triangles.Add(2 + step);
						triangles.Add(4 + step);
						triangles.Add(0 + step);
						triangles.Add(4 + step);
						triangles.Add(6 + step);
						break;
				}
			}
		}

		mesh.triangles = triangles.ToArray();
	}

	private void CalculateUVs()
	{
		uvs = new Vector2[vertices.Length];

		for (int i = 0; i < vertices.Length; i++)
		{
			uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
		}

		mesh.uv = uvs;
	}

	private int CalcTile(int a, int b, int c, int d)
	{
		return a + b * 2 + c * 4 + d * 8;
	}

	private void OnDrawGizmos()
	{
		if (weights == null)
		{
			return;
		}

		for (int i = 0; i < weights.GetLength(0); i++)
		{
			for (int j = 0; j < weights.GetLength(1); j++)
			{
				var center = new Vector3(i * tilemapScale, 0f, j * tilemapScale);
				var weight = weights[i, j];

				if (drawGizmos)
				{
					Gizmos.color = new Color(weight, weight, weight);
					Gizmos.DrawSphere(center, 0.2f);
				}
				
				if (drawHandles)
				{
					Handles.Label(center, $"({i}, {j})");
				}
			}
		}

		if (drawVerticies)
		{
			Gizmos.color = Color.red;

			foreach (var v in mesh.vertices)
			{
				Gizmos.DrawCube(v, Vector3.one * 0.1f);
			}
		}
	}
}

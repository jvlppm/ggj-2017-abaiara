using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Grids;
using System.Linq;

public class Grid : FlatHexGrid<Tile>, IGrid<Tile, FlatHexPoint> {
	static FlatHexPoint[] directions = new FlatHexPoint[] {
		new FlatHexPoint(0, 2),
		new FlatHexPoint(1, 1),
		new FlatHexPoint(1, -1),
		new FlatHexPoint(0, -2),
		new FlatHexPoint(-1, -1),
		new FlatHexPoint(-1, 1),
	};

	public Grid(int width, int height) : base(width, height) {
	}

	public override bool Contains(FlatHexPoint point)
	{
		if (point.X < cells.GetLowerBound(0) ||
			point.X > cells.GetUpperBound(0) ||
			point.Y < cells.GetLowerBound(1) ||
			point.Y > cells.GetUpperBound(1)) return false;
		return this[point];
	}

	public override IEnumerable<FlatHexPoint> GetAllNeighbors(FlatHexPoint point) {
		
		var n = directions
			.Select(d => new FlatHexPoint(point.X + d.X, point.Y + d.Y))
			.Where(Contains)
			.Where(d => d.X >=0 && d.X < width && d.Y >= 0 && d.Y < height)
			.ToArray();

		return n;
	}

	public struct PointDistance {
		public FlatHexPoint point;
		public int distance;
	}

	public IEnumerable<PointDistance> GetAllNeighbors(FlatHexPoint point, int maxDistance)
	{
		List<PointDistance> toProcess = new List<PointDistance>();
		toProcess.Add(new PointDistance { point = point, distance = 0 });

		for (int currentI = 0; currentI < toProcess.Count; currentI++) {
			var current = toProcess[currentI];
			if (current.distance > maxDistance) {
				yield break;
			}
			if (current.distance > 0) {
				yield return current;
			}

			foreach (var p in GetAllNeighbors(current.point)) {
				if (!toProcess.Any(pN => pN.point == p)) {
					toProcess.Add(new PointDistance { point = p, distance = current.distance + 1 });
				}
			}
		}
	}
}

public class TileManager : MonoBehaviour {
	const float tileSizeX = 9.60f;
	const float tileSizeY = 5.55f;

	public Tile hexagonPrefab;

	public Grid grid;

	// Use this for initialization
	void Awake () {

		grid = new Grid(20, 25);

		foreach (var tile in transform.GetComponentsInChildren<Tile>(true)) {
			grid[tile.point] = tile;
			tile.name = string.Format("Tile {0}", tile.point);
		}

		transform.GetComponentsInChildren<Tile>(true)
			.Where(t => grid[t.point] != t)
			.ToList()
			.ForEach(t => Destroy(t.gameObject));

		for (int x = 0; x < 20; x++) {
			for (int y = 0; y < 12; y++) {
				var z = (y * 2 + (x % 2 == 0? 0 : 1));

				if (grid[new FlatHexPoint(x, z)] != null) {
					continue;
				}

#if UNITY_EDITOR
				var tile = (Tile)UnityEditor.PrefabUtility.InstantiatePrefab(hexagonPrefab);
#else
				var tile = Instantiate(hexagonPrefab);
#endif

				tile.transform.position = new Vector3((x - 10) * tileSizeX, 0, (z - 20) * tileSizeY);
				tile.transform.parent = transform;

				tile.x = x;
				tile.z = z;

				grid[new FlatHexPoint(x, z)] = tile;
				tile.name = string.Format("Tile {0}", tile.point);
			}
		}

		// var path = Algorithms.AStar(grid, new FlatHexPoint(0, 0), new FlatHexPoint(19, 23), (a, b) => Distance(a, b), a => true, (a, b) => Distance(a, b));

		// if (path != null) {
		// 	var points = path.ToArray();
		// 	Debug.Log(points.Length + " points!");
		// 	foreach (var p in points) {
		// 		yield return new WaitForSeconds(0.1f);
		// 		grid[p].transform.Translate(new Vector3(0, 2, 0));
		// 	}
		// }
		// else {
		// 	Debug.LogError("Path not found");
		// }
	}

	Vector2 ToVector(FlatHexPoint point)
	{
		return new Vector2(point.X, point.Y);
	}

	float Distance(FlatHexPoint a, FlatHexPoint b)
	{
		var ca = grid[a].transform.position;
		var cb = grid[b].transform.position;
		return Vector3.SqrMagnitude(ca - cb);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

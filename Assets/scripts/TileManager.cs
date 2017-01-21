using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Grids;
using System.Linq;

public class Grid<TCell> : FlatHexGrid<TCell>, IGrid<TCell, FlatHexPoint> {
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
	public override IEnumerable<FlatHexPoint> GetAllNeighbors(FlatHexPoint point) {
		
		var n = directions
			.Select(d => new FlatHexPoint(point.X + d.X, point.Y + d.Y))
			.Where(Contains)
			.Where(d => d.X >=0 && d.X + 1 < width && d.Y >= 0 && d.Y + 1 < height)
			.ToArray();

		return n;
	}

	struct PointDistance {
		FlatHexPoint point;
		public int distance;
	}

	public IEnumerable<FlatHexPoint> GetAllNeighbors(FlatHexPoint point, int maxDistance)
	{
		if (maxDistance <= 0) {
			yield break;
		}
		HashSet<FlatHexPoint> returned = new HashSet<FlatHexPoint>();
		returned.Add(point);
		foreach (var p in GetAllNeighbors(point)) {
			if (returned.Add(p)) {
				yield return p;
			}
			if (maxDistance > 1) {
				foreach (var p2 in GetAllNeighbors(p, maxDistance - 1)) {
					if (returned.Add(p2)) {
						yield return p2;
					}
				}
			}
		}
	}
}

public class TileManager : MonoBehaviour {
	const float tileSizeX = 9.60f;
	const float tileSizeY = 5.55f;

	public Tile hexagonPrefab;

	public Grid<Tile> grid;

	// Use this for initialization
	IEnumerator Start () {

		grid = new Grid<Tile>(20, 25);

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

		yield break;
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

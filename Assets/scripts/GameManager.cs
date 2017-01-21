using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using System;
using Gamelogic.Grids;

public class GameManager : MonoBehaviour {
	public Camera gameCamera;
	public UIManager ui;
	public TileManager map;

	bool moving = false;


	Character _selectedCharacter;
	Character selectedCharacter {
		get { return _selectedCharacter; }
		set {
			_selectedCharacter = value;
			if (ui != null) {
				ui.Selected = value;
			}
			if (value != null) {
				HighlightMovementCells();
			}
		}
	}

	Action<LeanFinger> fingerTapHandler;

	// Use this for initialization
	void Start () {
		LeanTouch.OnFingerTap += OnFingerTap;
	}

	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed.
	/// </summary>
	void OnDestroy()
	{
		LeanTouch.OnFingerTap -= OnFingerTap;
	}

	private void OnFingerTap(LeanFinger finger)
	{
		if (moving) {
			return;
		}

		if (fingerTapHandler != null) {
			fingerTapHandler(finger);
			return;
		}

		var touchRay = finger.GetRay(gameCamera);

		RaycastHit hit;

		if (!Physics.Raycast(touchRay, out hit, 1000))
		{
			selectedCharacter = null;
			return;
		}

		var character = hit.collider.GetComponent<Character>();

		if (character != null) {
			selectedCharacter = character;
		}
		else {
			var tile = hit.collider.GetComponent<Tile>();
			if (tile != null) {
				OnCellTap(tile);
				selectedCharacter = tile.character;
			}
			else {
				selectedCharacter = null;
			}
		}
	}

	void HighlightMovementCells()
	{
		if (moving) {
			return;
		}

		if (!_selectedCharacter) {
			Debug.LogError("No char!");
			return;
		}

		if (!_selectedCharacter.tile) {
			Debug.LogError("No tile!");
			return;
		}

		foreach (var p in map.grid.GetAllNeighbors(_selectedCharacter.tile.point, _selectedCharacter.mp)) {
			var tile = map.grid[p];
			if (tile == null) {
				Debug.LogError("Tile not found: " + p);
				continue;
			}

			tile.SetState(TileState.HighlightMovement);
		}
	}

	void OnCellTap(Tile tile)
	{
		if (moving) {
			return;
		}

		if (tile.state == TileState.HighlightMovement) {
			StartCoroutine(MoveSelectedCharacter(tile));
		}

		foreach (var p in map.grid) {
			var t = map.grid[p];
			if (t == null) {
				continue;
			}

			t.SetState(TileState.Normal);
		}
	}

	IEnumerator MoveSelectedCharacter(Tile tile)
	{
		moving = true;
		var ch = _selectedCharacter;
		ch.tile.character = null;
		var m = ch.MoveTo(tile.transform.position);
		while (m.MoveNext()) {
			yield return m.Current;
		}
		ch.tile = tile;
		tile.character = ch;
		moving = false;
	}
}

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
				HighlightCells(tile.point);
				selectedCharacter = tile.character;
			}
			else {
				selectedCharacter = null;
			}
		}
	}

	void HighlightMovementCells()
	{
		if (!_selectedCharacter) {
			Debug.LogError("No char!");
			return;
		}

		if (!_selectedCharacter.tile) {
			Debug.LogError("No tile!");
			return;
		}

		foreach (var p in map.grid.GetAllNeighbors(_selectedCharacter.tile.point, 3)) {
			var tile = map.grid[p];
			if (!tile) {
				Debug.LogError("Tile not found: " + p);
			}
			else {
				tile.transform.Translate(new Vector3(0, 1, 0));
			}
		}
	}

	void HighlightCells(FlatHexPoint point)
	{
		foreach (var p in map.grid.GetAllNeighbors(point, 1)) {
			var tile = map.grid[p];
			if (!tile) {
				Debug.LogError("Tile not found: " + p);
			}
			else {
				tile.SetState(TileState.HighlightMovement);
			}
		}
	}
}

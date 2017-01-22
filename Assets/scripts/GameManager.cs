using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using System;
using Gamelogic.Grids;
using UI;

public class GameManager : MonoBehaviour, UI.SkillButton.IHandler {
	public Camera gameCamera;
	public UIManager ui;
	public TileManager map;

	bool moving = false;


	[SerializeField] Character _selectedCharacter;
	Character selectedCharacter {
		get { return _selectedCharacter; }
		set {
			_selectedCharacter = value;
			if (ui != null) {
				ui.SetSelection(value);
			}
		}
	}

	Action<LeanFinger> fingerTapHandler;

	// Use this for initialization
	void Start () {
		LeanTouch.OnFingerTap += OnFingerTap;
		selectedCharacter = _selectedCharacter;
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
			return;
		}

		var character = hit.collider.GetComponent<Character>();

		if (character != null) {
			selectedCharacter = character;
			HighlightMovementCells(character);
		}
		else {
			var tile = hit.collider.GetComponent<Tile>();
			if (tile != null) {
				OnCellTap(tile);
			}
		}
	}

	readonly Dictionary<FlatHexPoint, int> distancesCache = new Dictionary<FlatHexPoint, int>();

	void HighlightMovementCells(Character character)
	{
		if (!character) {
			Debug.LogError("No char!");
			return;
		}

		if (!character.tile) {
			Debug.LogError("No tile!");
			return;
		}

		HighlightNeighbors(character.tile, character.mp, TileState.Move);
	}

	void HighlightSkillCells(Character character, Skill skill)
	{
		if (!character) {
			Debug.LogError("No char!");
			return;
		}

		if (!character.tile) {
			Debug.LogError("No tile!");
			return;
		}

		HighlightNeighbors(character.tile, (int)Mathf.Min(skill.ap, character.ap), TileState.Attack);
	}

	public void HighlightNeighbors(Tile tile, int maxDistance, TileState state) {
		foreach (var p in distancesCache) {
			var t = map.grid[p.Key];
			t.SetState(TileState.Normal);
		}

		distancesCache.Clear();
		foreach (var p in map.grid.GetAllNeighbors(tile.point, maxDistance)) {
			distancesCache[p.point] = p.distance;
			var t = map.grid[p.point];
			if (t == null) {
				Debug.LogError("Tile not found: " + p.point);
				continue;
			}

			t.SetState(state);
		}
	}

	void OnCellTap(Tile tile)
	{
		if (moving) {
			return;
		}

		if (tile.state == TileState.Move) {
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
		ch.mp -= distancesCache[tile.point];
		moving = false;

		if (ch == selectedCharacter) {
			if (ui != null) {
				ui.SetSelection(ch);
			}
			HighlightMovementCells(ch);
		}
	}

    void SkillButton.IHandler.OnClick(Skill skill)
    {
		HighlightSkillCells(_selectedCharacter, skill);
    }
}

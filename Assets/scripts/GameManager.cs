using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using System;
using Gamelogic.Grids;
using UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour, UI.SkillButton.IHandler {
	public Camera gameCamera;
	public UIManager ui;
	public TileManager map;

	SkillButton currentSkill;

	bool moving = false;


	[SerializeField] Character _selectedCharacter;
	Character selectedCharacter {
		get { return _selectedCharacter; }
		set {
			if (_selectedCharacter) {
				_selectedCharacter.selected = false;
			}
			_selectedCharacter = value;
			if (_selectedCharacter) {
				_selectedCharacter.selected = true;
			}
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
		if (_selectedCharacter) {
			HighlightMovementCells(_selectedCharacter);
		}
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
		HandleFingerTap(finger);
		if (currentSkill != null) {
			EventSystem.current.SetSelectedGameObject(currentSkill.gameObject, null);
		}
	}

	void HandleFingerTap(LeanFinger finger)
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

		if (!Physics.Raycast(touchRay, out hit, 400))
		{
			return;
		}

		var character = hit.collider.transform.parent.GetComponent<Character>();
		if (character != null) {
			OnCharacterTap(character);
			return;
		}
		var tile = hit.collider.GetComponent<Tile>();
		if (tile != null) {
			OnCellTap(tile);
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

		HighlightNeighbors(character.tile, character.mp, ColorPathCell);
	}

	void HighlightSkillCells(Character character, SkillButton skillButton)
	{
		if (!character) {
			Debug.LogError("No char!");
			return;
		}

		if (!character.tile) {
			Debug.LogError("No tile!");
			return;
		}

		if (character.ap < skillButton.skill.ap)
		{
			Debug.LogError("Not enough points!");
			return;
		}

		currentSkill = skillButton;
		HighlightNeighbors(character.tile, skillButton.skill.range, ColorAttackCell);
	}

	public void HighlightNeighbors(Tile tile, int maxDistance, Action<Tile> coloring) {
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

			coloring(t);
		}
	}

	void ColorAttackCell(Tile tile)
	{
		if (tile.character != null) {
			if (tile.character.team == 0) {
				return;
			}
			tile.SetState(TileState.AttackHighlight);
			return;
		}

		tile.SetState(TileState.Attack);
	}

	void ColorPathCell(Tile tile)
	{
		if (tile.character != null) {
			return;
		}

		tile.SetState(TileState.Move);
	}

	void ResetCellColors()
	{
		foreach (var p in map.grid) {
			var t = map.grid[p];
			if (t == null) {
				continue;
			}

			t.SetState(TileState.Normal);
		}
	}

    void OnCharacterTap(Character character)
	{
		if (character.team != 0) {
			if (distancesCache.ContainsKey(character.tile.point) && character.tile.state == TileState.Attack) {
				Debug.Log("Attack!");
			}
			return;
		}

		currentSkill = null;
		selectedCharacter = character;
		HighlightMovementCells(character);
	}

	void OnCellTap(Tile tile)
	{
		if (moving) {
			return;
		}

		if (tile.state == TileState.Move) {
			ResetCellColors();
			StartCoroutine(MoveSelectedCharacter(tile));
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

    void SkillButton.IHandler.OnClick(SkillButton skill)
    {
		HighlightSkillCells(_selectedCharacter, skill);
    }
}

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

	public Character[] characters;

	SkillButton currentSkill;

	bool moving = false;

	int currentTeam = 0;

	[SerializeField] Character _selectedCharacter;
	Character selectedCharacter {
		get { return _selectedCharacter; }
	}

	Action<LeanFinger> fingerTapHandler;

	// Use this for initialization
	void Start () {
		LeanTouch.OnFingerTap += OnFingerTap;
		SetCurrentTeam(0);
	}

	void SetCurrentTeam(int team)
	{
		currentTeam = team;
		foreach (var p in characters) {
			if (p.team == currentTeam) {
				SetSelection(p);
				break;
			}
		}
	}

	void SetSelection(Character ch)
	{
		currentSkill = null;
		if (_selectedCharacter) {
			_selectedCharacter.selected = false;
		}
		_selectedCharacter = ch;
		if (_selectedCharacter) {
			_selectedCharacter.selected = true;
		}
		HighlightMovementCells(ch);
		if (ui != null && ch) {
			ui.SetSelection(ch);
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
		EventSystem.current.SetSelectedGameObject(currentSkill? currentSkill.gameObject : null, null);
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

		var character = hit.collider.transform.GetComponentInParent<Character>();
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
			if (tile.character.team == currentTeam) {
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
		if (character.team != currentTeam) {
			if (selectedCharacter != null && distancesCache.ContainsKey(character.tile.point) && character.tile.state == TileState.AttackHighlight) {
				AttackCharacter(character);
			}
			return;
		}

		SetSelection(character);
	}

	void AttackCharacter(Character character)
	{
		if (ui) {
			ui.HitAvatar(character);
		}

		selectedCharacter.ap = Mathf.Max(0, selectedCharacter.ap - currentSkill.skill.ap);
		character.hp = Mathf.Max(0, character.hp - currentSkill.skill.damage.GenerateValue());

		ui.Refresh();

		if (currentSkill.skill.ap > selectedCharacter.ap) {
			HighlightMovementCells(selectedCharacter);
		}

		if (character.hp <= 0) {
			character.tile.character = null;
			character.tile.SetState(TileState.Attack);
			Destroy(character.gameObject);
		}
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

		if (tile.character != null) {
			OnCharacterTap(tile.character);
		}
		else {
			SetSelection(_selectedCharacter);
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
		if (moving) {
			EventSystem.current.SetSelectedGameObject(currentSkill? currentSkill.gameObject : null, null);
			return;
		}

		if (currentSkill == skill) {
			currentSkill = null;
			HighlightMovementCells(_selectedCharacter);
			return;
		}

		currentSkill = skill;
		HighlightSkillCells(_selectedCharacter, skill);
    }

	public void OnNextPlayerClick()
	{
		if (moving) {
			return;
		}

		var nextTeam = (currentTeam + 1) % 2;
		if (nextTeam == 0) {
			foreach (var p in characters) {
				p.maxAp = Mathf.Min(10, p.maxAp + 1);
			}
		}

		foreach (var p in characters) {
			if (p && p.team == nextTeam) {
				p.ap += Mathf.Min(5, p.maxAp - p.ap);
				p.mp = p.maxMp;
			}
		}

		SetCurrentTeam(nextTeam);

		
	}
}

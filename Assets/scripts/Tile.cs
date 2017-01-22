using System;
using Gamelogic.Grids;
using UnityEngine;

public enum TileState
{
    Normal,
    Move,
    Attack,
    AttackHighlight
}

[Serializable]
public struct StateMaterial
{
    public TileState state;
    public Material material;
}

public class Tile : MonoBehaviour {
    public int x;
    public int z;

    public StateMaterial[] materials;
    public Material[] teamColors;
    public Material[] selectedColors;

    Character _character;

    public Character character {
        get { return _character; }
        set {
            _character = value;
            RefreshColor();
        }
    }

    public TileState state { get; private set; }

    Renderer view;

    void Awake() {
        view = GetComponentInChildren<Renderer>();
    }

    public FlatHexPoint point {
        get {
            return new FlatHexPoint(x, z);
        }
    }

    public void SetState(TileState state, bool updateView = true) {
        this.state = state;
        if (updateView) {
            RefreshColor();
        }
    }

    public void RefreshColor()
    {
        if (character != null)
        {
            if (character.selected) {
                view.material = selectedColors[character.team];
                return;
            }

            if (state == TileState.Normal) {
                view.material = teamColors[character.team];
                return;
            }
        }

        foreach (var s in materials) {
            if (s.state == state) {
                view.material = s.material;
            }
        }
    }
}

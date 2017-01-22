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
    public Material selectedColor;

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

    public void SetState(TileState state) {
        this.state = state;
        RefreshColor();
    }

    public void RefreshColor()
    {
        if (character != null && character.selected) {
            view.material = selectedColor;
            return;
        }

        if (state == TileState.Normal && character != null && character.team < teamColors.Length) {
            view.material = teamColors[character.team];
            return;
        }

        foreach (var s in materials) {
            if (s.state == state) {
                view.material = s.material;
            }
        }
    }
}

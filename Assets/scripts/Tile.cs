using System;
using Gamelogic.Grids;
using UnityEngine;

public enum TileState
{
    Normal,
    Move,
    Attack
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

    [System.NonSerializedAttribute] public Character character;

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
        foreach (var s in materials) {
            if (s.state == state) {
                view.material = s.material;
            }
        }
        this.state = state;
    }
}

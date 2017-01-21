using Gamelogic.Grids;
using UnityEngine;

public class Tile : MonoBehaviour {
    public int x;
    public int z;

    public Character character;

    public FlatHexPoint point {
        get {
            return new FlatHexPoint(x, z);
        }
    }
}

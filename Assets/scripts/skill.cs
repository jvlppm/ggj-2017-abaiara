using System.Collections;
using UnityEngine;

public class Skill : ScriptableObject {
    public enum Attack {
        Melee, 
        Jump,
        Ranged,
        LocalArea,
        Buff
    };

    [System.Serializable]
    public struct LocalAreaOptions {
        public bool push;
        public float defenseMultiplier;
    }

    public enum Activation
    {
        Button,
        EnemyLowHP
    }

    public Activation activation;
    public int range = 1;
    public float ap;
    public float damage;
    public Sprite icon;
    public Attack type;
    public LocalAreaOptions localAreaOptions;
}

using System;
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

    [System.Serializable]
    public struct Damage {
        public float min;
        public float max;
        public RandomFromDistribution.ConfidenceLevel_e level;

        public float GenerateValue()
        {
            return RandomFromDistribution.RandomRangeNormalDistribution(min, max, level);
        }
    }

    public enum Activation
    {
        Button,
        EnemyLowHP
    }

    public Activation activation;
    public int range = 1;
    public float ap;
    public Damage damage;

    public Sprite icon;
    public Attack type;
    public LocalAreaOptions localAreaOptions;
}

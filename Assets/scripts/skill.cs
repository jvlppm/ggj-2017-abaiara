using System.Collections;
using UnityEngine;

public class Skill : ScriptableObject {
    public enum Attack {
        Slash, 
        Devour
    };

    public float ap;
    public Sprite icon;
    public Attack type;
}

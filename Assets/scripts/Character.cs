using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour {
    public Tile tile;

    public Transform body;

    public int team;

    public float hp;
    public float ap;
    public int mp;

    public float maxHp { get; private set; }
    public float maxAp { get; set; }
    public int maxMp { get; private set; }

    public bool selected {
        get { return _selected; }
        set {
            _selected = value;
            if (tile) {
                tile.RefreshColor();
            }
        }
    }

    public float speed = 1;
    public Sprite avatar;
    public string displayName;
    public Skill[] skills;

    [SerializeField]bool lookingLeft;

    bool _selected;

    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    void OnValidate()
    {
        transform.localScale = lookingLeft? new Vector3(-1, 1, 1) : Vector3.one;
    }
    
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        maxHp = hp;
        maxAp = ap;
        maxMp = mp;
    }

	// Use this for initialization
	void Start () {
        if (tile) {
            tile.character = this;
        }
    }

    void moveTo (Vector3 position)
    {
        transform.position = position;
    }

    public IEnumerator MoveTo(Vector3 b) {
        lookingLeft = b.x < transform.position.x;
        transform.localScale = lookingLeft? new Vector3(-1, 1, 1) : Vector3.one;

        var a = transform.position;
         float step = (speed / (a - b).magnitude) * Time.fixedDeltaTime;
         float t = 0;
         while (t <= 1.0f) {
             t += step; // Goes from 0 to 1, incrementing by step each time
             transform.position = Vector3.Lerp(a, b, t);
             yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
         }
         transform.position = b;
    }

}

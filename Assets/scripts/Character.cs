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
    public float maxAp { get; private set; }
    public float maxMp { get; private set; }

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

    float defaultScaleX;
    Vector3 targetScale;
    bool _selected;
    
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        maxHp = hp;
        maxAp = ap;
        maxMp = mp;
        this.defaultScaleX = body.localScale.x;
        this.targetScale = body.localScale;
    }

	// Use this for initialization
	void Start () {
        if (tile) {
            tile.character = this;
        }
    }
	
	// Update is called once per frame
	void Update () {
		body.localScale = targetScale;//Vector3.Lerp(body.localScale, targetScale, 0.4f);
	}

    void moveTo (Vector3 position)
    {
        transform.position = position;
    }

    public IEnumerator MoveTo(Vector3 b) {
        var defaultScale = new Vector3(
            defaultScaleX,
            body.localScale.y,
            body.localScale.z
        );

        if (b.x < transform.position.x) {
            targetScale = Vector3.Scale(defaultScale, new Vector3(-1, 1, 1));
        }
        else {
            targetScale = defaultScale;
        }
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

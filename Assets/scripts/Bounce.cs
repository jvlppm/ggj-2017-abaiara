using UnityEngine;

public class Bounce : MonoBehaviour {

	public float t = 0;
	public float timesPerSecond = 1;
	public float amplitude = 1;
	public Vector3 direction = new Vector3(0, 1, 0);
	Vector3 basePos;


	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		basePos = transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
		t += Time.deltaTime;
		var sin = Mathf.Sin(t * Mathf.PI * timesPerSecond + Mathf.PI);
		var pos = sin * amplitude - 1;
		transform.localPosition = basePos + direction * pos;
	}
}

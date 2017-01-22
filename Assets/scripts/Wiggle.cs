using UnityEngine;

public class Wiggle : MonoBehaviour {

	public float t = 0;
	public float timesPerSecond = 1;
	public float amplitude = 1;


	public Vector3 maxAngle;
	Vector3 baseRotation;


	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		baseRotation = transform.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
		t += Time.deltaTime;
		var sin = Mathf.Sin(t * Mathf.PI * timesPerSecond + Mathf.PI);
		transform.eulerAngles = baseRotation + (maxAngle - baseRotation) * (sin + 1) / 2;
	}
}

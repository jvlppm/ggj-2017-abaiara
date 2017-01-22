using UnityEngine;

public class Scale : MonoBehaviour {

	public float t = 0;
	public float timesPerSecond = 1;
	public float amplitude = 1;


	public Vector3 minScale = Vector3.one;
	public Vector3 maxScale = Vector3.one;

	void Update () {
		t += Time.deltaTime;
		var sin = Mathf.Sin(t * Mathf.PI * timesPerSecond + Mathf.PI) * amplitude;
		transform.localScale = minScale + (maxScale - minScale) * (sin + 1) / 2;
	}
}

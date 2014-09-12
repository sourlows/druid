using UnityEngine;
using System.Collections;

public class SmoothCamera : MonoBehaviour {

	public float dampTime = 0.4f;
	public float yOffset = 2;
	private Vector3 velocity = Vector3.zero;
	public Transform target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (target)
		{
			Vector3 point = camera.WorldToViewportPoint(target.position);
			Vector3 delta = target.position - camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
			delta.y += yOffset;
			Vector3 destination = transform.position + delta;
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
			//transform.position = Vector3.Lerp(transform.position, destination, dampTime);
		}
	}
}

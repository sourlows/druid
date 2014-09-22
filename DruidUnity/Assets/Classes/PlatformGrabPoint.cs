using UnityEngine;
using System.Collections;

public class PlatformGrabPoint : MonoBehaviour {

	public GameObject platform;

	public bool onRightSideOfPlatform;
	Vector3 thisPosition;

	// Use this for initialization
	void Start () {
		thisPosition = this.GetComponent<Collider>().transform.position;

		if(platform.transform.position.x <= thisPosition.x)
		{
			onRightSideOfPlatform = true;
		}
		else
		{
			onRightSideOfPlatform = false;
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

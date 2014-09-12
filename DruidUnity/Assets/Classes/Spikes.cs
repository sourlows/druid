using UnityEngine;
using System.Collections;

public class Spikes : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnCollisionEnter(Collision entityHit)
	{
		// Check if we hit the player
		if(entityHit.gameObject.tag == "Player")
		{
			DruidController player = (DruidController) entityHit.gameObject.GetComponent<DruidController>();
			player.TakeHit(0.3f, false, false);
			//Debug.Log("Hit player");
		}
	}
}

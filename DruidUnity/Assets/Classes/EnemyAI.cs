using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour {

	public float xMoveSpeed = 3;

	public bool sleeps = false;
	bool sleeping = false;
	public bool chases = true;
	bool chasing = false;
	public bool patrols = true;
	bool patrolling = false;
	float patrolDirection = 1;	// Left = -1, Right = 1

	bool onRightEdge = false;
	bool onLeftEdge = false;

	float pursuitRange = 10;
	float attackRange = 2;
	float activateRange = 4;

	Rigidbody rigidbody;
	GameObject player;

	// Use this for initialization
	void Start () {
		rigidbody = this.GetComponent<Rigidbody>();
		player = GameObject.FindGameObjectWithTag("Player");
		if(patrols)
			patrolling = true;
		else if(sleeps)
			sleeping = true;
	}
	
	// Update is called once per frame
	void Update () {
		float distanceToPlayer = Vector3.Distance(rigidbody.position, player.transform.position);
		// Determine what direction the player is in. 
		// Player is left of enemy = -1
		// Player is right of enemy = 1
		float playerDirection = Mathf.Sign(player.transform.position.x - rigidbody.position.x);

		// If sleeping, check if player is in range to activate the unit
		if(sleeping)
		{
			if(distanceToPlayer < activateRange)
			{
				chasing = true;
				sleeping = false;
			}
			else
			{
				// Sleeping, do nothing
				return;
			}
		}

		if(chasing)
		{
			// If player is too far away, give up the chase and sulk
			if(distanceToPlayer > pursuitRange)
			{
				// Change behaviour to default behaviour
				if(sleeps)
					sleeping = true;
				else if(patrols)
					patrolling = true;
			}
			// Attack player if in range, else move towards player
			else if(distanceToPlayer < attackRange)
			{
				// Attack
				Debug.Log("Mook ATTACK!");
			}
			// Player's not in attack range, continue pursuit!
			else
			{
				// Don't move if we're gonna move off a ledge
				if((playerDirection > 0 && onRightEdge) || (playerDirection < 0 && onLeftEdge))
				{
					rigidbody.velocity = Vector3.zero;
				}
				else
				{
					rigidbody.velocity = new Vector3(xMoveSpeed * playerDirection, rigidbody.velocity.y, 0);
				}
			}
		}
		// If patrolling keep moving in the same direction until we hit a trigger telling us to turn around
		else if(patrolling)
		{
			// Check if we notice the player
			if(distanceToPlayer < activateRange)
			{
				if(chases)
				{
					chasing = true;
				}
			}
			// HAven't noticed the player, stay patrolling
			else
			{
				rigidbody.velocity = new Vector3(xMoveSpeed * patrolDirection, rigidbody.velocity.y, 0);
			}
		}



		//

	}


	void OnTriggerEnter(Collider entity)
	{
		// Check if we're on the edge of a platform
		if(entity.tag == "Right Edge")
			onRightEdge = true;
		else if(entity.tag == "Left Edge")
			onLeftEdge = true;

		if(onLeftEdge || onRightEdge)
		{
			patrolDirection = -patrolDirection;

			if(patrolling)
			{

			}
			// If chasing, decide on some other default behaviour
			else if(chasing)
			{
				if(patrols)
				{
					chasing = false;
					patrolling = true;
				}
				else
				{
					sleeping = true;
				}
			}
		}
	}

	void OnTriggerExit(Collider entity)
	{
		if(entity.tag == "Left Edge")
		{
			onLeftEdge = false;
		}
		else if(entity.tag == "Right Edge")
		{
			onRightEdge = false;
		}
	}
}

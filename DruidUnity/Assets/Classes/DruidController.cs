using UnityEngine;
using System.Collections;

public enum Form { Human, Wolf, Bear } ;


/**
 * Class controlling the movement of the Druid.
 */ 
public class DruidController : Unit 
{
	// Unity can't edit static variables from the inspector, so create a non-static class instead
	/**
	 * Stuff we can do for forms:
	 * Human decent jumping, has height, medium slope tolerance
	 * Wolf: fast horizontal, terrible vertical and air control, low slope tolerance
	 * Bear: heavy slow, bad vertical, as fast as human on horizontal, low slope tolerance
	 * Goat: high vertical, medium horizontal, high amounts of air control, high slope tolerance
	 * 
	 * How much air control do we want?
	 * Sould the player stop nearly instantly when no keys are held, or let velocity continue?
	 * Should it take a while to turn around?
	 * 
	 * Platforming features:
	 * Spike objects (done)
	 * movable blocks (done)
	 * platforms (done)
	 * moving platforms
	 * boulders
	 * ladders
	 * swinging ropes
	 * enemies
	 * switches
	 * triggers
	 * destructible physics objects
	 * fluid
	 */ 
	// Forms //
	[System.Serializable]
	public class HumanForm
	{
		public float Defence = 0.2f;
		public float AttackDamage = 0.4f;
		public Vector2 Size = new Vector2(0.35f, 2);
		public Vector2 CrouchedSize = new Vector2(0.5f, 1);
		public float WalkingSpeed = 8f;
		public float SprintingSpeed = 12f;
		public float MaxGroundAcceleration = 10;
		public float MaxAirAcceleration = 8;
		public float Weight = 1;
		public float PushPower = 2f;	// Used for pushing blocks
		public float Radius = 0.5f;		// How large the collision radius is
		public float SlopeLimit = 30;	// The maximum angle a slope can be before we start sliding off

	}
	public HumanForm human;

	[System.Serializable]
	public class WolfForm
	{
		public float Defence = 0.2f;
		public float AttackDamage = 0.4f;
		public Vector2 Size = new Vector2(1, 2);
		public Vector2 CrouchedSize = new Vector2(1, 1);
		public float WalkingSpeed = 13;
		public float SprintingSpeed = 20;
		public float MaxGroundAcceleration = 20;
		public float Weight = 0.8f;
		public float PushPower = 1f;	// Used for pushing blocks
		public float Radius = 1.5f;		// How large the collision radius is
		public float SlopeLimit = 40;	// The maximum angle a slope can be before we start sliding off
	}
	public WolfForm wolf;

	[System.Serializable]
	public class BearForm
	{
		public float Defence = 0.6f;
		public float AttackDamage = 0.8f;
		public Vector2 Size = new Vector2(1, 2);
		public Vector2 CrouchedSize = new Vector2(1, 1);
		public float WalkingSpeed = 6;
		public float SprintingSpeed = 9;
		public float MaxGroundAcceleration = 5;
		public float Weight = 4f;
		public float PushPower = 6f;	// Used for pushing blocks
		public float Radius = 2f;		// How large the collision radius is
		public float JumpHeight = 0.5f;
		public float SlopeLimit = 20;	// The maximum angle a slope can be before we start sliding off
	}
	public BearForm bear;


	// Current Stats //
	public Vector2 currentVelocity;
	public float Movement;
	public float walkAcceleration = 2600;
	public float sprintAcceleration = 4100f;
	public float AirControlRatio = 0.4f;
	public float walkDeAcceleration = 0.1f;
	public float climbingSpeed = 3400;
	public float maxSlope = 60f;
	public float jumpForce = 400;

	public float airControlXThreshold = 10f;	// If exceeding this amount while in the air, we cannot increase our X velocity
	public float NoKeySlowFactor = 3;

	float walkDeAccelerationVolx;
	float walkDeAccelerationVolz;
	bool canJump = false;
	bool facingRight = true;
	bool climbLedges = true;
	bool hanging = false;
	bool canClimbLadder = false;
	bool onLadder = false;
	bool sprinting = false;
	static bool grounded = false;
	float maxWalkSpeed = 10f;
	float maxSprintSpeed = 15f;
	Vector2 horizontalMovement;
	int jumpCooldown = 40;	// Must wait this many ticks before jumping again
	int jumpCounter = 0;
		
	private Form CurrentForm = Form.Human;

	public float AttackDamage = 0.4f;

	public Vector2 Size = new Vector2(0.35f, 2);	// Size is radius, then height
	public Vector2 CrouchedSize = new Vector2(1, 1);
	private bool crouched = false;
	
	public GUIText HPText;
	public GameObject model;
	public Animator animator;
	private CapsuleCollider collisionBox;	// Used to change the shape of the character for collisions


	private PlatformGrabPoint handHold;		// Set to the point we're holding onto when hanging

	// Use this for initialization
	void Start () {
		// Classes used to contain stats
		human = new HumanForm();
		wolf = new WolfForm();
		bear = new BearForm();

		collisionBox = this.GetComponent<CapsuleCollider>();

		CurrentForm = Form.Human;
		transformToHuman();
	}


	void OnCollisionEnter()
	{

	}


	// Transformation methods that change the base stats for our character
	public void transformToHuman()
	{
		Debug.Log("Transforming to human");
		CurrentForm = Form.Human;

		Size = human.Size;
		CrouchedSize = human.CrouchedSize;

		AttackDamage = human.AttackDamage;
		Defence = human.Defence;
	}
	public void transformToWolf()
	{
		Debug.Log("Transforming to wolf");
		CurrentForm = Form.Wolf;

		AttackDamage = wolf.AttackDamage;
		Defence = wolf.Defence;
	}
	public void transformToBear()
	{
		Debug.Log("Transforming to bear");
		CurrentForm = Form.Bear;

		AttackDamage = bear.AttackDamage;
		Defence = bear.Defence;
	}


	public void crouch() 
	{
		collisionBox.radius = CrouchedSize.x;
		collisionBox.height = CrouchedSize.y;
		crouched = true;
	}
	public void uncrouch()
	{
		collisionBox.radius = Size.x;
		collisionBox.height = Size.y;
		crouched = false;
	}


	public override void TakeHit(float incomingDamage, bool lethal, bool ignoreInvulnerabilityPeriod)
	{
		// If Defence = 0.2f, then 20% of the incoming damage is negated, or 80% is let through
		// The higher the Defence, the better
		float actualDamage = incomingDamage - (incomingDamage * Defence);
		
		HP = Mathf.Max((HP - actualDamage), 0);	// HP cannot fall below 0
		if(HP == 0)
		{
			// HP has reached 0. Druid has died
			Die();
		}
	}


	public override void Die ()
	{
		base.Die ();

		Debug.Log("Druid has died");
	}


	public void AnimateDruid()
	{
		if(animator)
		{
			//get the current state
			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

			//if we're in "Run" mode, respond to input for jump, and set the Jump parameter accordingly. 
			if(Input.GetButton("Jump")) 
			{
				animator.SetBool("Jump", true );
			}
			else
			{
				animator.SetBool("Jump", false);                
			}
			
			if(Input.GetButton("Horizontal"))
			{
				animator.SetBool("Walk", true);
			}
			else
			{
				animator.SetBool("Walk", false);
			}
			
			if(Input.GetButton("Sprint"))
			{
				animator.SetBool("Run", true);
			}
			else
			{
				animator.SetBool("Run", false);
			}
		}
	}


	/**
	 * Handles movement of our rigidbody character.
	 */ 
	void FixedUpdate (){ 
		currentVelocity = rigidbody.velocity;
		float horizontalMovement = rigidbody.velocity.x;
		float verticalMovement = rigidbody.velocity.y;

		if(grounded)
		{
			if(sprinting)
			{
				if(Mathf.Abs(horizontalMovement) > maxSprintSpeed)
				{
					//horizontalMovement = horizontalMovement.normalized;
					horizontalMovement -= 0.1f;
				}
			}
			else
			{
				if(Mathf.Abs(horizontalMovement) > maxWalkSpeed)
				{
					//horizontalMovement = horizontalMovement.normalized;
					horizontalMovement -=0.1f;
				}
			}
		}
		else if (hanging)
		{
			// Don't stop vertical movement if we're hanging so we don't slide down
			verticalMovement = 0;
		}
		
		rigidbody.velocity = new Vector3(horizontalMovement, verticalMovement, 0);

		// Applies friction from the ground, slowing the model on the X axis
		if(grounded)
		{
			float temp1x = Mathf.SmoothDamp(rigidbody.velocity.x, 0, ref walkDeAccelerationVolx, walkDeAcceleration);

			rigidbody.velocity = new Vector3 (temp1x,rigidbody.velocity.y, 0);
		}
		// Apply friction to slow process of moving up and down the ledge
		else if (hanging)
		{
			float temp1y = Mathf.SmoothDamp(0, rigidbody.velocity.y, ref walkDeAccelerationVolx, walkDeAcceleration);
			temp1y = Mathf.Min(temp1y, 5);
			rigidbody.velocity = new Vector3 (0, temp1y, 0);
		}
		
		
		//transform.rotation = Quaternion.Euler(0f, cameraObject.GetComponent<MouseLookScript>().currentYRotation, 0f);

		float horizontalInput = Input.GetAxis("Horizontal");
		float verticalInput = Input.GetAxis("Vertical");

		// Set forces that will be applied to the rigidbody that will move the character
		float xForce = 0;
		float yForce = 0;
	
		// Jumping //

		if(jumpCounter > 0)
		{
			jumpCounter++;

			if(jumpCounter > jumpCooldown)
			{
				jumpCounter = 0;
			}
		}
		// Add an upward Y force if jumping
		float y = 0;
		if(grounded && !hanging && Input.GetButton("Jump") && jumpCounter == 0)
		{
			jumpCounter++;
			yForce = jumpForce;
		}

		// Sprint if on the ground and hitting sprint
		if(grounded && sprinting)
		{
			xForce = sprintAcceleration;
		}
		else
		{
			xForce = walkAcceleration;
		}

		// Move up and down if hanging
		if(hanging)
		{
			yForce = verticalInput * climbingSpeed;
		}

		// Check if the player wants to move left or right
		if(horizontalInput == 0)
		{
			if(!grounded)
			{
				// In the air, with no horizontal input, slow the player down
				float temp1x = Mathf.SmoothDamp(rigidbody.velocity.x, 0, ref walkDeAccelerationVolx, 1f);

				rigidbody.velocity = new Vector3 (temp1x,rigidbody.velocity.y, 0);

				// In the air, with no horizontal input, slow the player down
				/*
				Vector2 slowedVelocity = rigidbody.velocity;
				slowedVelocity.x = slowedVelocity.x / 3;
				rigidbody.velocity = slowedVelocity;
				*/
			}
		}
		// Character has horizontal input
		else
		{
			// Make the character face the way the player is trying to move in
			if(horizontalInput > 0)
			{
				if(!facingRight)	// Face right if we're moving right and aren't facing right
				{
					facingRight = true;
					model.transform.Rotate(new Vector3(0, 180));
				}

				// Don't allow the character to accelerate past a certain point while in the air
				if(!grounded && rigidbody.velocity.x >= airControlXThreshold)
				{
					xForce = 0;
				}
			}
			else if(horizontalInput < 0)
			{
				if(facingRight)	// Face left if we're moving left and haven't faced left yet
				{
					facingRight = false;
					model.transform.Rotate(new Vector3(0, 180));
				}

				// Don't allow the character to accelerate past a certain point while in the air
				if(!grounded && rigidbody.velocity.x <= -airControlXThreshold)
				{
					xForce = 0;
				}
			}
		}

		// Add relative forces to move the character based on their input
		// Player is on the ground
		if (grounded)
		{
			rigidbody.AddRelativeForce(horizontalInput * xForce * Time.deltaTime, yForce, 0);//Input.GetAxis("Vertical") * walkAcceleration * Time.deltaTime);
		}
		// Player is hanging on a ledge
		else if(hanging)
		{
			rigidbody.AddRelativeForce(0, yForce * Time.deltaTime, 0);
		}
		// Player is in the air
		else
		{

			rigidbody.AddRelativeForce(horizontalInput * xForce * AirControlRatio * Time.deltaTime, 0, 0);//Input.GetAxis("Vertical") * walkAcceleration * walkAccelAirRatio* Time.deltaTime);        
		}
	}
	
	
	// Update is called once per frame
	public override void Update () {
		base.Update();

		// Hanging onto ledges
		if(handHold != null)
		{
			// There's a potential handhold, grab it if the input is going in the right direction
			// We're on the right side of the platform while pushing left
			if(handHold.onRightSideOfPlatform && Input.GetAxis("Horizontal") < 0)
			{
				if(!hanging)
				{
					Debug.Log("right");
					hanging = true;
					rigidbody.useGravity = false;
					rigidbody.velocity = Vector3.zero;
					//rigidbody.isKinematic = false;
				}
			}
			// We're on the left side of the platform while pushing right
			else if(!handHold.onRightSideOfPlatform && Input.GetAxis("Horizontal") > 0)
			{
				if(!hanging)
				{
					Debug.Log("left");
					hanging = true;
					rigidbody.useGravity = false;
					rigidbody.velocity = Vector3.zero;
				}
			}
			// No input, stop hanging
			else
			{
				// Check if we should drop off the ledge
				// Zero out the Y velocity so the character doesn't fly up the ledge when they let go
				if(hanging)
				{
					if(handHold.onRightSideOfPlatform && Input.GetAxis("Horizontal") > 0)
					{
						Vector3 v = rigidbody.velocity;
						v.y = 0;
						rigidbody.velocity = v;
						hanging = false;
						rigidbody.useGravity = true;
					}
					else if(!handHold.onRightSideOfPlatform && Input.GetAxis("Horizontal") < 0)
					{
						Vector3 v = rigidbody.velocity;
						v.y = 0;
						rigidbody.velocity = v;
						hanging = false;
						rigidbody.useGravity = true;
					}
				}
			}
		}

		if(canClimbLadder && !onLadder && Input.GetAxis("Vertical") > 0)
		{
			hanging = true;
			onLadder = true;
			rigidbody.useGravity = false;
		}
		else if (onLadder && Input.GetButton("Jump"))
		{
			// Stop climbing the ladder if the player hits jump
			stopHanging();
		}

		// Check if we're crouching
		if(crouched)
		{
			// The raycast checks if we'll oollide with something above us if we uncrouch
			if(!Input.GetButton("Crouch") && !Physics.Raycast(collider.rigidbody.position, Vector3.up, Size.y))
			{
				// Uncrouch if we were crouched before and let go of the button
				uncrouch();
			}
		}
		else if(Input.GetButton("Crouch") && grounded && !hanging)
		{
			// Crouch if they hit the button and we aren't crouched already
			crouch();
		}

		// Check for transformations
		if(Input.GetButtonDown("TransformHuman"))// && CurrentForm != Form.Human)
		{
			transformToHuman();
		}
		else if(Input.GetButtonDown("TransformWolf"))// && CurrentForm != Form.Human)
		{
			transformToWolf();
		}
		else if(Input.GetButtonDown("TransformBear") && CurrentForm != Form.Human)
		{
			transformToBear();
		}

		if(!crouched)
		{
			sprinting = Input.GetButton("Sprint");
		}
		else
		{
			sprinting = false;
		}

		// Set animations
		AnimateDruid();

		HPText.text = "HP: " + HP;
	}

	/**
	 * Called when we enter a collider with the isTrigger flag
	 */ 
	void OnTriggerEnter(Collider entity)
	{
		if(entity.gameObject.tag == "Ledge Trigger")
		{
			// We are in a grab point region. Set our handHold variable to indicate we could grab it
			PlatformGrabPoint grabPoint = (PlatformGrabPoint) entity.gameObject.GetComponent<PlatformGrabPoint>();
			handHold = grabPoint;
		}
		if(entity.gameObject.tag == "Ladder")
		{
			canClimbLadder = true;
		}
	}


	void OnTriggerExit(Collider entity)
	{
		if(entity.gameObject.tag == "Ledge Trigger")
		{
			stopHanging();
		}
		else if(entity.gameObject.tag == "Ladder")
		{
			stopHanging();
		}
	}


	void stopHanging()
	{
		// If we were hanging before, remove all momentum so we don't pop up over the ledge
		if(hanging)
		{
			Vector3 v = rigidbody.velocity;
			v.y = 1;
			rigidbody.velocity = v;
		}

		canClimbLadder = false;
		onLadder = false;
		hanging = false;
		handHold = null;
		rigidbody.useGravity = true;
	}


	void OnCollisionStay (Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts)
		{
			if (Vector3.Angle(contact.normal, Vector3.up) < maxSlope)
			{
				canJump = true;
				grounded = true;
			}
		}
	}
	void OnCollisionExit (){
		canJump = false;
		grounded = false;
	}
}

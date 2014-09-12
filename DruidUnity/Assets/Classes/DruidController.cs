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
	 */ 
	// Forms //
	[System.Serializable]
	public class HumanForm
	{
		public float Defence = 0.2f;
		public float AttackDamage = 0.4f;
		public Vector2 Size = new Vector2(1, 2);
		public Vector2 CrouchedSize = new Vector2(1, 1);
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

	public float Movement;
	public float walkAcceleration = 1600f;
	public float walkAccelAirRatio = 0f;
	public float walkDeAcceleration = 0.3f;
	public int accelerationController = 1;
	public float maxSlope = 60f;
	public float jumpForce = 8000;

	float walkDeAccelerationVolx;
	float walkDeAccelerationVolz;
	bool canJump = false;
	static bool grounded = false;
	float maxWalkSpeed = 10f;
	Vector2 horizontalMovement;
		
	private Form CurrentForm = Form.Human;

	public float AttackDamage = 0.4f;

	public Vector2 Size = new Vector2(1, 2);
	public Vector2 CrouchedSize = new Vector2(1, 1);
	private bool crouched = false;

	private BoxCollider collisionBox;	// USed to change the shape of the character for collisions

	public GUIText HPText;

	public Animator animator;

	// Use this for initialization
	void Start () {
		// Classes used to contain stats
		human = new HumanForm();
		wolf = new WolfForm();
		bear = new BearForm();

		collisionBox = this.GetComponent<BoxCollider>();

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
		collisionBox.size = new Vector3(CrouchedSize.x, CrouchedSize.y, 1);
		crouched = true;
	}
	public void uncrouch()
	{
		collisionBox.size = new Vector3(Size.x, Size.y, 1);
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
		
		horizontalMovement = new Vector2(rigidbody.velocity.x, rigidbody.velocity.z);
		
		if(horizontalMovement.magnitude > maxWalkSpeed){
			horizontalMovement = horizontalMovement.normalized;
			horizontalMovement *= maxWalkSpeed;
		}
		
		rigidbody.velocity = new Vector3(horizontalMovement.x, rigidbody.velocity.y, 0);

		// Applies friction from the ground, slowing the model on the X axis
		if(grounded)
		{
			float temp1x = Mathf.SmoothDamp(rigidbody.velocity.x, 0, ref walkDeAccelerationVolx, walkDeAcceleration);

			rigidbody.velocity = new Vector3 (temp1x,rigidbody.velocity.y, 0);
		}
		
		
		//transform.rotation = Quaternion.Euler(0f, cameraObject.GetComponent<MouseLookScript>().currentYRotation, 0f);

		// Add an upward Y force if jumping
		float y = 0;
		if(Input.GetButton("Jump"))
		{
			y = jumpForce;
		}

		float horizontalInput = Input.GetAxis("Horizontal");
		// Check if the player wants to move left or right
		if(horizontalInput == 0)
		{
			if(grounded)
			{
				rigidbody.AddRelativeForce(0, y, 0);
			}
		}
		else
		{
			if (grounded)
			{
				// Player is on the ground and wants to move left or right
				rigidbody.AddRelativeForce(horizontalInput * walkAcceleration * Time.deltaTime, y, 0);//Input.GetAxis("Vertical") * walkAcceleration * Time.deltaTime);
			}
			else
			{
				// Player is in the air and wants to move left or right
				rigidbody.AddRelativeForce(horizontalInput * walkAcceleration * walkAccelAirRatio * Time.deltaTime, 0, 0);//Input.GetAxis("Vertical") * walkAcceleration * walkAccelAirRatio* Time.deltaTime);        
			}
		}


	}
	
	void OnCollisionStay (Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts){
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

	
	// Update is called once per frame
	public override void Update () {
		base.Update();

		// Check if we're crouching
		if(crouched)
		{
			if(!Input.GetButton("Crouch"))
			{
				// Uncrouch if we were crouched before and let go of the button
				uncrouch();
			}
		}
		else if(Input.GetButton("Crouch"))
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

		// Set animations
		AnimateDruid();

		HPText.text = "HP: " + HP;
	}
}

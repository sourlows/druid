using UnityEngine;
using System.Collections;

public enum Form { Human, Wolf, Bear } ;

/**
 * Class controlling the movement of the Druid.
 */ 
public class DruidController : MonoBehaviour {

	// Unity can't edit static variables from the inspector, so create a non-static class instead

	// Forms //
	[System.Serializable]
	public class HumanForm
	{
		public float Defence = 0.2f;
		public float AttackDamage = 0.4f;
		public float Height = 1f;
		public float CrouchHeight = 0.5f;
		public float WalkingSpeed = 8f;
		public float SprintingSpeed = 12f;
		public float MaxGroundAcceleration = 10;
		public float MaxAirAcceleration = 8;
		public float Weight = 1;
		public float PushPower = 2f;	// Used for pushing blocks
		public float Radius = 0.5f;		// How large the collision radius is
		public float JumpHeight = 2f;
		public float SlopeLimit = 30;	// The maximum angle a slope can be before we start sliding off

	}
	public HumanForm human;

	[System.Serializable]
	public class WolfForm
	{
		public float Defence = 0.2f;
		public float AttackDamage = 0.4f;
		public float Height = 0.6f;
		public float CrouchHeight = 0.4f;
		public float WalkingSpeed = 13;
		public float SprintingSpeed = 20;
		public float MaxGroundAcceleration = 20;
		public float Weight = 0.8f;
		public float PushPower = 1f;	// Used for pushing blocks
		public float Radius = 1.5f;		// How large the collision radius is
		public float JumpHeight = 1f;
		public float SlopeLimit = 40;	// The maximum angle a slope can be before we start sliding off
	}
	public WolfForm wolf;

	[System.Serializable]
	public class BearForm
	{
		public float Defence = 0.6f;
		public float AttackDamage = 0.8f;
		public float Height = 1f;
		public float CrouchHeight = 0.7f;
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
	private Form CurrentForm = Form.Human;
	public float HP = 1;	// A value between 0 and 1. 0 or less means death
	public float Defence = 0.2f;	// A value that determines how much of the damage is absorbed.		
									// Ex: 0.2 means the damage is reduced by 20%
	public float AttackDamage = 0.4f;

	private float normalHeight;
	private float crouchHeight;

	public GUIText HPText;

	public Animator animator;

	private CharacterMotor motor;
	private CharacterController controller;

	// Use this for initialization
	void Start () {
		motor = GetComponent<CharacterMotor>();
		controller = GetComponent<CharacterController>();

		CurrentForm = Form.Human;

		// Classes used to contain stats
		human = new HumanForm();
		wolf = new WolfForm();
		bear = new BearForm();
	}


	void OnCollisionEnter()
	{

	}


	// Transformation methods that change the base stats for our character
	public void transformToHuman()
	{
		Debug.Log("Transforming to human");
		CurrentForm = Form.Human;

		AttackDamage = human.AttackDamage;
		Defence = human.Defence;

		controller.slopeLimit = human.SlopeLimit;

		motor.movement.maxHorizontalSpeed = human.WalkingSpeed;
		motor.movement.maxGroundAcceleration = human.MaxGroundAcceleration;
		motor.movement.pushPower = human.PushPower;
		
		motor.jumping.baseHeight = human.Height;
		motor.jumping.extraHeight = human.JumpHeight;
		
		normalHeight = human.Height;
		crouchHeight = human.CrouchHeight;
	}
	public void transformToWolf()
	{
		Debug.Log("Transforming to wolf");
		CurrentForm = Form.Wolf;

		AttackDamage = wolf.AttackDamage;
		Defence = wolf.Defence;

		controller.slopeLimit = wolf.SlopeLimit;

		motor.movement.maxHorizontalSpeed = wolf.WalkingSpeed;
		motor.movement.maxGroundAcceleration = wolf.MaxGroundAcceleration;
		motor.movement.pushPower = wolf.PushPower;

		motor.jumping.baseHeight = wolf.Height;
		motor.jumping.extraHeight = wolf.JumpHeight;

		normalHeight = wolf.Height;
		crouchHeight = wolf.CrouchHeight;
	}
	public void transformToBear()
	{
		Debug.Log("Transforming to bear");
		CurrentForm = Form.Bear;

		AttackDamage = bear.AttackDamage;
		Defence = bear.Defence;

		controller.slopeLimit = bear.SlopeLimit;

		motor.movement.maxHorizontalSpeed = bear.WalkingSpeed;
		motor.movement.maxGroundAcceleration = bear.MaxGroundAcceleration;
		motor.movement.pushPower = bear.PushPower;
		
		motor.jumping.baseHeight = wolf.Height;
		motor.jumping.extraHeight = bear.JumpHeight;
		
		normalHeight = bear.Height;
		crouchHeight = bear.CrouchHeight;
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


	public void TakeHit(float incomingDamage)
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

	
	// Update is called once per frame
	void Update () {
		// Check if we're crouching
		if(Input.GetButton("Crouch"))
		{
			controller.height = 1f;
		}
		else
		{
			controller.height = 2;
		}

		// Check for transformations
		if(Input.GetButtonDown("TransformHuman"))// && CurrentForm != Form.Human)
		{
			Debug.Log("hi");
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

		// Get the input vector from keyboard or analog stick
		var directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, 0);//Input.GetAxis("Vertical"));
		
		if (directionVector != Vector3.zero) 
		{
			// Get the length of the directon vector and then normalize it
			// Dividing by the length is cheaper than normalizing when we already have the length anyway
			var directionLength = directionVector.magnitude;
			directionVector = directionVector / directionLength;
			
			// Make sure the length is no bigger than 1
			directionLength = Mathf.Min(1, directionLength);
			
			// Make the input vector more sensitive towards the extremes and less sensitive in the middle
			// This makes it easier to control slow speeds when using analog sticks
			directionLength = directionLength * directionLength;
			
			// Multiply the normalized direction vector by the modified length
			directionVector = directionVector * directionLength;	
		}

		// Apply the direction to the CharacterMotor
		motor.inputMoveDirection = transform.rotation * directionVector;
		motor.inputJump = Input.GetButton("Jump");

		// Set animations
		AnimateDruid();

		HPText.text = "HP: " + HP;
	}
}

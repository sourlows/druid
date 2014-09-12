using UnityEngine;
using System.Collections;

/**
 * Basic unit class. Has the TakeHit method
 */ 
public class Unit : MonoBehaviour {

	public float Defence = 0;	// What percentage of incoming damage is negated. 
								// Ex: 0.20f negates 20% of incoming damage
	public float HP = 1;	// Value between 0 and 1. 0 means dead.

	private int damageInvulnerabilityLength = 60;	// Length of time we're invulnerable for after taking a hit
	private int damageInvulnTimer = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	public virtual void Update () 
	{
		if(damageInvulnTimer > 0)
		{
			damageInvulnTimer++;

			if(damageInvulnTimer > damageInvulnerabilityLength)
				damageInvulnTimer = 0;
		}
	}


	/**
	 * The units loses an amount of HP after reducing the incomingDamage  by its defence.
	 * If lethal is set to true, it will kill the unit regardless of its HP and defence.
	 * If ignoreInvulnerabilityPEriod is true the unit will take the attack despite being invulnerable
	 * from taking damage earlier.
	 */ 
	public virtual void TakeHit(float incomingDamage, bool lethal, bool ignoreInvulnerabilityPeriod)
	{
		// Take damage if we aren't invulnerable or if the attack ignores invulnerability (like death spikes)
		if(damageInvulnTimer > 0 || ignoreInvulnerabilityPeriod)
		{
			Debug.Log("Unit got hit");
			damageInvulnTimer++;

			if(lethal)
			{
				Die();
			}
			else
			{
				float actualDamage = incomingDamage - incomingDamage * Defence;	// Reduce damage by defence percentage
				HP = Mathf.Max(HP - actualDamage, 0);

				if(HP == 0)
				{
					Die();
				}
			}
		}
	}


	/**
	 * Called when the unit has died.
	 */ 
	public virtual void Die()
	{

	}
}

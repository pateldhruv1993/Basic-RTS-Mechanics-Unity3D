using UnityEngine;
using System.Collections;

/**
 * This class encapsulates health and death behaviours
 */ 

public class UnitHealth : MonoBehaviour
{
	public int startingHealth = 100;            // The amount of health the enemy starts the game with.
	public int currentHealth;                   // The current health the enemy has.
	public int scoreValue = 10;                 // The amount added to the player's score when the enemy dies.

	public GameObject explosionSystem;           // Reference to the particle system that plays when the enemy is damaged.
	
	Animator anim;
	bool isDead;                                // Whether the enemy is dead.
	
	void Awake ()
	{
		// Setting up the references.
		anim = GetComponent <Animator> ();
		//explosionSystem = GetComponentInChildren <ParticleSystem> ();

		
		// Setting the current health when the enemy first spawns.
		currentHealth = startingHealth;
	}
	
	public void TakeDamage (int amount, Vector3 hitPoint)
	{
		// If the enemy is dead...
		if(isDead)
			// ... no need to take damage so exit the function.
			return;

		// Reduce the current health by the amount of damage sustained.
		currentHealth -= amount;
		
		// Set the position of the particle system to where the hit was sustained.

		// If the current health is less than or equal to zero...
		if(currentHealth <= 0)
		{
			// ... the enemy is dead.
			Death ();
		}
	}
	
	
	void Death ()
	{
		//If the current unit is highlighet, turn highlighting off
		GameObject highlight = transform.Find("Highlight").gameObject;
		highlight.SetActive(false);

		//Spawn an explosion prefab at the current location
		GameObject boom = (GameObject)Instantiate(explosionSystem, transform.position, transform.rotation);
		//Turn it on
		boom.SetActive(true);
		//Schedule it to be destroyed 3 seconds from now, which will give the sound and particle system enough time
		Destroy(boom, 3);
		//Destroy the current unit
		Destroy(gameObject);
	}
}












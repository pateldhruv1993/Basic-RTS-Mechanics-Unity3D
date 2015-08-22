using UnityEngine;
using System.Collections;
using Lab4;

/**
 * This class encapsulates health and death behaviours
 */ 

public class UnitHealth : MonoBehaviour
{
	public int startingHealth = 100;            // The amount of health the enemy starts the game with.
	public int currentHealth;                   // The current health the enemy has.
	public int scoreValue = 10;                 // The amount added to the player's score when the enemy dies.
	public bool friendly;
	private float imBeingAttackedTimer = 5;
	public bool imBeingAttacked = false;
	public GameObject explosionSystem;           // Reference to the particle system that plays when the enemy is damaged.
	HiveMindAI hive;
	
	
	Animator anim;
	bool isDead;                                // Whether the enemy is dead.
	
	
	
	void Start(){
		hive = GameObject.Find("Hive mind").GetComponent<HiveMindAI>();
		if(gameObject.tag == "PlayerUnit"){
			friendly = true;
			hive.friendlies.Add(this.gameObject);
		} else if(gameObject.tag == "AIUnit"){
			friendly = false;
			hive.enemies.Add(this.gameObject);
		}
	}
	
	void Update(){
		if(imBeingAttacked){
			imBeingAttackedTimer -= Time.deltaTime;
			if(imBeingAttackedTimer <= 0){
				imBeingAttacked = false;
			}
		}
	}
	
	
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
		imBeingAttacked = true;
		imBeingAttackedTimer = 5;
		if(friendly){
			hive.friendlyIsBeingAttackedAt = transform.position;
		} else{
			hive.enemyIsBeingAttackedAt = transform.position;
		}
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
		if(friendly){
			hive.friendlies.Remove(this.gameObject);
		} else{
			hive.enemies.Remove(this.gameObject);
		}
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












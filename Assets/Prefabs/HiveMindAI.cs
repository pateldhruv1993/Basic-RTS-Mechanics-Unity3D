using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lab4;

public class HiveMindAI : MonoBehaviour {

	public string enemyTag;						//The string that hold a tag of the enemy - i.e. player - units
	public string friendlyTag;					//The string that hold a tag of the friendly - i.e. HiveMind - units
	public List<GameObject> friendlies = new List<GameObject>(); // List of selected units - in case you need it
	public List<GameObject> enemies = new List<GameObject>();
	public GameObject playerUnit;
	public GameObject aiUnit;
	public GameObject target;	
	public float spawnTimer;
	public Vector3 friendlyIsBeingAttackedAt = new Vector3(-1000, -1000, -1000);
	public Vector3 enemyIsBeingAttackedAt = new Vector3(-1000, -1000, -1000);
	public bool movedFriendliesToBattle = false;
	public bool movedEnemiesToBattle = false;
	private bool shouldSpawnUnits = false;
	private int numOfUnitsSpawned = -1;


	// Use this for initialization
	void Start () {
		/*GameObject[] temp = GameObject.FindGameObjectsWithTag("PlayerUnit");
		for(int count= 0; count < temp.Length; count++){
			friendlies.Add(temp[count]);
		}
		
		temp = GameObject.FindGameObjectsWithTag("AIUnit");
		for(int count= 0; count < temp.Length; count++){
			enemies.Add(temp[count]);
		}*/
	}
	
	// Update is called once per frame
	void Update () {
		spawnTimer -= Time.deltaTime;
		
		
		//Check if an enemy or friendly unit is being attacked
		foreach(GameObject obj in friendlies) {
		//for(int count = 0; count <= friendlies.Count; count++){
			if(obj.GetComponent<AIWithPathfinding>().imBeingAttacked == true && movedFriendliesToBattle == false){
				friendlyIsBeingAttackedAt = obj.transform.position;
				foreach(GameObject obj2 in friendlies){
					GameObject marker = (GameObject)Instantiate(target.gameObject, friendlyIsBeingAttackedAt, transform.rotation);
					marker.gameObject.name = "TempTarget";
					if(obj2.GetComponent<AIWithPathfinding>().imBeingAttacked != true){
						obj2.GetComponent<AIWithPathfinding>().AttackOn(marker.transform);
					}
				}
				movedFriendliesToBattle = true;
				break;
			}else{
				friendlyIsBeingAttackedAt =  new Vector3(-1000, -1000, -1000);
			}
		}
		

		foreach(GameObject obj in enemies){
			if(obj != null && obj.GetComponent<AIWithPathfinding>().imBeingAttacked == true && movedEnemiesToBattle == false){
				enemyIsBeingAttackedAt = obj.transform.position;
				GameObject marker = (GameObject)Instantiate(target.gameObject, enemyIsBeingAttackedAt, transform.rotation);
				marker.gameObject.name = "TempTarget";
				foreach(GameObject obj2 in enemies){
					if(obj2.GetComponent<AIWithPathfinding>().imBeingAttacked != true){
						obj2.GetComponent<AIWithPathfinding>().AttackOn(marker.transform);
					}
				}
				movedEnemiesToBattle = true;
				break;
			}else{
				enemyIsBeingAttackedAt =  new Vector3(-1000, -1000, -1000);
			}
		}
		
		
		

		Quaternion fAngle = Quaternion.AngleAxis(30, Vector3.up);

		//Here's some spawning code to get you started
		if (spawnTimer <= 0 && friendlies.Count != 0 && enemies.Count != 0){
			//Instantiate a player unit
			Vector3 fmoveAt1, fmoveAt2, fmoveAt3;
			Vector3 emoveAt1, emoveAt2, emoveAt3;
			
			if(friendlyIsBeingAttackedAt == new Vector3(-1000, -1000, -1000)){
				fmoveAt1 = new Vector3(Random.Range(-9.0F, -1.0F), 0, Random.Range(-47.0F, -40.0F));
				fmoveAt2 = new Vector3(Random.Range(-9.0F, -1.0F), 0, Random.Range(-47.0F, -40.0F));
				fmoveAt3 = new Vector3(Random.Range(-9.0F, -1.0F), 0, Random.Range(-47.0F, -40.0F));
			} else{
				fmoveAt1 = fmoveAt2 = fmoveAt3 = friendlyIsBeingAttackedAt;
			}
			Spawner(true, new Vector3(-41.47181f, 1.0f, -41.57702f), fmoveAt1);
			Spawner(true, new Vector3(-39.90593f, 1.0f, -40.22548f), fmoveAt2);
			Spawner(true, new Vector3(-37.98991f, 1.0f, -38.77068f), fmoveAt3);
			
			
			
			
			
			if(enemyIsBeingAttackedAt == new Vector3(-1000, -1000, -1000)){
				emoveAt1 = new Vector3(Random.Range(9.0F, 20.0F), 0, Random.Range(40.0F, 48.0F));
				emoveAt2 = new Vector3(Random.Range(9.0F, 20.0F), 0, Random.Range(40.0F, 48.0F));
				emoveAt3 = new Vector3(Random.Range(9.0F, 20.0F), 0, Random.Range(40.0F, 48.0F));
			} else{
				emoveAt1 = emoveAt2 = emoveAt3 = enemyIsBeingAttackedAt;
			}
			Spawner(false, new Vector3(41.04708f, 1.5f, 39.39053f), emoveAt1);
			Spawner(false, new Vector3(41.04708f, 1.5f, 39.39053f), emoveAt2);
			Spawner(false, new Vector3(41.04708f, 1.5f, 39.39053f), emoveAt3);
			
			
			spawnTimer = 25;
		}
		
		if(spawnTimer < 0){
			spawnTimer = 0;
		}
		/*
		if (Input.GetKeyUp(KeyCode.Alpha2)){
			//Instantiate an AI unit
			GameObject newUnit = (GameObject)Instantiate(aiUnit, new Vector3(0,1.5f,0), transform.rotation);
			//Temporarily deactivate
			newUnit.SetActive(false);
			//Instantiate a new target marker
			GameObject marker = (GameObject)Instantiate(target.gameObject, new Vector3(-2,1.5f,-2), transform.rotation);
			marker.gameObject.name = "TempTarget";
			
			//Set the new target marker to be the target, and execute the user attack move on it
			newUnit.GetComponent<AIWithPathfinding>().SetTarget(marker.transform);
			newUnit.GetComponent<AIWithPathfinding>().AttackOn(marker.transform);
			//Reactivate
			newUnit.SetActive(true);
		}*/
	}
	
	
	//GUI to draw Timer for next wave
	void OnGUI(){
		GUI.Box (new Rect(10, 10, 380, 22), "Next Wave in: " + spawnTimer.ToString("0") + " | Friendlies Count:" + friendlies.Count+ " | Enemies Count:" + enemies.Count);
	}
	
	
	
	//Method to spawn the units
	void Spawner(bool unitType, Vector3 spawnPos, Vector3 moveAt){
		GameObject newUnit;
		if(unitType){
			newUnit = (GameObject)Instantiate(playerUnit, spawnPos, transform.rotation);
			friendlies.Add(newUnit);
		} else{
			newUnit = (GameObject)Instantiate(aiUnit, spawnPos, transform.rotation);
			enemies.Add(newUnit);
		}	
		//Temporarily deactivate
		newUnit.SetActive(false);
		//Instantiate a new target marker
		GameObject marker = (GameObject)Instantiate(target.gameObject, moveAt, transform.rotation);
		marker.gameObject.name = "TempTarget";
		
		//Set the new target marker to be the target, and execute the user attack move on it
		newUnit.GetComponent<AIWithPathfinding>().SetTarget(marker.transform);
		newUnit.GetComponent<AIWithPathfinding>().AttackOn(marker.transform);
		//Reactivate
		newUnit.SetActive(true);
	}
	/*
	//Method to remove gameObjects from the 'friendlies' and 'enemies' list
	public void RemoveFromList(bool type, GameObject removeThis){
	    print ("------------I waz called------------- type: " + type);
		print ("Friendlies count: " + this.friendlies.Count);
		print ("RemoveFromList " + 0 + " " + this.friendlies[0].GetInstanceID());
		print ("RemoveFromList " + 1 + " " + this.friendlies[1].GetInstanceID());
		if(type){
		print ("I was calleed dtoo");
			if(friendlies.Remove(removeThis))
				print ("assult bot was removed");
		} else{
			if(enemies.Remove (removeThis))
				print ("enemy unit was removed");
		}
	}
	*/
}

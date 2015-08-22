using UnityEngine;
using System.Collections;
using Pathfinding;
using Pathfinding.RVO;
using Lab4;

/** 
 * This class defines all the combat and movement behaviours
 * The logic for selecting one of this behaviours resides in the state machine class
 */


namespace Lab4 {
	public class AIWithPathfinding : MonoBehaviour {
		public Transform target;			//Reference for the target object.  Can be a destination point or an enemy.
		public float sightDistance = 20;	//Sight range
		public float attackDistance = 10;	//Attack - i.e. shooting - range
		public GameObject highlight;		//A quad object that lights up up under user-selected units
		public bool imBeingAttacked;
		
		private Seeker seeker;				//A* Seeker class
		private CharacterController controller;
		public bool moveCommand = false;	//A flag that gets set if the user issued the Move commend
		public bool selected;				//A flag that gets set if the user issued clicked on the unit
		public string enemyTag;				//The string that hold a tag of the enemy
											//This code is used by both player and AI units
											//AI units are tagged "EnemyUnit", so for the player this tag is set to "EnemyUnit"
											//Player units are tagged "Selectable", so for the AI this tag is set to "Selectable"
										
		public float pathfindFreq = 1;		//How many times per second we need to recalculate the path to target


		private Vector3 dir;				//Move direction
		private Ray LOSRay;					//Ray object for LOS checks
		private float pathfindTimer = 0;	//Pathfind timer for pathfinding frequency
		private RaycastHit LOSHit;			//Raycast hit object for LOS checks
		private GunShooting[] guns;			//Array of guns (may only have one gun)
		private bool calculatingPath = false; //Flag to indicate if path calculation is in progress in a separate thread
		protected Animator animator;		//Reference to Mecanim animator object

		public Transform globalTarget;

		private bool stopped = false;		//Flag to indicate if the Stop command was issued by the user
		private bool hold = false;			//Flag to indicate if the Hold command was issued by the user
		public bool patrol = false;		//Flag to indicate if the Patrol command was issued by the user
		public bool attack = false;		//Flag to indicate if the Attack command was issued by the user

		private Vector3 patStart;
		private Vector3 patEnd;


		//The calculated path
		public Path path;
		
		//The AI's speed per second
		public float speed = 3;
		
		//The max distance from the AI to a waypoint for it to continue to the next waypoint
		public float nextWaypointDistance = 0.5f;
		public float distToTarget;
		
		//The waypoint we are currently moving towards
		private int currentWaypoint = 0;


		/** Initialize a few useful variables
		 */
		public void Start () {
			//Get n array of all guns attached to this entity- we might have more than one
			guns = GetComponentsInChildren<GunShooting>();
			seeker = GetComponent<Seeker>();
			controller = GetComponent<CharacterController>();
			animator = GetComponentInChildren<Animator>();

			if (target != null){
				//target.transform.position = transform.position;
			}
		}

		public void SetTarget (Transform newTarget) {
			target = newTarget;
		}

		/** Callback function for the A* algorithm - it gets called when 
		 *  the Seeker class is done searching and finds a path
		 */
		public void OnPathComplete (Path p) {
			if (!p.error) {
				path = p;
//				print("Found path to "+target.position);

				//Reset the waypoint counter
				currentWaypoint = 0;
				//Turn off the flag indicating path calculation
				calculatingPath = false;

			}
		}

		/** A function for the FSM to check if the unit reached the end of the path
		 */
		public void IssueMoveCommand(){
			moveCommand = true;
			stopped = false;
			hold = false;
			patrol = false;
			attack = false;
		}


		/** Idle behaviour implementation
		 */
		public void BeIdle(){
			//Reset pathfinding variables
			currentWaypoint = 0;
			path = null;

			//Set the animator variable
			animator.SetBool("moving", false);

			//Disable movement
			dir = Vector3.zero;
		}

		/** Reset pathfind timer
		 */
		public void ResetPathTimer(){
			pathfindTimer = 0;
		}

		/** Attack Approach behviour:
		 * - Search for a new path to target pathfindFreq times per second
		 * - Approach target (target can be an enemy unit or the target marker for the user click)
		 */
		public void AttackApproach(){
			if (pathfindTimer >= 1/pathfindFreq){
				FindPath();
				pathfindTimer = 0;
				return;
			}else{
				pathfindTimer += Time.fixedDeltaTime;
			}
			Approach();
		}

		/** Approach behviour:
		 * - Follow the waypoints
		 * 
		 */
		public void Approach(){
			//Direction to the next waypoint
//			print ("Called approach");
			if (path == null)
				return;
			if (currentWaypoint >= path.vectorPath.Count){
				//print ("Approach sets path to null");
				path=null;
				return;
			}

			Vector3 myFlatPos = transform.position;
			myFlatPos.y = 0;

			Vector3 targetFlatPos = path.vectorPath[currentWaypoint];
			targetFlatPos.y = 0;
			
			//Check if we are close enough to the next waypoint
			//If we are, proceed to follow the next waypoint

//			print ("Distance to waypoint: "+Vector3.Distance (myFlatPos,targetFlatPos));
			if (Vector3.Distance (myFlatPos,targetFlatPos) < nextWaypointDistance){// && currentWaypoint < path.vectorPath.Count-1){
				currentWaypoint++;
				return;
			}
			myFlatPos = transform.position;
			myFlatPos.y = 0;
			
			targetFlatPos = path.vectorPath[currentWaypoint];
			targetFlatPos.y = 0;
			
			
			dir = (targetFlatPos-myFlatPos).normalized;
			
			FaceTarget(path.vectorPath[currentWaypoint]);
			Debug.DrawRay(transform.position, dir);
			dir *= speed;

			
			animator.SetBool("moving", true);
			
			foreach (Vector3 node in path.vectorPath){
				Debug.DrawRay(node, Vector3.up, Color.red, 10);
			}

			Debug.DrawRay(path.vectorPath[currentWaypoint], 2*Vector3.up, Color.yellow);
		}

		/** Attack behavior
		 * - Stand still, disable all movement-related behaviour
		 * - Face target
		 * - Fire all guns
		 */
		public void Attack(){
			currentWaypoint = 0;
			path = null;
			dir = Vector3.zero;
			//Set the animator variable
			animator.SetBool("moving", false);

			if (target != null && target.tag == enemyTag){
				FaceTarget(target.position);
				GunShooting[] guns = GetComponentsInChildren<GunShooting>();
				foreach(GunShooting gun in guns){
					gun.Fire();
				}
			}
		}

		/** Hold behavior
		 * - Stand still, disable all movement-related behaviour
		 * - If no target in range - do nothing
		 * - If target in range, attack
		 */
		public void Hold(){
			if (CanAttack() && HasLOS()){
				GetClosest();
				Attack ();
			}else{
				target = null;
				BeIdle ();
			}
		}

		//Sets the flag to stop the current command
		public void Stop(){
			stopped = true;
			attack = false;
			hold = false;
			patrol = false;
		}
		
		//Sets the flag to enable Hold command
		public void HoldOn(){
			hold = true;
			attack = false;
			stopped = false;
			patrol = false;
		}
		
		public bool Stopped(){
			return stopped;
		}
		
		public bool ShouldHold(){
			return hold;
		}

		public void PatrolOn(Vector3 patPoint){
			print ("Turning patrol on");
			path = null;
			currentWaypoint = 0;
			patrol = true;
			attack = false;
			stopped = false;
			hold = false;
			patStart = transform.position;
			patEnd = patPoint;
			if (target == null){
				target = globalTarget;
				print ("Changing tagget**********");
				path = null;
			}
		}

		public void AttackOn(Transform attackTarget){
//			print ("Turning attack on");
			path = null;
			currentWaypoint = 0;
			attack = true;
			patrol = false;
			stopped = false;
			hold = false;
			target = attackTarget;
			FindPath();
			//path = null;
		}

		public bool ShouldPatrol(){
			return patrol;
		}

		public bool UserAttackMove(){
			return attack;
		}

		public void UserAttack(){

			if (path == null && !calculatingPath){
//				print ("UserAttack looking for path");
//				bool nullP = path==null;
//				print ("UserAttack looking for path: path is "+nullP);
//				print ("UserAttack looking for path: calculatingPath is "+calculatingPath);
				FindPath();
			}
			if (target != null && target.tag == enemyTag)
				AttackApproach();
			else
				Approach();
		}

		public void Patrol(){
			if (FlatDist(patStart, transform.position) < 0.5 && (target.position != patEnd || path == null) && !calculatingPath){
				target.position = patEnd;
				FindPath();
				print ("Switching: start="+transform.position+" end="+patEnd);
			}else if (FlatDist(patEnd, transform.position) < 0.5 && (target.position != patStart || path == null) && !calculatingPath){
				target.position = patStart;
				FindPath();
				print ("Switching: start="+transform.position+" end="+patStart);
			}
			Approach();
		}

		/** Checks to see if this unit has line of sight (LOS) to the closest enemy
 		 *  Since some units have multiple weapons, we need to make sure that the weapons themselves have LOS to target
 		 */
		public bool HasLOS(){
			if (EnemyClose()){
				//Save target and current orientation
				Transform tmpTrans = target;
				Quaternion tmpOri = transform.rotation;

				//Find closest enemy.  This also sets "this.target" to be the closest enemy
				GetClosest();

				//Face closest enemy
				FaceTarget(target.position);


				float enemDist = Vector3.Distance(transform.position, target.position);
				bool hasGunLOS = true;
				GunShooting gun;

				//Iterate through all guns.  For every gun, check if it has LOS to target.  
				for(int i = 0; i < guns.Length; i++){
					gun = guns[i];

					int LOSMask = LayerMask.GetMask ("Obstacles", LayerMask.LayerToName(gameObject.layer));
					//print ("Obstacles mask: "+LayerMask.GetMask ("Obstacles")+" Obstacles | gameObject.layer: "+ LOSMask);


					//Origin is the gun muzzle
					LOSRay.origin = gun.gameObject.transform.position;

					//Direction is the gun's forward vector
					LOSRay.direction = gun.gameObject.transform.forward;

					//Draw a ray for debug purposes
					//Debug.DrawRay(LOSRay.origin, LOSRay.direction*enemDist, Color.white);

					//Check if there's an obstacle between the gun muzzle and the target

					if(Physics.Raycast (LOSRay, out LOSHit, enemDist, LOSMask))
					{
//						print ("Gun["+i+"] ("+gun.name+") with tag "+transform.tag+" hit "+LOSHit.transform.name + " with tag "+ LOSHit.transform.tag);
						//If something was hit, AND the value of hasGunLOS with false - which makes it false

						//If we hit another Unit, but with a different tag, it's ain't friendly - kill it!
						if (LOSHit.transform.gameObject.layer == gameObject.layer && LOSHit.transform.tag != transform.tag)
							hasGunLOS = hasGunLOS && true;
						else
							hasGunLOS = hasGunLOS && false;
					}else{
						//If noting was hit, AND the value of hasGunLOS with true - which leaves it unmodified
						hasGunLOS = hasGunLOS && true;
					}
				}

				//Restore the original target and orientation
				//print ("Changing tagget**********");
				target = tmpTrans;
				transform.rotation = tmpOri;
				return hasGunLOS;
			}else{
				return false;
			}
		}

		public bool CanAttackTarget(){
			if (target != null && target.tag == enemyTag && FlatDist(transform.position, target.position) <= attackDistance){
				return true;
			}else{
				return false;
			}
		}

		/** Find if there's an enemy in attack range
 		 */
		public bool CanAttack(){
			GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
			Transform t = target;
			float minDistance = 1000000;

			//Iterate through all enemies and check their distance
			foreach (GameObject enemy in enemies){
				if (Vector3.Distance(enemy.transform.position, transform.position) < minDistance){
					minDistance = Vector3.Distance(enemy.transform.position, transform.position);
					t = enemy.transform;
				}
			}
			if (minDistance <= attackDistance){
				return true;
			}else
				return false;
		}	

		/** Find if there's an enemy in sight range
 		 */
		public bool EnemyClose(){
			GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
			Transform t = target;
			float minDistance = 1000000;

			//Iterate through all enemies and check their distance
			foreach (GameObject enemy in enemies){
				if (Vector3.Distance(enemy.transform.position, transform.position) < minDistance){
					minDistance = Vector3.Distance(enemy.transform.position, transform.position);
					t = enemy.transform;
				}
			}
			if (minDistance <= sightDistance){
				return true;
			}else{
				return false;
			}
		}

		/** Set this.target to the closest enemy
 		 */
		public void GetClosest(){
			GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
			Transform t = target;
			float minDistance = 1000000;

			//Iterate through all enemies and check their distance
			foreach (GameObject enemy in enemies){
				if (Vector3.Distance(enemy.transform.position, transform.position) < minDistance){
					minDistance = Vector3.Distance(enemy.transform.position, transform.position);
					t = enemy.transform;
				}
			}
			//print ("Changing tagget**********");
			target = t;
		}

		//Returns true if user issued the Move command.  Useful for thr FSM
		public bool ShouldApproach(){
			return moveCommand;
		}

		//Returns true if unit completed the Move command.  Useful for thr FSM
		public bool PathComplete(){
			if (!calculatingPath){
			//	print ("we're not calculating path");
				return path==null;
			}else{
				print ("we're calculating path");
				return false;
			}
		}

		//Calls the Seeker class from the A* pathfinding framework to find the path to target
		public void FindPath(){
//			print ("Starting FindPath(): calculatingPath is "+calculatingPath);
			if (target != null){

				if (!seeker)
					seeker = GetComponent<Seeker>();
				seeker.StartPath (transform.position,target.position, OnPathComplete);
				calculatingPath = true;


			}
	//		print ("Ending FindPath(): calculatingPath is "+calculatingPath);
		}

		//You can ignore this for now
		/*
		void OnControllerColliderHit(ControllerColliderHit hit) {
			float pushPower = 2.0F;
			Rigidbody body = hit.collider.attachedRigidbody;
			if (body == null || body.isKinematic)
				return;
			
			if (hit.moveDirection.y < -0.3F)
				return;
			
			Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
			body.velocity = pushDir * pushPower;
		}
		*/

		//Fixed time step update
		public void FixedUpdate () {
			imBeingAttacked = gameObject.GetComponent<UnitHealth>().imBeingAttacked;
			if (path == null && !calculatingPath) {
				//We have no path to move after yet
				animator.SetBool("moving", false);
				moveCommand = false;
				return;
				//If we are in the process of calculating the path, don't move yet
			}else if (path == null && calculatingPath){
				return;
			}

			//If we have no current taget (e.g. destroyed), reset everything
			if (target == null){
				path = null;
				print ("FixedUpdate sets path to null: target == null");
				moveCommand = false;
				animator.SetBool("moving", false);
				return;
			}

			//If we have reached the end of the path, reset everything
			if (currentWaypoint >= path.vectorPath.Count || Vector3.Distance(transform.position, target.position) <= distToTarget){

				print ("FixedUpdate sets path to null: distance to target is " + Vector3.Distance(transform.position, target.position));
				print ("transform.position = "+transform.position+" target.position = "+target.position);
				print ("Target is "+target.transform.name);
				path = null;
				animator.SetBool("moving", false);
				if (target != null && target.name == "TempTarget"){
					Destroy(target.gameObject);
				}
				return;
			}

			//Use the dir vector calculated by one of the other methids - e.g. Attach or Approach - to move the unit
			controller.SimpleMove (dir);
			Debug.DrawRay(path.vectorPath[currentWaypoint], 2*Vector3.up, Color.yellow);
		}

		//Rotate to face the target without looking up or down
		void FaceTarget(Vector3 pt){
			Vector3 lookAtPt = pt;
			lookAtPt.y = transform.position.y;
			
			transform.LookAt(lookAtPt);
		}

		float FlatDist(Vector3 pt1, Vector3 pt2){
			Vector3 myFlatPos = pt1;
			myFlatPos.y = 0;
			
			Vector3 targetFlatPos = pt2;
			targetFlatPos.y = 0;

			return Vector3.Distance(myFlatPos, targetFlatPos);
		}
	} 
}
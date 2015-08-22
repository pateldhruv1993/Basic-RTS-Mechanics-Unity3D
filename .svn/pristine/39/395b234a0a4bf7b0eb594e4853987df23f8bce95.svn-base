using UnityEngine;
using System.Collections;
using Pathfinding;
using Pathfinding.RVO;
using Lab4;

namespace Lab4 {
	public class PathfinderAI : MonoBehaviour {
		public Transform target;
		public float sightDistance = 20;
		public float attackDistance = 10;
		public GameObject highlight;
		
		private Seeker seeker;
		private CharacterController controller;
		public bool userCommand = false;
		public bool selected;
		public string enemyTag;


		private Vector3 dir;
		private Ray LOSRay;
		private float pathfindTimer = 0;
		private RaycastHit LOSHit;
		private GunShooting[] guns;
		protected Animator animator;

		
		
		//The calculated path
		public Path path;
		
		//The AI's speed per second
		public float speed = 3;
		
		//The max distance from the AI to a waypoint for it to continue to the next waypoint
		public float nextWaypointDistance = 0.5f;
		public float distToTarget;
		
		//The waypoint we are currently moving towards
		private int currentWaypoint = 0;
		
		public void Start () {
			guns = GetComponentsInChildren<GunShooting>();
			seeker = GetComponent<Seeker>();
			controller = GetComponent<CharacterController>();
			animator = GetComponentInChildren<Animator>();
			if (target != null)
				target.transform.position = transform.position;
		}
		
		public void OnPathComplete (Path p) {
			//		Debug.Log ("Yay, we got a path back. Did it have an error? "+p.error);
			if (!p.error) {
				path = p;
				//Reset the waypoint counter
				currentWaypoint = 0;
			}
		}

		public void IssueUserCommand(){
			userCommand = true;
		}

		public void BeIdle(){
			currentWaypoint = 0;
			path = null;
			animator.SetBool("moving", false);
			dir = Vector3.zero;
		}

		public void ResetPathTimer(){
			pathfindTimer = 0;
		}

		public void AttackApproach(){
			if (pathfindTimer >= 1){
				FindPath();
				pathfindTimer = 0;
				return;
			}else{
				pathfindTimer += Time.fixedDeltaTime;
			}
			Approach();
		}

		public void Approach(){
			//Direction to the next waypoint
			if (path == null)
				return;
			if (currentWaypoint >= path.vectorPath.Count){
				path=null;
				return;
			}

			Vector3 myFlatPos = transform.position;
			myFlatPos.y = 0;

			Vector3 targetFlatPos = path.vectorPath[currentWaypoint];
			targetFlatPos.y = 0;
			
			//Check if we are close enough to the next waypoint
			//If we are, proceed to follow the next waypoint
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
			//Debug.DrawRay(transform.position, dir);
			dir *= speed;

			
			animator.SetBool("moving", true);
			
			//Uncommet the code below if you want to see waypoints in the path

//			print (path.vectorPath.Count);
//			print ("Path:");
			foreach (Vector3 node in path.vectorPath){
//				print(node);
				Debug.DrawRay(node, Vector3.up, Color.red, 10);
			}

			Debug.DrawRay(path.vectorPath[currentWaypoint], 2*Vector3.up, Color.yellow);
		}

		public void Attack(){
			currentWaypoint = 0;
			path = null;
			dir = Vector3.zero;
			animator.SetBool("moving", false);
			if (target != null){
				FaceTarget(target.position);
				GunShooting[] guns = GetComponentsInChildren<GunShooting>();
				foreach(GunShooting gun in guns){
					gun.Fire();
				}
			}
		}

		public bool HasLOS(){
			if (EnemyClose()){
				Transform tmpTrans = target;
				Quaternion tmpOri = transform.rotation;
				GetClosest();
				FaceTarget(target.position);

				float enemDist = Vector3.Distance(transform.position, target.position);
				print ("LOS check found enemy "+target.name+ " "+enemDist+" units away");

				bool hasGunLOS = true;
				print("num guns: "+guns.Length);
				GunShooting gun;
				for(int i = 0; i < guns.Length; i++){
					gun = guns[i];
					int LOSMask = LayerMask.GetMask ("Obstacles");
					LOSRay.origin = gun.gameObject.transform.position;
					LOSRay.direction = gun.gameObject.transform.forward;

					Debug.DrawRay(LOSRay.origin, LOSRay.direction*enemDist, Color.white);
					if(Physics.Raycast (LOSRay, out LOSHit, enemDist, LOSMask))
					{
						print ("Gun["+i+"] ("+gun.name+") hit "+LOSHit.transform.name);
						hasGunLOS = hasGunLOS && false;
					}else{
						hasGunLOS = hasGunLOS && true;
					}
				}

				target = tmpTrans;
				transform.rotation = tmpOri;
				return hasGunLOS;
			}else{
				return false;
			}
			// Perform the raycast against gameobjects on the shootable layer and if it hits something...

			
		}

		public bool CanAttack(){
			GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
			Transform t = target;
			float minDistance = 1000000;
			foreach (GameObject enemy in enemies){
				if (Vector3.Distance(enemy.transform.position, transform.position) < minDistance){
					minDistance = Vector3.Distance(enemy.transform.position, transform.position);
					t = enemy.transform;
				}
			}
			if (minDistance <= attackDistance){
				//target = t;
				return true;
			}else
				return false;
		}	

		public bool EnemyClose(){
			GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
			Transform t = target;
			float minDistance = 1000000;
			foreach (GameObject enemy in enemies){
				if (Vector3.Distance(enemy.transform.position, transform.position) < minDistance){
					minDistance = Vector3.Distance(enemy.transform.position, transform.position);
					t = enemy.transform;
				}
			}
			if (minDistance <= sightDistance){
				//target = t;
			//	print ("Min distance: "+minDistance+" "+sightDistance);
				return true;
			}else{
			//	print ("Min distance: "+minDistance+" "+sightDistance);
				return false;
			}
		}

		public void GetClosest(){
			GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
			Transform t = target;
			float minDistance = 1000000;
			foreach (GameObject enemy in enemies){
				if (Vector3.Distance(enemy.transform.position, transform.position) < minDistance){
					minDistance = Vector3.Distance(enemy.transform.position, transform.position);
					t = enemy.transform;
				}
			}
			target = t;
		}

		public bool ShouldApproach(){
			return userCommand;
		}

		public bool PathComplete(){
			return path==null;
		}



		public void FindPath(){
			if (target != null){
				if (!seeker)
					seeker = GetComponent<Seeker>();

				//		print ("Path from "+transform.position+" to "+target.position);
				seeker.StartPath (transform.position,target.position, OnPathComplete);

			}
		}

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
		
		public void FixedUpdate () {
			if (path == null) {
				//We have no path to move after yet
				animator.SetBool("moving", false);
				userCommand = false;
				//if (target)
				//	FaceTarget(target.position);
				return;
			}

			if (target == null){
				//			Debug.Log ("End Of Path Reached");
				path = null;
				userCommand = false;
				animator.SetBool("moving", false);
				return;
			}
			
			if (currentWaypoint >= path.vectorPath.Count || Vector3.Distance(transform.position, target.position) <= distToTarget){
				//			Debug.Log ("End Of Path Reached");
				path = null;
				//userCommand = false;
				//FaceTarget(target.position);
				animator.SetBool("moving", false);
				return;
			}

			controller.SimpleMove (dir);
			Debug.DrawRay(path.vectorPath[currentWaypoint], 2*Vector3.up, Color.yellow);
		}

		void FaceTarget(Vector3 pt){
			Vector3 lookAtPt = pt;
			lookAtPt.y = transform.position.y;
			
			transform.LookAt(lookAtPt);
		}
	} 
}
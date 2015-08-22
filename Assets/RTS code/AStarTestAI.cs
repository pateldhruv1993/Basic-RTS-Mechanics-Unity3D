using UnityEngine;
using System.Collections;
using Pathfinding;
using Lab4;

namespace Lab4 {
	public class AStarTestAI : MonoBehaviour {
		public Transform target;

		public GameObject highlight;

		private Seeker seeker;
		private CharacterController controller;
		protected Animator animator;

		
		//The calculated path
		public Path path;
		
		//The AI's speed per second
		public float speed = 100;
		
		//The max distance from the AI to a waypoint for it to continue to the next waypoint
		public float nextWaypointDistance = 0.5f;
		
		//The waypoint we are currently moving towards
		private int currentWaypoint = 0;
		
		public void Start () {
			target = GameObject.Find("Target").transform;
			seeker = GetComponent<Seeker>();
			controller = GetComponent<CharacterController>();
			animator = GetComponentInChildren<Animator>();
			
			//Start a new path to the target.position, return the result to the OnPathComplete function
			//FindPath();
		}
		
		public void OnPathComplete (Path p) {
	//		Debug.Log ("Yay, we got a path back. Did it have an error? "+p.error);
			if (!p.error) {
				path = p;
				//Reset the waypoint counter
				currentWaypoint = 0;
			}
		}

		public void FindPath(){
			if (seeker)
				print ("Seeker is "+seeker== null);
			else
				seeker = GetComponent<Seeker>();
	//		print ("Path from "+transform.position+" to "+target.position);
			seeker.StartPath (transform.position,target.position, OnPathComplete);
		}


		public void FixedUpdate () {

			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

			if (path == null) {
				//We have no path to move after yet
				animator.SetBool("moving", false);
				return;
			}
			
			if (currentWaypoint >= path.vectorPath.Count) {
	//			Debug.Log ("End Of Path Reached");
				animator.SetBool("moving", false);
				return;
			}
			
			//Direction to the next waypoint
			Vector3 myFlatPos = transform.position;
			myFlatPos.y = 0;
			
			Vector3 targetFlatPos = path.vectorPath[currentWaypoint];
			targetFlatPos.y = 0;
			
			
			Vector3 dir = (targetFlatPos-myFlatPos).normalized;
			
			Vector3 lookAtPt = path.vectorPath[currentWaypoint];
			lookAtPt.y = transform.position.y;
			
			transform.LookAt(lookAtPt);
			Debug.DrawRay(transform.position, dir);
			dir *= speed;
			controller.SimpleMove (dir);
			
			animator.SetBool("moving", true);
			
			
			//Uncommet the code below if you want to see waypoints in the path

			/*
			print (path.vectorPath.Count);
			foreach (Vector3 node in path.vectorPath){
				print(node);
				Debug.DrawRay(node, Vector3.up, Color.red, 10);
			}
			Debug.DrawRay(path.vectorPath[currentWaypoint], 2*Vector3.up, Color.yellow);
			*/
			
			//Check if we are close enough to the next waypoint
			//If we are, proceed to follow the next waypoint
			if (Vector3.Distance (myFlatPos,targetFlatPos) < nextWaypointDistance) {
				currentWaypoint++;
				return;
			}
		}
	} 
}
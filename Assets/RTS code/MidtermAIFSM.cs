using UnityEngine;
using System.Collections;

/** This class handles state machine and state changes
 *  It calls methods defined in AIWithPathfinding to get the conditions
 *  and execute the actual behaviours.
 */

namespace Lab4 {
	public class MidtermAIFSM : AIFSM {
		GameObject mouseTarget;
		// Use this for initialization
		void Start () {
			base.Start();
			currentState = GameState.Idle;
			aiController = GetComponent<AIWithPathfinding>();
			mouseTarget = GameObject.Find("CursorTarget");
		}
		
		// Update is called once per frame
		void Update () {
			//Get all the boolean variables for decision making.  See FSM diagram for reference.
			bool inAttackRange = aiController.CanAttack();
			bool inTargetAttackRange = aiController.CanAttackTarget();
			bool hasLOS = aiController.HasLOS();
			bool inSightRange = aiController.EnemyClose();
			bool userApproach = aiController.ShouldApproach();
			bool pathComplete = aiController.PathComplete();
			bool stop = aiController.Stopped();
			bool hold = aiController.ShouldHold();
			bool patrol = aiController.ShouldPatrol();
			bool userAttack = aiController.UserAttackMove();

//			print ("Current state: "+currentState+" and pathComplete is "+pathComplete);
			
			//Combine the boolean variables to create more humanly readable conditions
			bool canAttack = inAttackRange && hasLOS && !userApproach;
			bool canAttackTarget = inTargetAttackRange && hasLOS && !userApproach;
			bool canAttackApproach = inSightRange && (!inAttackRange || !hasLOS) && !userApproach;
			bool shouldIdle = !inSightRange || aiController.target == null;
			
//			print ("canAttack: "+canAttack+ " inAttackRange: "+ inAttackRange + " hasLOS: "+ hasLOS + " userApproach: "+ userApproach);
//			print ("canAttackApproach: "+canAttackApproach+ " inSightRange: "+ inSightRange + " inAttackRange: "+ inAttackRange + " userApproach: "+ userApproach);
			//print ("inSightRange: "+ inSightRange + " inAttackRange: "+ inAttackRange + " userApproach: "+ userApproach);
			//print ("canAttack: "+canAttack+" canAttackApproach: "+canAttackApproach+" shouldIdle: "+shouldIdle+" userApproach: "+userApproach);
			
			/*
			 * This transition function is only responsible for state changes
			 * If the conditions require a state change, we update currentState
			 * otherwise, if we need to remain in the current state, we do nothing
			 */ 
			switch(currentState)
			{
			case GameState.Idle:
				
				//if user says move
				if (userApproach){
					print (name+"Switching to Approach");
					SwitchToApproach();
					//If we can attack
				}else if (hold){
					print (name+"Switching to Hold");
					currentState = GameState.Hold;
				}else if (patrol){
					SwitchToPatrol();
				}else if (userAttack){
					SwitchToUserAttack();
					//If we can approach to attack
				}else if (canAttack){
					SwitchToAttack();
					//If we can approach to attack
				}else if (canAttackApproach){
					print (name+"Switching to AttackMove");
					aiController.GetClosest();
					currentState = GameState.AttackApproach;
				}
				break;
			case GameState.Approach:
				//if user says stop
				if (pathComplete || stop){
					print (name+"Switching to Idle");
					currentState = GameState.Idle;
				}else if (hold){
					print (name+"Switching to Hold");
					currentState = GameState.Hold;
				}else if (patrol){
					SwitchToPatrol();
				}else if (userAttack){
					SwitchToUserAttack();
				}
				break;
			case GameState.Attack:
				//if user says move
				if (userApproach){
					print (name+"Switching to Approach");
					SwitchToApproach();
					//If we have nothing to do
				}else if (hold){
					print (name+"Switching to Hold");
					currentState = GameState.Hold;
				}else if (patrol){
					SwitchToPatrol();
				}else if (userAttack){
					SwitchToUserAttack();
				}else if (shouldIdle) {
					print (name+"Switching to Idle");
					//On transition from Attack to Idle: set the mouse target, set target position to be that of the unit
					//aiController.target = mouseTarget.transform;
					//mouseTarget.transform.position = aiController.transform.position;
					currentState = GameState.Idle;
					//If we can approach to attack
				}else if (canAttackApproach){
					print (name+"Switching to AttackMove");
					currentState = GameState.AttackApproach;
				}
				break;
			case GameState.AttackApproach:
				//if user says move
				if (userApproach){
					print (name+"Switching to Approach");
					SwitchToApproach();
					//If we can attack
				}else if (hold){
					print (name+"Switching to Hold");
					currentState = GameState.Hold;
				}else if (patrol){
					SwitchToPatrol();
				}else if (userAttack){
					SwitchToUserAttack();
				}else if (canAttack) {
					SwitchToAttack();
					//If we have nothing to do
				}else if (shouldIdle){
					print (name+"Switching to Idle");
					currentState = GameState.Idle;
				}
				break;
			case GameState.Hold:
				if (userApproach){
					print (name+"Switching to Approach");
					SwitchToApproach();
					//If we can attack
				}else if (stop){
					print (name+"Switching to Idle");
					currentState = GameState.Idle;
				}else if (patrol){
					SwitchToPatrol();
				}else if (userAttack){
					SwitchToUserAttack();
				}
				break;
			case GameState.Patrol:
				if (userApproach){
					print (name+"Switching to Approach");
					SwitchToApproach();
					//If we can attack
				}else if (hold){
					print (name+"Switching to Hold");
					currentState = GameState.Hold;
				}else if (stop){
					print (name+"Switching to Idle");
					currentState = GameState.Idle;
				}else if (userAttack){
					SwitchToUserAttack();
				}else if (canAttack){
					SwitchToAttack();
					//If we can approach to attack
				}else if (canAttackApproach){
					print (name+"Switching to AttackMove");
					aiController.GetClosest();
					currentState = GameState.AttackApproach;
				}
				break;
			case GameState.UserAttack:
				if (userApproach){
					print (name+"Switching to Approach");
					SwitchToApproach();
					//If we can attack
				}else if (hold){
					print (name+"Switching to Hold");
					currentState = GameState.Hold;
				}else if (patrol){
					SwitchToPatrol();
				//If has target, attack target if can
				//If has no target, attack nearest if able
				}else if(canAttackTarget){
					SwitchToAttack(false);
				}else if(canAttack){
					SwitchToAttack();
				}else if (canAttackApproach){
					print (name+"Switching to AttackMove");
					aiController.GetClosest();
					currentState = GameState.AttackApproach;
				}else if(pathComplete || stop){
					print (name+"Switching to Idle: pathComplete is "+pathComplete+", stop is "+stop);
					currentState = GameState.Idle;
				}
				break;
			default:
				//If something goes wrong, default to idle
				currentState = GameState.Idle;
				break;
			}
			ExecuteCurrent();
		}
		
		//This gets called on transition form any state to approach
		public void SwitchToApproach(){
			//print (name+"Switching to approach");
			aiController.SetTarget (mouseTarget.transform);
			currentState = GameState.Approach;
		}

		//This gets called on transition form any state to approach
		public void SwitchToPatrol(){
			aiController.patrol = false;
			print (name+"Switching to Patrol");
			currentState = GameState.Patrol;
		}

		public void SwitchToUserAttack(){
			aiController.attack = false;
			print (name+"Switching to UserAttack");
			currentState = GameState.UserAttack;
		}

		public void SwitchToAttack(bool findTarget = true){
			print (name+"Switching to Attack with findTarget = "+findTarget);
			//print ("canAttack: "+canAttack+ " inAttackRange: "+ inAttackRange + " hasLOS: "+ hasLOS + " userApproach: "+ userApproach);
			//if (aiController.target == null || aiController.target.tag != aiController.enemyTag){
			if (findTarget){
				aiController.GetClosest();
			}
		    currentState = GameState.Attack;
		}
		
		public void ExecuteCurrent()
		{
//			print ("Current state: "+currentState);
			switch(currentState)
			{
			case GameState.Idle:
				aiController.BeIdle();
				break;
			case GameState.Approach:
				aiController.Approach();
				break;
			case GameState.Attack:
				aiController.Attack ();
				break;
			case GameState.UserAttack:
				aiController.UserAttack ();
				break;
			case GameState.Hold:
				aiController.Hold ();
				break;
			case GameState.Patrol:
				aiController.Patrol ();
				break;
			case GameState.AttackApproach:
				aiController.AttackApproach();
				break;
			default:
				//If something goes wrong, default to idle
				aiController.BeIdle();
				break;
			}
		}
	}
}

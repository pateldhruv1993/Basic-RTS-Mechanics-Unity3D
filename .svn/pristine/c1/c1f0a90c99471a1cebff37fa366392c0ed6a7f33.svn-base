#pragma strict
var fuseDelay: float; //delay we'll use before the force is applied to it
var missileVelocity : float = 10;
var turn : float = 20;
var smokePrefab : ParticleSystem;
private var target : Transform;
var homingMissile : Rigidbody;
var targetTag: String;


function Start() {

	homingMissile = transform.rigidbody;
	Fire();

}
 
 
function FixedUpdate ()
{
	if (target == null || homingMissile == null)
		return;
	
	homingMissile.velocity = transform.forward * missileVelocity;
	
	var targetRotation = Quaternion.LookRotation(target.position - transform.position);
	homingMissile.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, turn));
	
}
 
function Fire ()
{
	//yield WaitForSeconds(fuseDelay);
	//rigidbody.isKinematic = true;
	var targets : GameObject[] = GameObject.FindGameObjectsWithTag(targetTag);
	var closestDist = Mathf.Infinity;
	var closest : GameObject;
	for (Target in targets) {
		var dist = (transform.position - Target.transform.position).sqrMagnitude;
		 
		if(dist < closestDist){
			closestDist = dist;
			closest = Target;
		}
	}
	target = closest.transform;
	//transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(closest.transform.position-transform.position), turn * Time.deltaTime);
	//transform.position += transform.forward * missileForce * Time.deltaTime;
	 
}
	 
function OnCollisionEnter(theCollision : Collision)
{
	if(theCollision.gameObject.name == "AssaultBot")
	{
		smokePrefab.emissionRate = 0.0f;
		//Destroy(missileMod.gameObject);
		Destroy(gameObject);
	}
}
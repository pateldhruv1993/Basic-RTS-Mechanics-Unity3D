#pragma strict
var missile : Rigidbody;

function Update () {
	if(Input.GetButtonDown("Fire1"))
	{
   		Instantiate(missile, transform.position,transform.rotation);
	}
}
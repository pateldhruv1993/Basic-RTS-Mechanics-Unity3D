using UnityEngine;
using System.Collections;

/**
 * This class deals manual camera controls 
 * We'll add more to it later in the semester
 */ 

public class CameraController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//Scroll the camera around using WASD or cursor keys
		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");
		Vector3 disp = new Vector3(x,0,z);
		transform.position += disp;

	}
}

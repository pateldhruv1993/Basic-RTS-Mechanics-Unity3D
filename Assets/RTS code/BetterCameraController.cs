using UnityEngine;
using System;
using System.Collections;

namespace Lab4 {
	public class BetterCameraController : MonoBehaviour {

		//Lookat target
		public Transform target;
		public Transform pivot;
		public float dampen = 0.5f;
		private UnitSelectMover unitController; 
		private Quaternion defaultOrientation;
		private bool follow = false;
		float rotDir = 0;

		float minFov = 35f;
		float maxFov = 70f;
		float sensitivity = 30f;
		float rotVal = 45;

		float minZoomAngle = 122;
		float maxZoomAngle = 147;


		// Use this for initialization
		void Start () {

			transform.LookAt(target);
			defaultOrientation = transform.rotation;
			unitController = GetComponent<UnitSelectMover>();
		}
		
		// Update is called once per frame
		void Update () {

			Vector3 currentRight = transform.rotation * Vector3.right;
			Vector3 defaultRight = defaultOrientation * Vector3.right;

			float horizAngle = Vector3.Angle(defaultRight, currentRight);
			float vertAngle = Vector3.Angle(transform.rotation * Vector3.forward, Vector3.up);

			float zoomVal = Input.GetAxis("CamZoom");
			float camRotVal = Input.GetAxis("CamRotate");

			//We can only zoom or rotate the camera - we can't do both at the same time
			if (zoomVal != 0){
				//Zoom in
				if (zoomVal < 0 && vertAngle > minZoomAngle){
					transform.RotateAround(pivot.position, currentRight,  zoomVal * 15 * Time.deltaTime);
				//Zoom out
				}else if (zoomVal > 0 && vertAngle < maxZoomAngle){
					transform.RotateAround(pivot.position, currentRight,  zoomVal * 15 * Time.deltaTime);
				}
			}else{
				//If not zooming, we can rotate
				if (camRotVal > 0 && horizAngle <= rotVal){
					transform.RotateAround(target.position, Vector3.up, rotVal * Time.deltaTime);
					rotDir = -1;		
				}else if (camRotVal < 0 && horizAngle <= rotVal){
					rotDir = 1;		
					transform.RotateAround(target.position, Vector3.up, -rotVal * Time.deltaTime);
				//Undo the rotation
				}else if (camRotVal == 0){
					if (horizAngle > 1){
						transform.RotateAround(target.position, Vector3.up, rotDir*rotVal*Time.deltaTime);
					}else{
						if (Mathf.Abs(horizAngle) <= 1){
							transform.RotateAround(target.position, Vector3.up, rotDir*horizAngle);
						}
					}
				}
			}


			//Scroll the camera around using WASD or cursor keys
			float x = Input.GetAxis("Horizontal");
			float z = Input.GetAxis("Vertical");
			if (x != 0 || z != 0)
				follow = false;

			Vector3 disp = new Vector3(x,0,z);

			disp *=dampen;

			target.position += disp;

			//Return an optional (or nullable) Vector3
			Vector3? groupPos = unitController.GetGroupPos();
			if (Input.GetKey (KeyCode.F) && (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl ))){
				follow = true;
			}else if (Input.GetKey (KeyCode.F)){
				follow = false;
				//If there's a control group, we can get its center position
				if (groupPos.HasValue)
					target.position = groupPos.Value; 
			}

			if (follow){
				//If there's a control group, we can get its center position
				if (groupPos.HasValue)
					target.position = groupPos.Value; 
			}
			//transform.position += disp;
			//transform.LookAt(target);


		}
	}
}

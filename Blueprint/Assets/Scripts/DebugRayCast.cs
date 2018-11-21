﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRayCast : MonoBehaviour {

	public Vector3 direction = Vector3.forward;
	public RaycastHit hit;
	public float maxDistance = 10;
	public LayerMask layermask;
	Camera cam;

	public Interactable focus;

	// Use this for initialization
	void Start () {
		GameObject cam1 = GameObject.Find("PlayerCamera");
	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawRay (transform.position, direction * maxDistance, Color.red);

		if (Physics.Raycast(transform.position, direction, out hit, maxDistance)) {
			SetFocus(hit.collider.GetComponent<Interactable>());
		}

		if (Input.GetMouseButtonDown(0)){
			Debug.Log ("Left-mouse click");
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			hit = new RaycastHit();
			if (Physics.Raycast(ray, out hit)){
				Debug.Log(hit.transform.gameObject);
			}
		}
	}

	void SetFocus (Interactable newFocus) {
		focus = newFocus;
	}
}

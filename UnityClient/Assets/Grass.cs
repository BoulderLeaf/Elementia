using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{

	private Animator _animator;
	
	// Use this for initialization
	void Start ()
	{
		_animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("Enter");
		_animator.Play("Shake");
	}

	private void OnCollisionEnter(Collision other)
	{
		Debug.Log("Enter collider");
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BillboardSprite : MonoBehaviour
{
	void LateUpdate()
	{
		var target = Camera.main.transform.position;
		target.y = transform.position.y;
		transform.LookAt(target);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveMaker : MonoBehaviour {

    public float speed = 10f;
    private float origZ;
    public float resetZ = 0f;
    public bool destroy = false;

	private void Start ()
    {
        origZ = transform.position.z;	
	}
	
	private void Update()
    {
        transform.Translate(0, 0, -speed * Time.deltaTime);
        if (transform.position.z < resetZ) {
            if (!destroy) {
                transform.Translate(0, 0, origZ);
            }
            else {
                Destroy(gameObject);
            }
        }
	}
}

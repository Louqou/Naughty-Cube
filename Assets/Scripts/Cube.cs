using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    private GameObject currCaptureCube;

    private bool falling = false;
    public bool Falling
    {
        get {
            return falling;
        }
        set {
            if (value) {
                GetComponent<Rigidbody>().isKinematic = false;
                GetComponent<Rigidbody>().useGravity = true;
                Push();
                falling = value;
            }
        }
    }

    public bool BeingDestroyed
    {
        get {
            return destroyCube;
        }
    }
    private bool destroyCube = false;
    private float destroySpeed = 4.0f;
    private float distLower = 1.01f;

    private void Update()
    {
        if (destroyCube) {
            LowerCube();
        }
    }

    private void Push()
    {
        float force = Random.Range(80, 120);
        GetComponent<Rigidbody>().AddForce(Vector3.back * force);
    }

    private void LowerCube()
    {
        float dist = Mathf.Min(destroySpeed * Time.deltaTime, distLower);
        transform.Translate(0, -dist, 0, Space.World);
        currCaptureCube.transform.Translate(0, -dist / 1.5f, 0, Space.World);
        distLower -= dist;
        if (distLower == 0.0f) {
            Destroy(currCaptureCube);
            Destroy(gameObject);
        }
    }

    public void DestroyCube()
    {
        currCaptureCube = Instantiate(Resources.Load<GameObject>("CaptureCube"), transform.position, Quaternion.identity);
        destroyCube = true;
    }
}

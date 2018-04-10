using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnEdge : MonoBehaviour {

    void OnTriggerExit(Collider collision)
    {
        Destroy(collision.gameObject);
    }
}
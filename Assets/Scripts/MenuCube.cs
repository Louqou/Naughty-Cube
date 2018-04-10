using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCube : MonoBehaviour
{
    public float velocity = 0.5f;
    public float waveHeight = 0.15f;

    private IEnumerator Wave()
    {
        bool reverse = false;
        float waveToTravel = waveHeight;
        float vel = velocity;

        while (true) {
            float dist = Mathf.Min(Mathf.Abs(vel) * Time.deltaTime, waveToTravel);
            transform.Translate(0, Mathf.Sign(vel) * dist, 0);
            waveToTravel -= dist;
            if (waveToTravel == 0f) {
                if (reverse) {
                    break;
                }
                waveToTravel = waveHeight;
                vel = -vel;
                reverse = true;
            }
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "WaveMaker") {
            StartCoroutine(Wave());
        }
    }
}

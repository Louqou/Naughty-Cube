using System.Collections;
using UnityEngine;

public class Flash : MonoBehaviour
{
    public GameObject toFlash;
    public GameObject mainCamera;

    private bool flashing;
    public bool Flashing {
        get {
            return flashing;
        }
        set {
            if (value && !flashing) {
                StartCoroutine(FlashObject());
            }
            flashing = value;
        }
    }

    public float speed = 2.3f;

    public IEnumerator FlashObject()
    {
        flashing = true;
        while(flashing) {
            toFlash.SetActive(true);
            mainCamera.GetComponent<Camera>().cullingMask = 0;
            mainCamera.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("UI");
            yield return new WaitForSeconds(speed);
            toFlash.SetActive(false);
            mainCamera.GetComponent<Camera>().cullingMask = -1;
            yield return new WaitForSeconds(speed);
        }
    }
}

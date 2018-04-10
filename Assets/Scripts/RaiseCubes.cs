using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseCubes : MonoBehaviour
{
    private GameObject[,] cubes;
    public GameObject[,] Cubes
    {
        set {
            cubes = value;
            SetUp();
        }
    }
    public AudioClip cubeRowRisenSound;

    private bool setUpDone = false;
    private bool risingDone;
    public bool RisingDone
    {
        get {
            return risingDone;
        }
    }

    private int risingRow;
    public int RisingRow
    {
        get {
            return risingRow;
        }
    }

    private float riseTime;
    private float riseSpeed = 1.0f;
    private float riseDelay = 0.5f;
    private float[] rowRisen;

    private PuzzleCamera puzzleCamera;

    private void Start()
    {
        puzzleCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PuzzleCamera>();
    }

    private void SetUp()
    {
        riseTime = Time.time;
        rowRisen = new float[cubes.GetLength(1)];
        risingRow = 0;
        risingDone = false;
        setUpDone = true;
    }

    private void Update()
    {
        if (setUpDone && !risingDone && !puzzleCamera.cameraMoving) {
            RaiseWaveCubes();
        }
    }

    private void RaiseWaveCubes()
    {
        if (Time.time >= riseTime + riseDelay && risingRow < cubes.GetLength(1)) {
            risingRow++;
            riseTime = Time.time;
        }

        for (int j = 0; j < risingRow; j++) {
            if (rowRisen[j] < 1.0f) {
                float dist = Mathf.Min(riseSpeed * Time.deltaTime, 1.0f - rowRisen[j]);
                rowRisen[j] += dist;
                for (int i = 0; i < cubes.GetLength(0); i++) {
                    cubes[i, j].transform.Translate(new Vector3(0, dist, 0));
                }
            }
            else if (rowRisen[j] < 2.0f) {
                rowRisen[j] = 3.0f;
                GetComponent<AudioSource>().PlayOneShot(cubeRowRisenSound);
            }
        }

        if (rowRisen[rowRisen.Length - 1] >= 1.0f) {
            risingDone = true;
            setUpDone = false;
        }
    }
}

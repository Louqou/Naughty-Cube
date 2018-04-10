using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingCubes : MonoBehaviour
{
    public AudioClip blockRotateSound;

    private GameObject[,] cubes;
    public GameObject[,] Cubes
    {
        set {
            cubes = value;
            Restart();
        }
    }

    private Vector3[] corners;
    private Stage stage;

    private float rotateSpeed = 140;
    private float rotateSpeedFast = 250;

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
    private float rotateDelay = 2.5f;
#else
    private float rotateDelay = 1.7f;
#endif

    private float rotateDelayFast = 0.1f;
    private float nextRotateTime;

    private bool speedUp = false;
    public bool SpeedUp
    {
        set {
            if (!speedUp && value == true) {
                if (Time.time < nextRotateTime) {
                    nextRotateTime = Time.time + 0.2f;
                }
            }

            speedUp = value;
        }
        get {
            return speedUp;
        }
    }

    public bool RotateEnabled { get; set; }
    public bool stopNextRotate;
    private float totalCubeRotate = 0.0f;

    private bool demoStage;

    private void Start()
    {
        RotateEnabled = false;
        stage = GetComponent<Stage>();
        if (stage == null && GetComponent<DemoStageController>() != null) {
            demoStage = true;
        }
    }

    private void Update()
    {
        if (RotateEnabled && (demoStage || stage.CubesLeft > 0) && cubes != null) {
            if (Time.time > nextRotateTime) {
                if (!demoStage) {
                    stage.scoreMod = 0;
                }
                CubeMovement(CalcRotationAmount());
            }
            else {
                CubeMovement(0.0f);
            }
        }
    }

    private void CubeMovement(float rotation)
    {
        totalCubeRotate += rotation;
        for (int i = 0; i < cubes.GetLength(0); i++) {
            for (int j = 0; j < cubes.GetLength(1); j++) {
                if (cubes[i, j] && !cubes[i, j].GetComponent<Cube>().Falling && !cubes[i, j].GetComponent<Cube>().BeingDestroyed) {
                    if (rotation != 0.0f) {
                        cubes[i, j].transform.RotateAround(corners[j], Vector3.right, -rotation);
                    }
                    if (CubeShouldFall(cubes[i, j].transform.position.x, cubes[i, j].transform.position.z)) {
                        cubes[i, j].GetComponent<Cube>().Falling = true;
                        if (!demoStage) {
                            stage.CubeFallen(cubes[i, j]);
                        }
                    }
                }
            }
        }
        if (totalCubeRotate >= 90.0f) {
            BlocksFullyRotated();
        }
    }

    private float CalcRotationAmount()
    {
        return Mathf.Min((speedUp ? rotateSpeedFast : rotateSpeed) * Time.deltaTime, 90.0f - totalCubeRotate);
    }

    private void BlocksFullyRotated()
    {
        GetComponent<AudioSource>().PlayOneShot(blockRotateSound);
        totalCubeRotate = 0.0f;
        IncrementRotationVectors();
        CalculateNextRotateTime();

        if (stopNextRotate) {
            RotateEnabled = false;
            stopNextRotate = false;
        }
    }

    private void IncrementRotationVectors()
    {
        for (int j = 0; j < corners.Length; j++) {
            corners[j].z -= 1;
        }
    }

    private void Restart()
    {
        if (cubes != null) {
            corners = new Vector3[cubes.GetLength(1)];
            CalculateRotationVectors();
            CalculateNextRotateTime();
            totalCubeRotate = 0.0f;
            RotateEnabled = true;
        }
    }

    private void CalculateNextRotateTime()
    {
        nextRotateTime = Time.time + (speedUp ? rotateDelayFast : rotateDelay);
    }

    private void CalculateRotationVectors()
    {
        Transform trans;
        for (int j = 0; j < corners.Length; j++) {
            trans = cubes[0, j].transform;
            corners[j] = new Vector3(trans.position.x - trans.localScale.x / 2,
            trans.position.y - trans.localScale.y / 2,
            trans.position.z - trans.localScale.z / 2);
        }
    }

    public void ResetDelay()
    {
        if (Time.time < nextRotateTime) {
            nextRotateTime = Time.time + (speedUp ? rotateDelayFast : rotateDelay);
        }
    }

    private bool CubeShouldFall(float x, float z)
    {
        return z < (GetComponent<FloorBlocks>().LevelEnd - GetComponent<FloorBlocks>().LevelLength) - 0.2f;
    }
}

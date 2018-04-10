using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCubeSpawner : MonoBehaviour
{
    public GameObject normal;
    private GameObject[,] cubes;
    private float speed = 2f;
    private int width = 5;
    private int length;
    private int zEnd = -5;

    private void Awake()
    {
        length = (int)transform.position.z - zEnd;
        cubes = new GameObject[width, length];
        CreateRoad();
    }

    private void Update()
    {
        MoveHori();
    }

    private void CreateRoad()
    {
        for (int x = 0; x < width; x++) {
            for (int z = 0; z < length; z++) {
                cubes[x, z] = Instantiate(normal, new Vector3(x + 0.5f, -0.5f, z + zEnd + 0.5f), Quaternion.identity);
            }
        }
    }

    private void MoveHori()
    {
        for (int x = 0; x < width; x++) {
            for (int z = 0; z < length; z++) {
                cubes[x, z].transform.Translate(0, 0, -speed * Time.deltaTime);
                if (cubes[x, z].transform.position.z <= zEnd) {
                    float extra = cubes[x, z].transform.position.z - zEnd;

                    cubes[x, z].transform.position = new Vector3(
                        cubes[x, z].transform.position.x,
                        cubes[x, z].transform.position.y,
                        transform.position.z
                        );

                    cubes[x, z].transform.Translate(0, 0, extra);
                }
            }
        }
    }
}

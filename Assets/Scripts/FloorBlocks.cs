using System.Collections;
using UnityEngine;
using UnityEngine.PostProcessing;

public class FloorBlocks : MonoBehaviour
{
    public GameObject floorCube;

    private GameObject[,,] floorCubes;
    private int levelWidth;
    public int LevelWidth
    {
        get {
            return levelWidth;
        }
    }
    private int levelDepth = 1;
    private int levelLength;
    public int LevelLength
    {
        get {
            return levelLength;
        }
    }
    private int levelEnd;
    public int LevelEnd
    {
        get {
            return levelEnd;
        }
    }

    private LevelLoader levelLoader;

    private bool movingNewRow = false;
    private float moveRowSpeed = 5.0f;
    private float moveRowDistance = 0.0f;

    public AudioClip destoryRowSound;
    private float lastDestroyRowSound;
    private float destroyRowSoundDelay = 0.5f;

    private int[] stageLen = { 5, 7, 7, 7, 10, 10, 15, 15, 17 };

    private PPEffects ppEffects;

    private void Awake()
    {
        ppEffects = GetComponent<PPEffects>();
        lastDestroyRowSound = Time.time;
        levelLoader = GetComponent<LevelLoader>();
        levelWidth = levelLoader.LevelWidth;
        levelLength = levelLoader.CalcPuzzleLen() * levelLoader.NumPuzzles + stageLen[LevelLoader.stage - 1];
        levelEnd = levelLength;
        CreateStaticBlocks();
    }

    private void Update()
    {
        if (movingNewRow) {
            MoveNewRow();
        }
    }

    private void CreateStaticBlocks()
    {
        floorCubes = new GameObject[levelWidth, levelDepth, levelLength + levelLoader.NumPuzzles * levelLoader.NumWaves];
        for (int i_x = 0; i_x < levelWidth; i_x++) {
            for (int i_y = 0; i_y < levelDepth; i_y++) {
                for (int i_z = 0; i_z < levelLength; i_z++) {
                    floorCubes[i_x, i_y, i_z + levelLoader.NumPuzzles * levelLoader.NumWaves] =
                        Instantiate(floorCube, new Vector3(i_x + 0.5f, -i_y - 0.5f, i_z + 0.5f), Quaternion.identity);
                }
            }
        }
    }

    private void MoveNewRow()
    {
        GameObject currentCube;
        float distanceToMove = Mathf.Min(moveRowSpeed * Time.deltaTime, moveRowDistance);

        for (int i_x = 0; i_x < levelWidth; i_x++) {
            for (int i_y = 0; i_y < levelDepth; i_y++) {
                currentCube = floorCubes[i_x, i_y, floorCubes.GetLength(2) - (levelLength + 1)];
                currentCube.transform.position = new Vector3(
                    currentCube.transform.position.x,
                    currentCube.transform.position.y,
                    currentCube.transform.position.z + distanceToMove
                    );
            }
        }
        moveRowDistance -= distanceToMove;
        if (moveRowDistance <= 0.0f) {
            movingNewRow = false;
            levelLength++;
        }
    }

    public void AddRow()
    {
        for (int i_x = 0; i_x < levelWidth; i_x++) {
            for (int i_y = 0; i_y < levelDepth; i_y++) {
                floorCubes[i_x, i_y, floorCubes.GetLength(2) - (levelLength + 1)] =
                    Instantiate(floorCube, new Vector3(i_x + 0.5f, -i_y - 0.5f, levelEnd - LevelLength - 7), Quaternion.identity);
            }
        }
        moveRowDistance = 6.5f;
        movingNewRow = true;
    }

    public void DestroyRow()
    {
        ppEffects.StartChromEffect(false);
        ppEffects.StartColorEffect(false);
        for (int i_x = 0; i_x < levelWidth; i_x++) {
            for (int i_y = 0; i_y < levelDepth; i_y++) {
                floorCubes[i_x, i_y, floorCubes.GetLength(2) - levelLength].GetComponent<Cube>().Falling = true;
            }
        }

        if (levelLength > 1) {
            levelLength--;
        }

        if (Time.time > lastDestroyRowSound + destroyRowSoundDelay) {
            lastDestroyRowSound = Time.time;
            GetComponent<AudioSource>().PlayOneShot(destoryRowSound);
        }
    }
}

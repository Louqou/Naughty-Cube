using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject magicSquare;
    public GameObject destroySquare;
    public GameObject createdDestroySquare;

    private float createDestroyDelay = 0.2f;
    private float createDestroyTime = 0.0f;
    private float activateMagicTime = 0.0f;

    public GameObject characterContainer;
    public GameObject characterAnimation;
    private Vector3 characterScale;
    private Vector3 characterPosition;

    public AudioClip removeDestroySquareSound;
    public AudioClip placeSquareSound;

    private Stage stage;
    private RotatingCubes rotatingCubes;
    private FloorBlocks floorBlocks;
    private RaiseCubes raiseCubes;
    private Pause pause;

    private bool playerOutOfBounds = false;
    public bool PlayerOutOfBounds
    {
        get {
            return playerOutOfBounds;
        }
    }
    private bool playerCrushed = false;
    public bool PlayerCrushed
    {
        get {
            return playerCrushed;
        }
    }

    public bool DisableActions
    {
        set {
            disableCapture = value;
            disableMagic = value;
        }
    }

    public bool disableCapture;
    public bool disableMagic;
    public bool capturePressed;
    public bool magicPressed;

    private bool demoStage;

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
    private float doubleTouchTime;
    private float touchSpeedUpDelay = 0.3f;
#endif

    private void Start()
    {
        GameObject stageGo = GameObject.FindGameObjectWithTag("Stage");
        raiseCubes = stageGo.GetComponent<RaiseCubes>();
        stage = stageGo.GetComponent<Stage>();
        if (stage == null && stageGo.GetComponent<DemoStageController>() != null) {
            demoStage = true;
        }
        floorBlocks = stageGo.GetComponent<FloorBlocks>();
        rotatingCubes = stageGo.GetComponent<RotatingCubes>();
        pause = stageGo.GetComponent<Pause>();
        characterScale = characterContainer.transform.localScale;
        characterPosition = characterContainer.transform.position;
    }

    private void Update()
    {
        if (!playerOutOfBounds) {
            playerOutOfBounds = PlayerPosOutOfBounds();
            if (playerOutOfBounds) {
                PlayerFall();
            }
            else if (!playerCrushed && raiseCubes.RisingDone) {
                Actions();
            }
        }
    }

    public void PlayerFall()
    {
        stage.GameOver = true;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PuzzleCamera>().playerFalling = true;
    }

    private void LateUpdate()
    {
        magicPressed = false;
        capturePressed = false;
    }

    public void CrushPlayer()
    {
        if (!playerCrushed) {
            rotatingCubes.SpeedUp = true;
            characterScale = characterContainer.transform.localScale;
            characterPosition = characterContainer.transform.position;
            playerCrushed = true;
            characterContainer.transform.localScale = new Vector3(characterContainer.transform.localScale.x * 1.5f,
                0.1f,
                characterContainer.transform.localScale.z * 1.5f);
            characterContainer.transform.position = new Vector3(
                characterContainer.transform.position.x,
                0.04f,
                characterContainer.transform.position.z
                );
            characterAnimation.GetComponent<Animator>().enabled = false;
        }
    }

    public void UncrushPlayer()
    {
        if (playerCrushed) {
            playerCrushed = false;
            rotatingCubes.SpeedUp = false;
            characterContainer.transform.localScale = characterScale;
            characterContainer.transform.position = characterPosition;
            characterAnimation.GetComponent<Animator>().enabled = true;
        }
    }

    private void Actions()
    {
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        TouchScreenControl();
#else
        NonTouchScreenControl();
#endif
    }

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
    private void TouchScreenControl()
    {
        if (Input.touchCount == 2) {
            if (doubleTouchTime >= touchSpeedUpDelay) {
                rotatingCubes.SpeedUp = true;
            }
            else {
                doubleTouchTime += Time.deltaTime;
            }
        }
        else {
            doubleTouchTime = 0;
            rotatingCubes.SpeedUp = false;
        }
    }
#endif

    private void NonTouchScreenControl()
    {
        if (Input.GetKeyDown("q")) {
            CreateDestroySquareAction();
        }

        if (Input.GetKeyDown("e")) {
            ActivateMagicSquares();
        }

        if (Input.GetButton("Fire3")) {
            rotatingCubes.SpeedUp = true;
        }
        else {
            rotatingCubes.SpeedUp = false;
        }
    }

    private bool PlayerPosOutOfBounds()
    {
        Vector3 pos = gameObject.transform.position;
        return pos.x < 0 || pos.x > floorBlocks.LevelWidth || pos.z > floorBlocks.LevelEnd || pos.z < floorBlocks.LevelEnd - floorBlocks.LevelLength;
    }

    private void ActivateDestroySquare()
    {
        GameObject cube = CubeAboveSquare(createdDestroySquare.transform.position);
        Destroy(createdDestroySquare);
        if (cube) {
            DestroyCube(cube);
            rotatingCubes.ResetDelay();
            GetComponent<AudioSource>().time = 0.1f;
            GetComponent<AudioSource>().Play();
        }
        else {
            GetComponent<AudioSource>().PlayOneShot(removeDestroySquareSound, 0.6f);
        }
    }

    public void ActivateMagicSquares()
    {
        if (!disableMagic && !playerCrushed && raiseCubes.RisingDone && !pause.Paused
            && Time.time >= activateMagicTime + createDestroyDelay) {
            magicPressed = true;
            activateMagicTime = Time.time;
            GetComponent<AudioSource>().time = 0.1f;
            GetComponent<AudioSource>().Play();

            GameObject[] allMagicSquares = GameObject.FindGameObjectsWithTag("MagicSquare");
            Vector3[] allMagicPositions = new Vector3[allMagicSquares.Length];

            // Deactivate current magic squares
            // Needs to be done before activation as magic squares will not overlap
            for (int go = 0; go < allMagicSquares.Length; go++) {
                allMagicPositions[go] = allMagicSquares[go].transform.position;
                allMagicSquares[go].SetActive(false);
            }

            for (int pos = 0; pos < allMagicPositions.Length; pos++) {
                ActivateMagicSquare(allMagicPositions[pos]);
            }

            for (int go = 0; go < allMagicSquares.Length; go++) {
                Destroy(allMagicSquares[go]);
            }
        }
    }

    private void ActivateMagicSquare(Vector3 squarePos)
    {
        bool protect = createdDestroySquare != null
            && squarePos.x == createdDestroySquare.transform.position.x
            && squarePos.z == createdDestroySquare.transform.position.z;

        if (!protect) {
            GameObject cube = CubeAboveSquare(squarePos);
            if (cube != null) {
                DestroyCube(cube);
            }
        }
        else {
            Destroy(createdDestroySquare);
        }

        rotatingCubes.ResetDelay();
    }

    public void CreateDestroySquareAction()
    {
        if (!disableCapture && !playerCrushed && raiseCubes.RisingDone && !pause.Paused
            && Time.time >= createDestroyTime + createDestroyDelay) {
            capturePressed = true;
            createDestroyTime = Time.time;
            if (createdDestroySquare == null) {
                Vector3 pos = new Vector3(
                                    RoundToHalf(gameObject.transform.position.x),
                                    0.003f,
                                    RoundToHalf(gameObject.transform.position.z));
                if (!SquareOutOfBounds(pos)) {
                    createdDestroySquare = Instantiate(destroySquare, pos, Quaternion.identity);
                    GetComponent<AudioSource>().PlayOneShot(placeSquareSound, 0.6f);
                }
            }
            else {
                ActivateDestroySquare();
            }
        }
    }

    private void PlaceMagicSquare(Vector3 cubePosition)
    {
        GameObject[] currMagicSquares = GameObject.FindGameObjectsWithTag("MagicSquare");
        for (int x = -1; x < 2; x++) {
            for (int z = -1; z < 2; z++) {
                Vector3 place = new Vector3(RoundToHalf(cubePosition.x) + x, 0.002f, RoundToHalf(cubePosition.z) + z);
                if (!SquareOutOfBounds(place) && !MagicSquareAlreadyExists(place, currMagicSquares)) {
                    Instantiate(magicSquare, place, Quaternion.identity);
                }
            }
        }
    }

    private bool MagicSquareAlreadyExists(Vector3 location, GameObject[] currMagicSquares)
    {
        bool found = false;
        foreach (GameObject go in currMagicSquares) {
            if (go.transform.position == location && go.activeSelf) {
                found = true;
                break;
            }
        }
        return found;
    }

    private GameObject CubeAboveSquare(Vector3 squarePos)
    {
        RaycastHit hit;
        squarePos.y -= 0.1f;
        Vector3 end = new Vector3(squarePos.x, squarePos.y + 0.2f, squarePos.z);
        if (Physics.Linecast(squarePos, end, out hit) && hit.collider.gameObject.tag == "DyCube") {
            return hit.collider.gameObject;
        }

        return null;
    }

    private void DestroyCube(GameObject cube)
    {
        if (cube.name == "NaughtyCube(Clone)") {
            floorBlocks.DestroyRow();
            RowDestroyed();
        }
        else if (cube.name == "MagicCube(Clone)") {
            PlaceMagicSquare(cube.transform.position);
        }

        if (!demoStage) {
            stage.PlayerDestroyedCube(cube);
        }
        cube.GetComponent<Cube>().DestroyCube();
    }

    private bool SquareOutOfBounds(Vector3 pos)
    {
        return pos.x < 0
            || pos.x > floorBlocks.LevelWidth
            || pos.z < floorBlocks.LevelEnd - floorBlocks.LevelLength
            || pos.z > floorBlocks.LevelEnd;
    }

    public void PuzzleEnded()
    {
        if (createdDestroySquare) {
            Destroy(createdDestroySquare);
        }

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("MagicSquare")) {
            Destroy(go);
        }

        UncrushPlayer();
    }

    public void RowDestroyed()
    {
        if (createdDestroySquare && SquareOutOfBounds(createdDestroySquare.transform.position)) {
            Destroy(createdDestroySquare);
        }

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("MagicSquare")) {
            if (SquareOutOfBounds(go.transform.position)) {
                Destroy(go);
            }
        }

    }

    private float RoundToHalf(float toRound)
    {
        return Mathf.Round(toRound + 0.5f) - 0.5f;
    }
}
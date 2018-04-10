using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DemoStageController : MonoBehaviour
{
    public Text instructionText;

    private List<string> instructions;

    public GameObject mainCam;
    public GameObject player;
    public GameObject nextArrow;
    public GameObject greenSquare;
    public GameObject magicCube;
    public GameObject naughtyCube;
    public GameObject woopsText;
    public GameObject touchButtons;

    private GameObject placedGreenSquare;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    private bool camLookAt;
    private int instructionStep;
    private bool woopsieDone;

    GameObject[,] demoWave;
    public GameObject floorCube;

    private MyMovement playerMovement;
    private RotatingCubes rotatingCubes;
    private PlayerController playerController;

    private void Awake()
    {
        mainCam.transform.position = new Vector3(
            GetComponent<FloorBlocks>().LevelWidth / 2.0f, 5.5f, -2.25f
            );
    }

    private void Update()
    {
        if (!woopsieDone && playerController.PlayerCrushed) {
            woopsieDone = true;
            woopsText.GetComponent<Animator>().SetTrigger("PerfectPuzzle");
        }
    }

    private void Start()
    {
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        touchButtons.SetActive(true);
#endif
        GetInstructions();
        playerController = player.GetComponent<PlayerController>();
        playerController.DisableActions = true;
        rotatingCubes = GetComponent<RotatingCubes>();
        playerMovement = player.GetComponent<MyMovement>();
        playerMovement.playerMoveInputDisabled = true;
        StartCoroutine(WaitForCamera());
    }

    public void Next()
    {
        instructionText.text = instructions[instructionStep];
        nextArrow.SetActive(false);
        switch (++instructionStep) {
            case 1:
                CameraLookAt(player.transform, new Vector3(1, 2, -2), true, true);
                break;
            case 2:
                camLookAt = false;
                playerMovement.playerMoveInputDisabled = false;
                break;
            case 3:
                CreateDemoLevelBlocks();
                playerMovement.playerMoveInputDisabled = true;
                CameraLook(new Vector3(4, 3.6f, 3.7f), new Vector3(26.6f, -14.8f, 0), false, false);
                StartCoroutine(WaitForCubesToRise());
                break;
            case 4:
                rotatingCubes.Cubes = demoWave;
                rotatingCubes.stopNextRotate = true;
                StartCoroutine(WaitForCubesToRotate(EnableArrow));
                break;
            case 5:
                camLookAt = false;
                playerMovement.playerMoveInputDisabled = false;
                playerController.disableCapture = false;
                StartCoroutine(WaitForCaptureAndRemoveCapture());
                break;
            case 6:
                playerController.disableCapture = false;
                playerMovement.playerMoveInputDisabled = false;
                WaitForCaptureOnGreen(new Vector3(2.5f, 6.5f));
                break;
            case 7:
                playerMovement.playerMoveInputDisabled = false;
                StartCoroutine(RotateOnceAvoidPlayer(new Action(() =>
                {
                    nextArrow.SetActive(true);
                    playerMovement.playerMoveInputDisabled = true;
                })));
                break;
            case 8:
                StartCoroutine(WaitForCubesToRotate(() =>
                {
                    StartCoroutine(WaitForCapture("q", EnableArrow));
                }));
                break;
            case 9:
                int x = 1, y = 2;
                PlaceSpecialCube(magicCube, x, y);
                CameraLookAt(demoWave[x, y].transform, new Vector3(1, 2, -2), true, false);
                break;
            case 10:
                camLookAt = false;
                PlayerCaptureCube(new Vector2(1.5f, 5.5f), "q", EnableArrow);
                break;
            case 11:
                CameraLookAt(demoWave[1, 1].transform, new Vector3(2f, 2.5f, -4.5f), true, true);
                playerController.DisableActions = true;
                playerMovement.playerMoveInputDisabled = true;
                break;
            case 12:
                StartCoroutine(WaitForCapture("e", EnableArrow));
                break;
            case 13:
                camLookAt = false;
                break;
            case 14:
                x = 2; y = 0;
                PlaceSpecialCube(naughtyCube, x, y);
                CameraLookAt(demoWave[x, y].transform, new Vector3(1, 2, -2), true, false);
                break;
            case 15:
                camLookAt = false;
                PlaceSpecialCube(magicCube, 3, 2);
                PlayerCaptureCube(new Vector2(3.5f, 4.5f), "q", new Action(() =>
                {
                    PlayerCaptureCube(new Vector2(2.5f, 5.5f), "e", EnableArrow);
                }));
                break;
            case 16:
                CameraLookAt(demoWave[2, 0].transform, new Vector3(1, 2, -2), true, false);
                break;
            case 17:
                camLookAt = false;
                PlayerCaptureCube(new Vector2(0.5f, 4.5f), "q", new Action(() =>
                {
                    PlayerCaptureCube(new Vector2(1.5f, 3.5f), "q", EnableArrow);
                }));
                break;
            case 18:
                playerController.DisableActions = true;
                StartCoroutine(RotateAvoidPlayer());
                StartCoroutine(WaitForCubeToFall(demoWave[2, 0].GetComponent<Cube>()));
                break;
            case 19:
                StartCoroutine(FinishDemo());
                break;
            default:
                break;
        }
    }

    private IEnumerator FinishDemo()
    {
        yield return new WaitForSeconds(2.0f);
        float fadeTime = gameObject.GetComponent<Fading>().BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene("Scenes/Menu", LoadSceneMode.Single);
    }

    private IEnumerator WaitForCubeToFall(Cube cube)
    {
        while (!cube.Falling) {
            yield return null;
        }
        nextArrow.SetActive(true);
    }

    private void EnableArrow()
    {
        nextArrow.SetActive(true);
    }

    private void PlayerCaptureCube(Vector2 capCoords, String key, Action action)
    {
        playerMovement.playerMoveInputDisabled = false;
        PlaceGreenSquare(capCoords);
        StartCoroutine(WaitForCaptureOnSquare(capCoords,
            new Action(() =>
            {
                Destroy(placedGreenSquare);
                playerController.DisableActions = true;
                StartCoroutine(RotateOnceAvoidPlayer(new Action(() =>
                {
                    playerMovement.playerMoveInputDisabled = false;
                    StartCoroutine(WaitForCapture(key, action));
                })));
            })));
    }

    private IEnumerator RotateOnceAvoidPlayer(Action action)
    {
        while (playerMovement.ForwardBlocked()) {
            yield return null;
        }
        playerMovement.playerMoveInputDisabled = true;
        rotatingCubes.RotateEnabled = true;
        rotatingCubes.stopNextRotate = true;
        StartCoroutine(WaitForCubesToRotate(action));
    }

    private IEnumerator RotateAvoidPlayer()
    {
        while (playerMovement.ForwardBlocked()) {
            yield return null;
        }
        rotatingCubes.RotateEnabled = true;
    }

    private void PlaceSpecialCube(GameObject cube, int x, int y)
    {
        GameObject oldCube = demoWave[x, y];

        demoWave[x, y] = Instantiate(cube,
            new Vector3(oldCube.transform.position.x,
            oldCube.transform.position.y,
            oldCube.transform.position.z), Quaternion.identity);

        Destroy(oldCube);
    }

    private IEnumerator WaitForPlayerToMoveBack(float zPos, Action action)
    {
        while (player.transform.position.z > zPos) {
            yield return null;
        }
        action();
    }

    private IEnumerator WaitForCaptureOnSquare(Vector2 square, Action action)
    {
        bool placedOnSquare = false;
        playerController.disableCapture = false;
        while (!placedOnSquare) {
            placedOnSquare =
                playerController.createdDestroySquare
                && square.x == playerController.createdDestroySquare.transform.position.x
                && square.y == playerController.createdDestroySquare.transform.position.z;
            yield return null;
        }
        action();
    }

    private void WaitForCaptureOnGreen(Vector2 pos)
    {
        PlaceGreenSquare(pos);
        StartCoroutine(WaitForCaptureOnSquare(pos, new Action(() =>
        {
            Destroy(placedGreenSquare);
            playerController.DisableActions = true;
            playerMovement.playerMoveInputDisabled = true;
            nextArrow.SetActive(true);
        })));
    }

    private IEnumerator WaitForCapture(string key, Action action)
    {
        if (key == "e") {
            playerController.disableMagic = false;
            while (!playerController.magicPressed) {
                yield return null;
            }
        }
        else if (key == "q") {
            playerController.disableCapture = false;
            while (!playerController.capturePressed) {
                yield return null;
            }
        }
        action();
    }

    private IEnumerator WaitForCaptureAndRemoveCapture()
    {
        yield return StartCoroutine(WaitForCapture("q", new Action(()=> {})));
        yield return null;
        yield return StartCoroutine(WaitForCapture("q", new Action(() =>
        {
            playerController.disableCapture = true;
            playerMovement.playerMoveInputDisabled = true;
            nextArrow.SetActive(true);
        })));
    }

    private IEnumerator WaitForCamera()
    {
        PuzzleCamera puzzleCamera = mainCam.GetComponent<PuzzleCamera>();
        while (puzzleCamera.cameraMoving) {
            yield return null;
        }
        nextArrow.SetActive(true);
    }

    private IEnumerator WaitForCubesToRotate(Action action)
    {
        while (rotatingCubes.RotateEnabled) {
            yield return null;
        }
        action();
    }

    private IEnumerator WaitForCubesToRise()
    {
        RaiseCubes raiseCubes = GetComponent<RaiseCubes>();
        while (!raiseCubes.RisingDone) {
            yield return null;
        }
        nextArrow.SetActive(true);
    }

    private void PlaceGreenSquare(Vector2 position)
    {
        Vector3 place = new Vector3(position.x, 0.004f, position.y);
        placedGreenSquare = Instantiate(greenSquare, place, Quaternion.identity);
    }

    private void CreateDemoLevelBlocks()
    {
        demoWave = new GameObject[4, 3];
        for (int i = 0; i < demoWave.GetLength(0); i++) {
            for (int j = 0; j < demoWave.GetLength(1); j++) {
                demoWave[i, j] =
                    Instantiate(
                        floorCube, new Vector3(i + 0.5f,
                        -0.505f,
                        11 - (j + 0.4905f)), Quaternion.identity);
            }
        }
        GetComponent<RaiseCubes>().Cubes = demoWave;
    }

    private void CameraLookAt(Transform target, Vector3 offset, bool setArrowStart, bool setArrowEnd)
    {
        Vector3 targetPos = target.position + offset;

        Quaternion targetRot = Quaternion.LookRotation(target.position - targetPos);
        StartCoroutine(CameraLook(targetPos, targetRot, setArrowStart, setArrowEnd));
    }

    private void CameraLook(Vector3 targetPos, Vector3 targetRot, bool setArrowStart, bool setArrowEnd)
    {
        StartCoroutine(CameraLook(targetPos, Quaternion.Euler(targetRot), setArrowStart, setArrowEnd));
    }

    private IEnumerator CameraLook(Vector3 targetPos, Quaternion targetRot, bool setArrowStart, bool setArrowEnd)
    {
        playerMovement.moveCamera = false;
        originalCamPos = mainCam.transform.position;
        originalCamRot = mainCam.transform.rotation;

        camLookAt = true;
        float movedPerc = 0;

        while (camLookAt && movedPerc < 1) {
            movedPerc += Time.deltaTime;

            mainCam.transform.position = Vector3.Lerp(
                originalCamPos,
                targetPos,
                movedPerc
                );

            mainCam.transform.rotation = Quaternion.Slerp(originalCamRot, targetRot, movedPerc);
            yield return null;

        }

        if (setArrowStart) {
            nextArrow.SetActive(true);
        }
        while (camLookAt) {
            yield return null;
        }
        if (setArrowStart) {
            nextArrow.SetActive(false);
        }

        movedPerc = 0;
        Transform currentTrans = mainCam.transform;

        while (movedPerc < 1) {
            movedPerc += Time.deltaTime;

            mainCam.transform.position = Vector3.Lerp(
                currentTrans.position,
                originalCamPos,
                movedPerc
                );

            mainCam.transform.rotation = Quaternion.Slerp(currentTrans.rotation, originalCamRot, movedPerc);
            yield return null;
        }
        playerMovement.moveCamera = true;
        if (setArrowEnd) {
            nextArrow.SetActive(true);
        }
    }

    private void GetInstructions()
    {
        instructions = new List<string>
        {
            "This is you!",

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            "Swipe to move.\nKeep your finger pressed to\nkeep moving.",
#else
            "Use <color=yellow>W A S D</color> to move.",
#endif

            "Cubes will rise from\nthe top of the level.",
            "Cubes will rotate forward.\nCapture them before they\nfall or a row of the level\nwill be destroyed.",

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            "- Press the <color=magenta>pink button</color> to \n  place a <color=magenta>capture square</color>.\n- Press it again to remove it.",
#else
            "- Press <color=yellow>Q</color> to place a\n  <color=magenta>capture square</color>.\n- Press <color=yellow>Q</color> again to remove it.",
#endif

            "Place a <color=magenta>capture square</color> on\nthe <color=green>green square</color>.",
            "Move out of the way of\nthe rotating cubes or you\nwill be crushed and all\nremaining cubes will fall.",

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            "Press the <color=magenta>pink button</color> to\ncapture any cube over\nthe <color=magenta>capture square</color>.",
#else
            "Press <color=yellow>Q</color> to capture any\ncube over the <color=magenta>capture square</color>.",
#endif

            "<color=#69AEFFFF>Magic cubes</color> can be used to\ncapture many cubes at once.",

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            "Capture the <color=#69AEFFFF>magic cube</color>.\nRemember to move back!",
#else
            "Capture the <color=#69AEFFFF>magic cube</color> (<color=yellow>Q</color>).\nRemember to move back!",
#endif

            "Capturing <color=#69AEFFFF>magic cubes</color> will\ncreate <color=#69AEFFFF>magic squares</color> in\nthe surrounding area!",

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            "Press the <color=#69AEFFFF>blue button</color>\nto capture all cubes\nabove <color=#69AEFFFF>magic squares</color>.",
#else
            "Press <color=yellow>E</color> to capture all cubes\nabove <color=#69AEFFFF>magic squares</color>.",
#endif

            "Capturing many cubes at\nonce will increase your score.",
            "This is a <color=red>naughty cube</color>!\nDo not capture <color=red>naughty cubes</color>\nor a row of the level will\nbe destroyed.",

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            "- Capture the <color=#69AEFFFF>magic cube</color>.\n- Get a <color=magenta>capture square</color>\n  under the <color=red>naughty cube</color>.\n- Press the <color=#69AEFFFF>blue button</color>.",
#else
            "- Capture the <color=#69AEFFFF>magic cube</color>.\n- Get a <color=magenta>capture square</color>\n  under the <color=red>naughty cube</color>.\n- Press <color=yellow>E</color>.",
#endif

            "The <color=red>naughty cube</color> was\nnot captured. The <color=magenta>capture</color>\n<color=magenta>square</color> shields the cube from\nthe <color=#69AEFFFF>magic square</color>.",
            "Capture the remaining\ncubes.",

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            "Allow the <color=red>naughty cube</color> to fall.\nPress two fingers to speed\nup the cubes.",
#else
            "Allow the <color=red>naughty cube</color> to fall.\nPress shift to speed up\nthe cubes.",
#endif

            "Demo complete, good luck!"
        };
    }
}

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

#else

#endif
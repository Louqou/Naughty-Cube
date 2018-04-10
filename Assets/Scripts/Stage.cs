using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

public class Stage : MonoBehaviour
{
    public GameObject floorCube;
    public GameObject normalCube;
    public GameObject naughtyCube;
    public GameObject magicCube;
    public GameObject mainCamera;
    public GameObject touchButtons;

    private GameObject[,] puzzleCubes;
    private GameObject[,] waveCubes;
    private int puzzleLen;

    private bool setUpDone = true;
    private bool setSpecialStarted = false;

    private int allowedFallen = 4;
    private int currentFallen;

    public AudioClip setSpecialSound;

    private int cubesLeft;
    public int CubesLeft
    {
        get {
            return cubesLeft;
        }
    }

    private int currentPuzzle;
    private bool perfectPuzzle = true;
    private bool lastPuzzlePerfect;
    private bool endWave = true;
    private bool gameOver = false;
    public bool GameOver
    {
        set {
            gameOver = value;
        }
    }
    private bool nextStage;
    private float nextStageDelay;
    private bool doNothing = false;
    private bool levelTooShort = false;

    private RotatingCubes rotatingCubes;
    private LevelLoader levelLoader;
    private FloorBlocks floorBlocks;
    private PlayerController playerController;
    private RaiseCubes raiseCubes;

    public static int score = 0;

    public Text scoreText;
    public Text modText;
    public int scoreMod = 0;
    public Text fallCountText;
    public Text perfectPuzzleText;
    public Text stageText;
    public GameObject pauseScreen;
    public GameObject playArrow;
    public GameObject pauseLines;

    public GameObject floorWaveMaker;

    PPEffects ppEffects;

    private void Awake()
    {
        floorBlocks = GetComponent<FloorBlocks>();
        mainCamera.transform.position = new Vector3(
            floorBlocks.LevelWidth / 2.0f, 5.5f, -2.25f);
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        touchButtons.SetActive(true);
#endif
    }

    private void Start()
    {
        ppEffects = GetComponent<PPEffects>();
        raiseCubes = GetComponent<RaiseCubes>();
        levelLoader = GetComponent<LevelLoader>();
        rotatingCubes = GetComponent<RotatingCubes>();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        stageText.text = "Level: " + LevelLoader.stage;
        ShowAd();
#if !UNITY_ADS
        GameObject.FindGameObjectWithTag("BackgroundMusic").GetComponent<BackMusic>().StartMusic();
#endif
    }

    private void ShowAd()
    {
#if UNITY_ADS
        Time.timeScale = 0;
        if (Advertisement.IsReady()) {
            var options = new ShowOptions { resultCallback = StartGame };
            Advertisement.Show(options);
        }
        else {
            StartGame(ShowResult.Skipped);
        }
#endif
    }
#if UNITY_ADS
    private void StartGame(ShowResult result)
    {
        Time.timeScale = 1;
        GameObject.FindGameObjectWithTag("BackgroundMusic").GetComponent<BackMusic>().StartMusic();
    }
#endif

    private void NewWave()
    {
        if (floorBlocks.LevelLength <= 1) {
            gameOver = true;
            return;
        }

        puzzleLen = levelLoader.CurrWaveBlocks[0].GetLength(1);
        currentPuzzle = 0;
        cubesLeft = puzzleLen * floorBlocks.LevelWidth;

        levelTooShort = floorBlocks.LevelLength <= puzzleLen * levelLoader.NumPuzzles + 1;
        if (!levelTooShort) {
            puzzleCubes = new GameObject[floorBlocks.LevelWidth, puzzleLen];
            waveCubes = new GameObject[floorBlocks.LevelWidth, puzzleLen * levelLoader.NumPuzzles];
        }
        else {
            ppEffects.StartChromEffect(true);
            ppEffects.StartColorEffect(true);
            GetComponent<Flash>().Flashing = true;
        }

        setUpDone = false;
        setSpecialStarted = false;

        currentFallen = 0;
        endWave = false;

        CreateWaveBlocks();
        CreateFloorWaveMaker();
        raiseCubes.Cubes = waveCubes;
    }

    private void CreateFloorWaveMaker()
    {
        GameObject fwm = Instantiate(floorWaveMaker, new Vector3(floorBlocks.LevelWidth / 2f, -1.15f, floorBlocks.LevelEnd + 15f), Quaternion.identity);
        WaveMaker waveMaker = fwm.GetComponent<WaveMaker>();
        waveMaker.destroy = true;
        waveMaker.speed = 8f;
        waveMaker.resetZ = floorBlocks.LevelEnd - floorBlocks.LevelLength;
    }

    private void Update()
    {
        if (doNothing) {
            return;
        }

        UpdateScoreText();

        if (nextStage) {
            if (!mainCamera.GetComponent<PuzzleCamera>().cameraMoving && nextStageDelay == 0) {
                StartNextStage();
            }
            else if (nextStageDelay != 0) {
                LoadNextStage();
            }
        }
        else if (gameOver) {
            StartCoroutine(GetComponent<Pause>().ShowGameOverScreen());
            doNothing = true;
        }
        else if (setUpDone) {
            FinishWave();
        }
        else if (raiseCubes.RisingDone && !setSpecialStarted) {
            FinishSetUp();
        }
    }

    private void StartNextStage()
    {
        float delay = mainCamera.GetComponent<PuzzleCamera>().CreateWaveMaker();
        nextStageDelay = Time.time + delay + 5f;
    }

    private void LoadNextStage()
    {
        if (Time.time > nextStageDelay) {
            ppEffects.RestoreDefault();
            LevelLoader.stage += 1;
            if (LevelLoader.stage <= 9) {
                SceneManager.LoadScene("Scenes/Stage", LoadSceneMode.Single);
            }
            else {
                SceneManager.LoadScene("Scenes/GameOver", LoadSceneMode.Single);
            }
        }
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString("N0");

        modText.text = "x " + scoreMod;
        if (scoreMod > 20) {
            modText.text += " Naughty!!";
        }
        else if (scoreMod > 15) {
            modText.text += " Wicked!";
        }
        else if (scoreMod > 10) {
            modText.text += " Nice!";
        }

        fallCountText.text = "";
        for (int i = 0; i < currentFallen % allowedFallen; i++) {
            fallCountText.text += "_";
        }
    }

    private void FinishWave()
    {
        if (!endWave && cubesLeft == 0) {
            EndOfPuzzle();
        }
        else if (endWave) {
            if (levelLoader.GetNextWave()) {
                NewWave();
            }
            else {
                nextStage = true;
            }
        }
    }

    private void FinishSetUp()
    {
        if (levelTooShort) {
            SetBlankRow();
            setUpDone = true;
            if (levelTooShort && GetComponent<Flash>().Flashing == true) {
                GetComponent<Flash>().Flashing = false;
            }
        }
        else {
            setSpecialStarted = true;
            StartCoroutine(SetSpecialCubesDelay());
        }
    }
    private void EndOfPuzzle()
    {
        rotatingCubes.RotateEnabled = false;
        if (perfectPuzzle) {
            perfectPuzzleText.GetComponent<Animator>().SetTrigger("PerfectPuzzle");
            floorBlocks.AddRow();
            mainCamera.GetComponent<PuzzleCamera>().StartEndPuzzleCamera();
        }

        if (++currentPuzzle < levelLoader.NumPuzzles) {
            cubesLeft = puzzleLen * floorBlocks.LevelWidth;
            ResetPuzzleCubes();
            StartCoroutine(SetSpecialCubesDelay());
        }
        else {
            endWave = true;
        }

        playerController.PuzzleEnded();
        lastPuzzlePerfect = perfectPuzzle;
        perfectPuzzle = true;
    }

    private void CreateWaveBlocks()
    {
        if (!levelTooShort) {
            CreateAllWaveBlocks();
        }
        else {
            CreateShortenedWaveBlocks();
        }
    }

    private void CreateAllWaveBlocks()
    {
        for (int p = 0; p < levelLoader.NumPuzzles; p++) {
            for (int i = 0; i < waveCubes.GetLength(0); i++) {
                for (int j = 0; j < puzzleLen; j++) {
                    if (j + (p * puzzleLen) < floorBlocks.LevelLength - 1) {
                        waveCubes[i, j + (p * puzzleLen)] =
                            Instantiate(floorCube, new Vector3(i + 0.5f,
                            -0.505f,
                            floorBlocks.LevelEnd - (j + (p * puzzleLen) + 0.4905f)), Quaternion.identity);
                    }
                    else {
                        waveCubes[i, j + (p * puzzleLen)] = null;
                    }

                    if (p == levelLoader.NumPuzzles - 1) {
                        puzzleCubes[i, j] = waveCubes[i, j + (p * puzzleLen)];
                    }
                }
            }
        }
    }

    private void CreateShortenedWaveBlocks()
    {
        int levelLength = floorBlocks.LevelLength;
        if (lastPuzzlePerfect) {
            levelLength++;
        }

        waveCubes = new GameObject[floorBlocks.LevelWidth, levelLength - 1];
        for (int i = 0; i < waveCubes.GetLength(0); i++) {
            for (int j = 0; j < waveCubes.GetLength(1); j++) {
                waveCubes[i, j] = Instantiate(floorCube, new Vector3(i + 0.5f,
                            -0.505f,
                            floorBlocks.LevelEnd - (j + 0.4905f)), Quaternion.identity);
            }
        }
    }

    private void ResetPuzzleCubes()
    {
        for (int i = 0; i < floorBlocks.LevelWidth; i++) {
            for (int j = 0; j < puzzleLen; j++) {
                puzzleCubes[i, j] = waveCubes[i, j + ((levelLoader.NumPuzzles - currentPuzzle - 1) * puzzleLen)];
            }
        }
    }

    private void SetBlankRow()
    {
        GetComponent<AudioSource>().PlayOneShot(setSpecialSound);
        GameObject oldCube;
        puzzleCubes = new GameObject[waveCubes.GetLength(0), 1];
        for (int i = 0; i < waveCubes.GetLength(0); i++) {
            oldCube = waveCubes[i, waveCubes.GetLength(1) - 1];
            puzzleCubes[i, 0] = Instantiate(normalCube, new Vector3(oldCube.transform.position.x,
                oldCube.transform.position.y, oldCube.transform.position.z),
                Quaternion.identity);
            Destroy(oldCube);
        }
        rotatingCubes.Cubes = puzzleCubes;
    }

    private IEnumerator SetSpecialCubesDelay()
    {
        yield return new WaitForSeconds(1);
        SetSpecialCubes();
        yield return new WaitForSeconds(2);
        setUpDone = true;
        rotatingCubes.Cubes = puzzleCubes;
    }

    private void SetSpecialCubes()
    {
        GetComponent<AudioSource>().PlayOneShot(setSpecialSound);
        GameObject newCube;
        GameObject oldCube;
        for (int i = 0; i < floorBlocks.LevelWidth; i++) {
            for (int j = 0; j < puzzleLen; j++) {
                if (levelLoader.CurrWaveBlocks[currentPuzzle][i, j] == Blocks.Naughty) {
                    newCube = naughtyCube;
                }
                else if (levelLoader.CurrWaveBlocks[currentPuzzle][i, j] == Blocks.Magic) {
                    newCube = magicCube;
                }
                else {
                    newCube = normalCube;
                }
                oldCube = puzzleCubes[i, j];
                puzzleCubes[i, j] = Instantiate(newCube,
                    new Vector3(oldCube.transform.position.x,
                    oldCube.transform.position.y,
                    oldCube.transform.position.z), Quaternion.identity);
                Destroy(oldCube);
            }
        }
    }

    public void CubeFallen(GameObject cube)
    {
        if (!cube.GetComponent<Cube>().BeingDestroyed) {
            bool isNaughty = cube.name == "NaughtyCube(Clone)";
            if (!isNaughty) {
                perfectPuzzle = false;
            }
            else if (isNaughty && !playerController.PlayerCrushed) {
                score += 10;
            }

            if (ShouldDestoryRow(isNaughty)) {
                floorBlocks.DestroyRow();
                playerController.RowDestroyed();
            }
            cubesLeft--;
        }
    }

    public bool ShouldDestoryRow(bool isNaughty)
    {
        return isNaughty && playerController.PlayerCrushed && ++currentFallen % allowedFallen == 0
            || !isNaughty && ++currentFallen % allowedFallen == 0;
    }

    public void PlayerDestroyedCube(GameObject cube)
    {
        if (!cube.GetComponent<Cube>().Falling) {
            if (cube.name == "NaughtyCube(Clone)") {
                perfectPuzzle = false;
            }
            else {
                score += 10 + scoreMod;
                scoreMod += 1;
            }
            cubesLeft--;
        }
    }
}
using UnityEngine;

public class PuzzleCamera : MonoBehaviour
{
    public Transform player;
    public GameObject waveMakerGo;
    private GameObject currWaveMaker;
    public GameObject endStageText;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private Vector3 moveToPos;
    private Quaternion lookAtRot;

    public bool cameraMoving;
    private bool endWaveView;
    private bool moveToBack;

    private FloorBlocks floorBlocks;

    private float moveTime = 0.7f;
    private float startTime;
    private float endPuzzleWaitTime = 1.5f;

    private float increaseScoreTime;
    private float scoreRowAdded;

    public bool playerFalling;

    private bool stageStartRot;

    private void Awake()
    {
        cameraMoving = true;
    }

    private void Start()
    {
        floorBlocks = GameObject.FindGameObjectWithTag("Stage").GetComponent<FloorBlocks>();
        moveToPos = transform.position;
        lookAtRot = transform.rotation;
        transform.rotation = Quaternion.Euler(new Vector3(-42f, gameObject.transform.rotation.eulerAngles.y, gameObject.transform.rotation.eulerAngles.z));
        StoreOriginalTransfrom();
        stageStartRot = true;
        startTime = Time.time;
        moveTime = 2f;
    }

    private void Update()
    {
        if (stageStartRot && MoveCamera()) {
            cameraMoving = false;
            stageStartRot = false;
        }
        else if (player && playerFalling) {
            transform.LookAt(player.transform.position);
        }
        else if (endWaveView) {
            if (moveToBack) {
                MoveToBackView();
            }
            else {
                ReturnCamera();
            }
        }
        else if (currWaveMaker) {
            SmoothMove();
            ShowEndLevelText();
            AddScore();
        }
    }

    private void AddScore()
    {
        if (scoreRowAdded < floorBlocks.LevelLength && Time.time > increaseScoreTime) {
            increaseScoreTime = Time.time + (1 / currWaveMaker.GetComponent<WaveMaker>().speed);
            scoreRowAdded++;
            Stage.score += 20;
        }
    }

    private void ShowEndLevelText()
    {
        if (Mathf.Round(transform.rotation.eulerAngles.x) > 80) {
            endStageText.SetActive(true);
            //cameraMoving = false;
        }
    }

    private void MoveToBackView()
    {
        if (MoveCamera() && Time.time > startTime + endPuzzleWaitTime) {
            SetUpReturn();
        }
    }

    private void SetUpReturn()
    {
        moveToBack = false;
        Vector3 tempPos = originalPos;
        Quaternion tempRot = originalRot;
        originalPos = moveToPos;
        originalRot = lookAtRot;
        moveToPos = tempPos;
        lookAtRot = tempRot;
        startTime = Time.time;
    }

    private void ReturnCamera()
    {
        if (MoveCamera()) {
            cameraMoving = false;
            endWaveView = false;
        }
    }

    private bool MoveCamera()
    {
        float fractionMoved = (Time.time - startTime) / moveTime;
        transform.position = Vector3.Lerp(originalPos, moveToPos, fractionMoved);
        transform.rotation = Quaternion.Lerp(originalRot, lookAtRot, fractionMoved);
        return fractionMoved >= 1;
    }

    private void CalcLookAtRot()
    {
        Vector3 lookAtPos = new Vector3(floorBlocks.LevelWidth / 2f, -0.5f, floorBlocks.LevelEnd - floorBlocks.LevelLength);
        lookAtRot = Quaternion.LookRotation(lookAtPos - moveToPos);
    }

    private void CalcMoveToPosition()
    {
        moveToPos = new Vector3(floorBlocks.LevelWidth, 5f, floorBlocks.LevelEnd - floorBlocks.LevelLength - 7f);
    }

    public void StartEndPuzzleCamera()
    {
        StoreOriginalTransfrom();
        CalcMoveToPosition();
        CalcLookAtRot();

        startTime = Time.time;
        cameraMoving = true;
        endWaveView = true;
        moveToBack = true;
        moveTime = 0.7f;
    }

    private void StoreOriginalTransfrom()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    private void SmoothMove()
    {
        transform.LookAt(currWaveMaker.transform, transform.up);
        transform.position = Vector3.Lerp(transform.position, moveToPos, Time.deltaTime);
    }

    public float CreateWaveMaker()
    {
        moveToPos = new Vector3(floorBlocks.LevelWidth / 2f, 5f, floorBlocks.LevelEnd - floorBlocks.LevelLength - 6.6f);
        cameraMoving = true;

        currWaveMaker = Instantiate(waveMakerGo, new Vector3(floorBlocks.LevelWidth / 2f, -1.15f, floorBlocks.LevelEnd), Quaternion.identity);
        WaveMaker waveMaker = currWaveMaker.GetComponent<WaveMaker>();
        waveMaker.destroy = true;
        waveMaker.speed = 4f;
        waveMaker.resetZ = floorBlocks.LevelEnd - floorBlocks.LevelLength - 7f;

        increaseScoreTime = Time.time + (1 / waveMaker.speed);

        return floorBlocks.LevelLength / waveMaker.speed;
    }
}

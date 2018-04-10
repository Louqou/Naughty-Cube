using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMovement : MonoBehaviour
{
    private MoveAxis xMove = new MoveAxis();
    private MoveAxis zMove = new MoveAxis();

    private float moveHorizontal;
    private float moveVertical;
    public bool playerMoveInputDisabled;

    public GameObject myCamera;

    public GameObject characterContainer;
    private PlayerController playerController;
    private FloorBlocks floorBlocks;
    private RaiseCubes raiseCubes;
    private PuzzleCamera endPuzzleCamera;

    private float rotationSpeed = 5.0f;
    public bool moveCamera = true;

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
    Vector2 firstTouchPos;
#endif

    private void Start()
    {
        GameObject stageGo = GameObject.FindGameObjectWithTag("Stage");
        raiseCubes = stageGo.GetComponent<RaiseCubes>();
        floorBlocks = stageGo.GetComponent<FloorBlocks>();
        playerController = GetComponent<PlayerController>();
        endPuzzleCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PuzzleCamera>();
    }

    private void Update()
    {
        MoveInput();
    }

    private void FixedUpdate()
    {
        if (!playerController.PlayerOutOfBounds && !playerController.PlayerCrushed && floorBlocks.enabled
            && !endPuzzleCamera.cameraMoving) {
            SetRotation();
            Movement();

            if (moveCamera) {
                float vel = 0;
                myCamera.transform.position = new Vector3(
                    myCamera.transform.position.x,
                    myCamera.transform.position.y,
                    Mathf.SmoothDamp(myCamera.transform.position.z, characterContainer.transform.position.z - 2.75f, ref vel, Time.fixedDeltaTime * 3));
            }
        }
    }

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
    private void TouchScreenMovement()
    {
        if (Input.touchCount == 0) {
            moveHorizontal = 0.0f;
            moveVertical = 0.0f;
            return;
        }

        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) {
                firstTouchPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved) {
                Vector2 touchDeltaPosition = touch.position - firstTouchPos;
                moveHorizontal = Mathf.Abs(touchDeltaPosition.x) > 50 ? touchDeltaPosition.x : 0.0f;
                moveVertical = Mathf.Abs(touchDeltaPosition.y) > 50 ? touchDeltaPosition.y : 0.0f;
            }
            else if (touch.phase == TouchPhase.Ended) {
                moveHorizontal = 0.0f;
                moveVertical = 0.0f;
            }
        }
    }
#endif

    private void MoveInput()
    {
        // Push back the player from the rising rows
        if (!raiseCubes.RisingDone && transform.position.z > floorBlocks.LevelEnd - (raiseCubes.RisingRow + 1)) {
            moveHorizontal = 0;
            moveVertical = -1.0f;
        }
        else {
            if (!playerMoveInputDisabled) {

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
                TouchScreenMovement();
#else
                moveHorizontal = Input.GetAxisRaw("Horizontal");
                moveVertical = Input.GetAxisRaw("Vertical");
#endif
            }
            else {
                moveHorizontal = 0;
                moveVertical = 0;
            }
        }
    }

    private void Movement()
    {
        // Deal with movement input
        if (moveHorizontal != 0.0f) {
            xMove.Input(moveHorizontal, !RightBlocked(), !LeftBlocked());
        }
        if (moveVertical != 0.0f) {
            zMove.Input(moveVertical, !ForwardBlocked(), !BackBlocked());
        }

        transform.Translate(xMove.Move(), 0, zMove.Move());

        // Snap to the centre of the grid
        // Needed due the collisions moving the player
        if (xMove.DistanceToMove == 0f) {
            RoundX(gameObject);
        }
        if (zMove.DistanceToMove == 0f) {
            RoundZ(gameObject);
        }
    }

    private void RoundX(GameObject gameObject)
    {
        transform.position = new Vector3(
            Mathf.Round(transform.position.x + 0.5f) - 0.5f,
            transform.position.y,
            transform.position.z);
    }

    private void RoundZ(GameObject gameObject)
    {
        transform.position = new Vector3(
            transform.position.x,
        transform.position.y,
        Mathf.Round(transform.position.z + 0.5f) - 0.5f);
    }

    private void SetRotation()
    {
        characterContainer.transform.Rotate(Vector3.up, CalcCharacterRotation() * Time.fixedDeltaTime * rotationSpeed);
    }

    private float CalcCharacterRotation()
    {
        return CalcTurnAngle(characterContainer.transform.rotation.eulerAngles.y,
            Mathf.Atan2(xMove.DistanceToMove * xMove.Direction(), zMove.DistanceToMove * zMove.Direction()) * Mathf.Rad2Deg);
    }

    // Calculates the shortest turn angle between two bearings.
    // A negative angle symbolising an anti-clockwise turn.
    private float CalcTurnAngle(float angleStart, float angleTarget)
    {
        // Taken from http://gmc.yoyogames.com/index.php?showtopic=532420
        return (((angleTarget - angleStart) + 540) % 360) - 180;
    }

    public bool ForwardBlocked()
    {
        if (transform.position.z > floorBlocks.LevelEnd - 1) {
            return true;
        }
        return VertBlocked(1);
    }

    private bool BackBlocked()
    {
        if (transform.position.z < floorBlocks.LevelEnd - floorBlocks.LevelLength + 1) {
            return true;
        }
        return VertBlocked(-1);
    }

    private bool RightBlocked()
    {
        if (transform.position.x > floorBlocks.LevelWidth - 1) {
            return true;
        }
        return HoriBlocked(1);
    }

    private bool LeftBlocked()
    {
        if (transform.position.x < 1) {
            return true;
        }
        return HoriBlocked(-1);
    }

    private bool VertBlocked(int direction)
    {
        Vector3 start = PositionAfterMovement();
        Vector3 size = new Vector3(0.45f, 0.45f, 0);
        Vector3 end = new Vector3(0, 0, direction);
        return Physics.BoxCast(start, size, end, Quaternion.identity, 1.45f);
    }

    private bool HoriBlocked(int direction)
    {
        Vector3 start = PositionAfterMovement();
        Vector3 size = new Vector3(0, 0.45f, 0.45f);
        Vector3 end = new Vector3(direction, 0, 0);
        return Physics.BoxCast(start, size, end, Quaternion.identity, 1.45f);
    }

    private Vector3 PositionAfterMovement()
    {
        return new Vector3(
            transform.position.x + xMove.Direction() * xMove.DistanceToMove,
            0.5f,
            transform.position.z + zMove.Direction() * zMove.DistanceToMove);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag != "Floor" && !playerController.PlayerCrushed) {
            if (!(xMove.DistanceToMove != 0 && zMove.DistanceToMove != 0)) {
                playerController.CrushPlayer();
            }
            else {
                Vector3 moveVector = Vector3.Normalize(
                    new Vector3(
                        transform.position.x - collision.contacts[0].point.x,
                        0,
                        transform.position.z - collision.contacts[0].point.z));

                moveVector = moveVector / 12;
                transform.Translate(moveVector);
                xMove.DistanceToMove = xMove.DistanceToMove - xMove.Direction() * moveVector.x;
                zMove.DistanceToMove = zMove.DistanceToMove - zMove.Direction() * moveVector.z;
            }
        }
    }
}

public class MoveAxis
{
    private float distanceToMove = 0.0f;
    public float DistanceToMove
    {
        get {
            if (distanceToMove < 0.0f) {
                return 0.0f;
            }
            else {
                return distanceToMove;
            }
        }
        set {
            distanceToMove = value;
        }
    }

    private float velocity = 5.0f;
    private float moveDistance = 1.0f;

    public float Move()
    {
        float movement = 0.0f;

        if (distanceToMove > 0.0f) {
            float move = Mathf.Min(Mathf.Abs(velocity) * Time.fixedDeltaTime, distanceToMove);

            if (Mathf.Sign(velocity) < 0) {
                movement = -move;
            }
            else {
                movement = move;
            }
            distanceToMove -= move;
        }

        return movement;
    }

    public void Input(float axisInput, bool posFree, bool negFree)
    {
        bool posDirection = Mathf.Sign(axisInput) >= 0;

        if (axisInput != 0.0f) {
            if (!posDirection && Mathf.Sign(velocity) >= 0 && negFree) {
                velocity = -velocity;
                distanceToMove = moveDistance - distanceToMove;
            }
            else if (posDirection && Mathf.Sign(velocity) < 0 && posFree) {
                velocity = -velocity;
                distanceToMove = moveDistance - distanceToMove;
            }
        }

        if (distanceToMove <= 0.0f && axisInput != 0.0f) {
            if (!posDirection && negFree) {
                distanceToMove = 1.0f;
                velocity = -Mathf.Abs(velocity);
            }
            else if (posDirection && posFree) {
                distanceToMove = 1.0f;
                velocity = Mathf.Abs(velocity);
            }
        }
    }

    public int Direction()
    {
        return Mathf.Sign(velocity) < 0 ? -1 : 1;
    }
}

using UnityEngine;
using UnityEngine.XR;

// Attach to your Player GameObject (the capsule)
// Requires a Rigidbody on the same object
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraRig;
    public Transform centerEyeAnchor;
    public LaneManager laneManager;

    [Header("Forward Speed")]
    public float startSpeed = 5f;
    public float maxSpeed = 20f;
    public float speedRampRate = 0.1f;

    [Header("Lane Movement")]
    public float laneChangeSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 5f;
    public float jumpHeightThreshold = 0.15f;

    [Header("Duck / Roll")]
    public float duckThreshold = -0.15f;
    public float rollDuration = 0.5f;

    // ── private state ──────────────────────────────────────
    private Rigidbody rb;
    private int currentLane = 1;   // 0=left 1=centre 2=right
    private float targetX = 0f;
    private float currentSpeed;
    private bool isGrounded = true;
    private bool isRolling = false;
    private float rollTimer = 0f;

    // head tracking
    private float standingHeadY = 0f;
    private float prevHeadY = 0f;
    private bool headCalibrated = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        currentSpeed = startSpeed;
        Invoke(nameof(CalibrateHead), 1f);
    }

    void CalibrateHead()
    {
        standingHeadY = centerEyeAnchor.localPosition.y;
        prevHeadY = standingHeadY;
        headCalibrated = true;
        Debug.Log("Head height calibrated: " + standingHeadY);
    }

    void Update()
    {
        if (!GameManager.Instance.IsPlaying) return;
        HandleLaneInput();
        HandleJump();
        HandleDuck();
        RampSpeed();
        SmoothLane();
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.IsPlaying) return;
        rb.MovePosition(rb.position +
            transform.forward * currentSpeed * Time.fixedDeltaTime);
    }

    // ── lanes ──────────────────────────────────────────────
    void HandleLaneInput()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.LeftHand)
            .TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 stick);

        if (stick.x < -0.5f && currentLane > 0)
        {
            currentLane--;
            targetX = laneManager.GetLaneX(currentLane);
        }
        else if (stick.x > 0.5f && currentLane < 2)
        {
            currentLane++;
            targetX = laneManager.GetLaneX(currentLane);
        }
    }

    void SmoothLane()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * laneChangeSpeed);
        transform.position = pos;
    }

    // ── jump ───────────────────────────────────────────────
    void HandleJump()
    {
        if (!isGrounded) return;

        bool btnJump = false;
        bool headJump = false;

        InputDevices.GetDeviceAtXRNode(XRNode.RightHand)
            .TryGetFeatureValue(CommonUsages.primaryButton, out btnJump);

        if (headCalibrated)
        {
            float delta = centerEyeAnchor.localPosition.y - prevHeadY;
            headJump = delta > jumpHeightThreshold;
        }

        if (btnJump || headJump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        prevHeadY = centerEyeAnchor.localPosition.y;
    }

    // ── duck / roll ────────────────────────────────────────
    void HandleDuck()
    {
        if (!headCalibrated || isRolling) return;

        float drop = centerEyeAnchor.localPosition.y - standingHeadY;
        if (drop < duckThreshold) StartRoll();

        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f) isRolling = false;
        }
    }

    void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;
        // TODO: trigger your roll animation here
        // GetComponent<Animator>().SetTrigger("Roll");
        Debug.Log("ROLL triggered");
    }

    // ── speed ramp ─────────────────────────────────────────
    void RampSpeed()
    {
        currentSpeed = Mathf.Min(
            currentSpeed + speedRampRate * Time.deltaTime, maxSpeed);
    }

    // ── collisions ─────────────────────────────────────────
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;

        if (col.gameObject.CompareTag("Obstacle"))
            GameManager.Instance.TriggerGameOver();
    }
}
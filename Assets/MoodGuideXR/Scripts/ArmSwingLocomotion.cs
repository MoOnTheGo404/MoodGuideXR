using UnityEngine;

// ArmSwingLocomotion
// "Jogging" travel: swing the LEFT controller to move forward. Speed comes from how fast
// you swing; direction comes from where your head is facing. Non-standard travel method.
//
// Put this on the OVRCameraRig (which must have a CharacterController). Assign Head =
// CenterEyeAnchor and Left Hand = LeftHandAnchor. Set movementEnabled = false to freeze
// movement (the door menu does this while it's open).
[RequireComponent(typeof(CharacterController))]
public class ArmSwingLocomotion : MonoBehaviour
{
    [Header("Refs")]
    public Transform head;          // CenterEyeAnchor - steering direction
    public Transform leftHand;      // LeftHandAnchor - the swinging arm

    [Header("Tuning")]
    [Tooltip("How much hand-swing speed converts into movement.")]
    public float speedScale = 1.5f;
    [Tooltip("Max move speed (m/s).")]
    public float maxSpeed = 2.5f;
    [Tooltip("Ignore hand jitter below this swing speed (m/s).")]
    public float swingThreshold = 0.4f;
    [Tooltip("Higher = snappier start/stop.")]
    public float smoothing = 8f;
    public float gravity = -9.81f;

    [Tooltip("Set false to freeze movement (e.g. while a menu is open).")]
    public bool movementEnabled = true;

    private CharacterController cc;
    private Vector3 lastLocalHandPos;
    private float currentSpeed;
    private float verticalVel;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (leftHand != null)
            lastLocalHandPos = transform.InverseTransformPoint(leftHand.position);
    }

    void Update()
    {
        // Frozen: keep the hand reference current so re-enabling doesn't read a giant
        // phantom swing, zero the speed, and do nothing else.
        if (!movementEnabled)
        {
            if (leftHand != null)
                lastLocalHandPos = transform.InverseTransformPoint(leftHand.position);
            currentSpeed = 0f;
            return;
        }

        // 1) Measure the left hand's swing speed RELATIVE to the rig (so our own movement
        //    doesn't feed back into the reading).
        float swingSpeed = 0f;
        if (leftHand != null)
        {
            Vector3 local = transform.InverseTransformPoint(leftHand.position);
            swingSpeed = (local - lastLocalHandPos).magnitude / Mathf.Max(Time.deltaTime, 1e-5f);
            lastLocalHandPos = local;
        }

        // 2) Convert swing speed -> target move speed (ignore jitter, cap it).
        float target = swingSpeed > swingThreshold
            ? Mathf.Min(swingSpeed * speedScale, maxSpeed)
            : 0f;
        currentSpeed = Mathf.Lerp(currentSpeed, target, Time.deltaTime * smoothing);

        // 3) Move along the head's horizontal forward.
        Vector3 fwd = Vector3.forward;
        if (head != null) { fwd = head.forward; fwd.y = 0f; fwd.Normalize(); }

        Vector3 move = fwd * currentSpeed;

        // Gravity so the capsule stays on the floor.
        if (cc.isGrounded && verticalVel < 0f) verticalVel = -2f;
        verticalVel += gravity * Time.deltaTime;
        move.y = verticalVel;

        cc.Move(move * Time.deltaTime);
    }
}
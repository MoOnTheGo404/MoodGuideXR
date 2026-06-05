using UnityEngine;

// Door
// Put this on each interior door's "Door" child (the one you tagged "Door").
// Exposes openAmount (0 = closed, 1 = fully open) and rotates the hinge to match.
// The toggle and slider in the HUD menu both write to openAmount.
//
// IMPORTANT: the apartment-kit doors ship with an Animator + opencloseDoor script that
// would overwrite the hinge every frame. This script disables the door's own Animator on
// Awake so continuous (slider) control works. If your door still snaps back, also disable
// any "opencloseDoor" script component on the same object in the Inspector.
[DisallowMultipleComponent]
public class Door : MonoBehaviour
{
    [Tooltip("Transform that rotates. Leave empty to rotate this object itself.")]
    public Transform hinge;

    [Tooltip("Angle in degrees at fully open. Flip the sign (e.g. -90) if it opens the wrong way.")]
    public float openAngle = 90f;

    [Tooltip("How quickly it eases toward the target angle.")]
    public float speed = 6f;

    [Range(0f, 1f)] public float openAmount = 0f;   // 0 = closed, 1 = open

    private Quaternion closedRot;
    private Quaternion openRot;

    void Awake()
    {
        if (hinge == null) hinge = transform;
        closedRot = hinge.localRotation;
        openRot = closedRot * Quaternion.Euler(0f, openAngle, 0f);

        // Stop the kit's animator from fighting our rotation.
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;
    }

    void Update()
    {
        Quaternion target = Quaternion.Slerp(closedRot, openRot, openAmount);
        hinge.localRotation = Quaternion.Slerp(hinge.localRotation, target, Time.deltaTime * speed);
    }

    public void SetOpenAmount(float t01) => openAmount = Mathf.Clamp01(t01);
    public void SetOpen(bool open) => openAmount = open ? 1f : 0f;
}

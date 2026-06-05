using UnityEngine;
using UnityEngine.UI;

public class DoorMenuHud : MonoBehaviour
{
    [Header("Scene refs")]
    public DoorRaySelector selector;        // the one on RightHandAnchor
    public Transform rayOrigin;             // RightHandAnchor
    public Transform head;                  // CenterEyeAnchor (for HUD placement)
    public ArmSwingLocomotion locomotion;   // the one on OVRCameraRig (freeze while menu open)
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    [Header("Menu")]
    public GameObject menuRoot;             // the Canvas (or a panel) to show/hide
    public float distance = 1.2f;           // how far in front of the face it appears
    public bool followView = false;         // false = placed in front on open, then stays put

    [Header("Widgets")]
    public Toggle openToggle;               // on = fully open
    public Slider openSlider;               // 0..100
    public Collider toggleCollider;         // BoxCollider on the Toggle
    public Collider sliderCollider;         // BoxCollider on the Slider
    public Collider closeCollider;          // BoxCollider on the X button

    [Header("Ray")]
    public float rayLength = 10f;

    private Door currentDoor;
    private bool menuOpen;
    private bool wasTriggerPressed;

    void Start()
    {
        if (menuRoot != null) menuRoot.SetActive(false);
        if (openSlider != null) { openSlider.minValue = 0f; openSlider.maxValue = 100f; }
    }

    void Update()
    {
        if (!menuOpen && DoorRaySelector.CurrentSelected != null)
            OpenMenu(DoorRaySelector.CurrentSelected);

        if (!menuOpen) return;

        if (DoorRaySelector.CurrentSelected == null) { CloseMenu(); return; }

        if (followView) PositionInFront();

        HandleRay();
    }

    void OpenMenu(GameObject doorGO)
    {
        currentDoor = doorGO.GetComponentInChildren<Door>();
        if (currentDoor == null) currentDoor = doorGO.GetComponentInParent<Door>();
        if (currentDoor == null)
            Debug.LogWarning("DoorMenuHud: selected door has no Door component - slider/toggle won't move it. Add Door.cs to: " + doorGO.name);

        menuOpen = true;
        if (selector != null) selector.selectionEnabled = false;
        if (locomotion != null) locomotion.movementEnabled = false;
        if (menuRoot != null) menuRoot.SetActive(true);
        PositionInFront();

        float t = currentDoor != null ? currentDoor.openAmount : 0f;
        if (openSlider != null) openSlider.SetValueWithoutNotify(t * 100f);
        if (openToggle != null) openToggle.SetIsOnWithoutNotify(t >= 0.999f);

        wasTriggerPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controller);
    }

    void CloseMenu()
    {
        menuOpen = false;
        currentDoor = null;
        if (menuRoot != null) menuRoot.SetActive(false);
        if (selector != null)
        {
            selector.Deselect();
            selector.selectionEnabled = true;
        }
        if (locomotion != null) locomotion.movementEnabled = true;
    }

    void PositionInFront()
    {
        if (menuRoot == null || head == null) return;
        Vector3 pos = head.position + head.forward * distance;
        menuRoot.transform.position = pos;
        menuRoot.transform.rotation = Quaternion.LookRotation(pos - head.position, Vector3.up);
    }

    void HandleRay()
    {
        bool triggerPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controller);
        bool freshPress = triggerPressed && !wasTriggerPressed;

        Transform o = rayOrigin != null ? rayOrigin : transform;
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(o.position, o.forward, out hit, rayLength);

        if (hitSomething)
        {
            Collider c = hit.collider;

            if (c == closeCollider && freshPress)
            {
                CloseMenu();
                wasTriggerPressed = triggerPressed;
                return;
            }

            if (c == toggleCollider && freshPress && openToggle != null)
            {
                bool n = !openToggle.isOn;
                openToggle.SetIsOnWithoutNotify(n);
                if (currentDoor != null) currentDoor.SetOpen(n);
                if (openSlider != null) openSlider.SetValueWithoutNotify(n ? 100f : 0f);
            }

            if (c == sliderCollider && triggerPressed && openSlider != null)
            {
                float t = SliderValueFromHit(hit);
                openSlider.SetValueWithoutNotify(t * 100f);
                if (currentDoor != null) currentDoor.SetOpenAmount(t);
                if (openToggle != null) openToggle.SetIsOnWithoutNotify(t >= 0.999f);
            }
        }

        wasTriggerPressed = triggerPressed;
    }

    float SliderValueFromHit(RaycastHit hit)
    {
        BoxCollider bc = sliderCollider as BoxCollider;
        Vector3 local = sliderCollider.transform.InverseTransformPoint(hit.point);
        if (bc != null && bc.size.x != 0f)
        {
            float left = bc.center.x - bc.size.x * 0.5f;
            return Mathf.Clamp01((local.x - left) / bc.size.x);
        }
        return Mathf.Clamp01(local.x + 0.5f);
    }
}
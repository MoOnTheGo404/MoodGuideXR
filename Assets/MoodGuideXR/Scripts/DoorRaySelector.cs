using UnityEngine;

public class DoorRaySelector : MonoBehaviour
{
    [Header("Ray")]
    public Transform rayOrigin;
    public float rayLength = 10f;

    [Header("Selection")]
    public string doorTag = "Door";
    public Color hoverColor = Color.blue;
    public Color selectedColor = Color.red;
    public float emissionIntensity = 2f;

    [Header("Input")]
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    [Tooltip("Set false (by the menu) to stop selecting/deselecting doors while a menu is open.")]
    public bool selectionEnabled = true;

    public static GameObject CurrentSelected { get; private set; }

    private LineRenderer line;
    private GameObject hovered;
    private bool wasTriggerPressed;

    private MaterialPropertyBlock mpb;
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        if (rayOrigin == null) rayOrigin = transform;
        mpb = new MaterialPropertyBlock();

        line = GetComponent<LineRenderer>();
        if (line == null) line = gameObject.AddComponent<LineRenderer>();
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.enabled = false;
    }

    void Update()
    {
        bool triggerPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controller);

        Vector3 origin = rayOrigin.position;
        Vector3 dir = rayOrigin.forward;

        RaycastHit hit;
        bool hitSomething = Physics.Raycast(origin, dir, out hit, rayLength);
        Vector3 endPoint = hitSomething ? hit.point : origin + dir * rayLength;

        // Show the ray while holding the trigger, OR whenever the menu owns the ray (selection disabled).
        bool showRay = triggerPressed || !selectionEnabled;
        line.enabled = showRay;
        if (showRay)
        {
            line.SetPosition(0, origin);
            line.SetPosition(1, endPoint);
        }

        if (!selectionEnabled)
        {
            ClearHover();
            wasTriggerPressed = triggerPressed;
            return;
        }

        GameObject door = null;
        if (hitSomething)
        {
            Transform t = hit.collider.transform;
            while (t != null)
            {
                if (t.CompareTag(doorTag)) { door = t.gameObject; break; }
                t = t.parent;
            }
        }

        if (triggerPressed && door != null && door != CurrentSelected)
        {
            if (hovered != door)
            {
                ClearHover();
                hovered = door;
                SetEmission(hovered, hoverColor);
            }
        }
        else
        {
            ClearHover();
        }

        bool justReleased = wasTriggerPressed && !triggerPressed;
        if (justReleased && door != null)
            SelectDoor(door);
        else if (!wasTriggerPressed && triggerPressed && CurrentSelected != null)
            DeselectCurrent();

        wasTriggerPressed = triggerPressed;
    }

    void SelectDoor(GameObject door)
    {
        if (CurrentSelected == door) return;
        if (CurrentSelected != null) DeselectCurrent();
        if (hovered == door) hovered = null;
        CurrentSelected = door;
        SetEmission(CurrentSelected, selectedColor);
        Debug.Log("Selected door: " + door.name);
    }

    private void DeselectCurrent()
    {
        if (CurrentSelected == null) return;
        ClearEmission(CurrentSelected);
        CurrentSelected = null;
    }

    public void Deselect() => DeselectCurrent();

    void ClearHover()
    {
        if (hovered != null && hovered != CurrentSelected)
            ClearEmission(hovered);
        hovered = null;
    }

    void SetEmission(GameObject obj, Color color)
    {
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionColorID, color * emissionIntensity);
            r.SetPropertyBlock(mpb);
            foreach (var m in r.materials) m.EnableKeyword("_EMISSION");
        }
    }

    void ClearEmission(GameObject obj)
    {
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionColorID, Color.black);
            r.SetPropertyBlock(mpb);
        }
    }
}
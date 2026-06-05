using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RectColliderFitter : MonoBehaviour
{
    void OnEnable() { Fit(); }
    void Start() { Fit(); }

    public void Fit()
    {
        RectTransform rt = transform as RectTransform;
        if (rt == null) return;
        BoxCollider bc = GetComponent<BoxCollider>();
        Rect r = rt.rect;
        bc.center = new Vector3(r.center.x, r.center.y, 0f);
        bc.size = new Vector3(r.width, r.height, 1f);
    }
}
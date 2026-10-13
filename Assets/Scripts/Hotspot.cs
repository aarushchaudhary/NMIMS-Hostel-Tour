using UnityEngine;

public class Hotspot : MonoBehaviour {
    public PanoNode target;

    void OnMouseDown() {
        var viewer = FindFirstObjectByType<PanoViewer>();
        if (viewer != null) viewer.Travel(target);
    }
}

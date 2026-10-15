using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Hotspot : MonoBehaviour {
    public PanoNode target;

    void Awake() {
        var i = GetComponent<XRSimpleInteractable>();
        if (i == null) i = gameObject.AddComponent<XRSimpleInteractable>();
        i.selectEntered.AddListener(_ => Go());
    }

    void OnMouseDown() { Go(); }

    void Go() {
        var v = FindFirstObjectByType<PanoViewer>();
        if (v != null) v.Travel(target);
    }
}

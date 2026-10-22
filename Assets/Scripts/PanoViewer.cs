using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PanoViewer : MonoBehaviour {
    public PanoNode startNode;
    public Renderer screenRenderer;
    public GameObject arrowPrefab;      // leave empty in TourVR
    public Image fadePanel;             // leave empty in VR
    public TMP_Text areaLabel;
    public float arrowRadius = 4.5f;
    public float arrowHeight = -0.5f;
    public float fadeTime = 0.25f;

    PanoNode current;
    bool busy;
    public PanoNode Current => current;
    readonly List<GameObject> arrows = new List<GameObject>();

    void Start() {
        if (startNode != null) StartCoroutine(Go(startNode));
        else Debug.LogError("PanoViewer: Start Node is not assigned. Run Tour > 2 - Build Nodes From Frames.");
    }

    IEnumerator Go(PanoNode node) {
        busy = true;
        yield return Fade(0f, 1f, fadeTime);

        current = node;
        screenRenderer.material.mainTexture = node.frame;
        if (areaLabel != null) areaLabel.text = node.areaName;
        BuildArrows();

        yield return Fade(1f, 0f, fadeTime);
        busy = false;
    }

    void BuildArrows() {
        foreach (var a in arrows) Destroy(a);
        arrows.Clear();
        if (arrowPrefab == null) return;

        for (int i = 0; i < current.neighbors.Length; i++) {
            if (Mathf.Approximately(current.neighborYaw[i], 180f)) continue;

            float yaw = current.neighborYaw[i] * Mathf.Deg2Rad;
            var pos = new Vector3(Mathf.Sin(yaw) * arrowRadius, arrowHeight, Mathf.Cos(yaw) * arrowRadius);
            var arrow = Instantiate(arrowPrefab, pos, Quaternion.LookRotation(pos));
            arrow.GetComponent<Hotspot>().target = current.neighbors[i];
            arrows.Add(arrow);
        }
    }

    IEnumerator Fade(float from, float to, float time) {
        if (fadePanel == null) yield break;
        for (float t = 0; t < time; t += Time.deltaTime) {
            var c = fadePanel.color;
            c.a = Mathf.Lerp(from, to, t / time);
            fadePanel.color = c;
            yield return null;
        }
        var f = fadePanel.color;
        f.a = to;
        fadePanel.color = f;
    }

    public void Travel(PanoNode n) {
        if (busy || n == null) return;
        StartCoroutine(Go(n));
    }

    public void Back() {
        if (current == null) return;
        for (int i = 0; i < current.neighbors.Length; i++) {
            if (Mathf.Approximately(current.neighborYaw[i], 180f)) {
                Travel(current.neighbors[i]);
                return;
            }
        }
    }

    public void Forward() {
        if (current == null) return;
        for (int i = 0; i < current.neighbors.Length; i++) {
            if (!Mathf.Approximately(current.neighborYaw[i], 180f)) {
                Travel(current.neighbors[i]);
                return;
            }
        }
    }
}

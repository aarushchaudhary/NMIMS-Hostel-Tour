using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;

public class VRControls : MonoBehaviour {
    public PanoViewer viewer;
    public Transform xrOrigin;
    public TMP_Text debugText;          // optional

    public float turnSpeed = 60f;
    public float maxYaw = 60f;
    public float maxPitch = 20f;
    public float deadzone = 0.2f;
    public float rayLength = 6f;
    public bool showInstructions = true;

    float yaw, pitch;
    bool leftTrigHeld, aHeld, bHeld, xHeld, yHeld, gripHeld;
    AreaMenu menu;
    Canvas canvas;
    Camera cam;
    LineRenderer line;
    GameObject instructionsPanel;
    string hovered = "";

    const string HelpText =
    "CONTROLS\n" +
    "A: next area     B: previous area\n" +
    "Left Y: forward     Left X: back\n" +
    "Left trigger: click menu buttons (point the line at them)\n" +
    "Joysticks: turn / tilt the view\n" +
    "Right grip: hide / show this help";

    void Start() {
        if (viewer == null) viewer = FindFirstObjectByType<PanoViewer>();
        menu = FindFirstObjectByType<AreaMenu>();
        canvas = FindFirstObjectByType<Canvas>();
        cam = Camera.main;
        if (xrOrigin == null && cam != null) {
            var t = cam.transform;
            if (t.parent != null && t.parent.parent != null) xrOrigin = t.parent.parent;
        }

        // Pointer line
        var go = new GameObject("PointerLine");
        line = go.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = 0.004f;
        line.endWidth = 0.004f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = Color.cyan;
        line.enabled = false;

        CreateInstructions();
    }

    void CreateInstructions() {
        if (canvas == null) return;

        instructionsPanel = new GameObject("InstructionsPanel", typeof(RectTransform), typeof(Image));
        instructionsPanel.transform.SetParent(canvas.transform, false);
        var prt = (RectTransform)instructionsPanel.transform;
        prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0f);
        prt.pivot = new Vector2(0.5f, 0f);
        prt.anchoredPosition = new Vector2(0, 20);
        prt.sizeDelta = new Vector2(1300, 260);
        var img = instructionsPanel.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.65f);
        img.raycastTarget = false;

        var tgo = TMP_DefaultControls.CreateText(new TMP_DefaultControls.Resources());
        tgo.name = "InstructionsText";
        tgo.transform.SetParent(instructionsPanel.transform, false);
        var trt = (RectTransform)tgo.transform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(20, 10);
        trt.offsetMax = new Vector2(-20, -10);
        var txt = tgo.GetComponent<TMP_Text>();
        txt.text = HelpText;
        txt.fontSize = 36;
        txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.Center;
        txt.raycastTarget = false;

        instructionsPanel.SetActive(showInstructions);
    }

    void Update() {
        var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        // Right A = next area, right B = previous area
        bool a = Pressed(right, CommonUsages.primaryButton);
        bool b = Pressed(right, CommonUsages.secondaryButton);
        if (a && !aHeld) JumpArea(+1);
        if (b && !bHeld) JumpArea(-1);
        aHeld = a;
        bHeld = b;

        // Left Y = forward one frame, left X = back one frame
        bool y = Pressed(left, CommonUsages.secondaryButton);
        bool x = Pressed(left, CommonUsages.primaryButton);
        if (y && !yHeld && viewer != null) viewer.Forward();
        if (x && !xHeld && viewer != null) viewer.Back();
        yHeld = y;
        xHeld = x;

        // Right grip = show / hide instructions
        bool grip = Pressed(right, CommonUsages.gripButton);
        if (grip && !gripHeld && instructionsPanel != null)
            instructionsPanel.SetActive(!instructionsPanel.activeSelf);
        gripHeld = grip;

        // Left trigger = click the button the left controller points at
        bool l = TriggerDown(left, out float lv);
        Button target = UpdatePointer(left);
        if (l && !leftTrigHeld && target != null) target.onClick.Invoke();
        leftTrigHeld = l;

        // Joysticks turn and tilt
        Vector2 s = Stick(right) + Stick(left);
        if (s.magnitude < deadzone) s = Vector2.zero;
        yaw += s.x * turnSpeed * Time.deltaTime;
        pitch -= s.y * turnSpeed * Time.deltaTime;
        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        if (xrOrigin != null) xrOrigin.localRotation = Quaternion.Euler(pitch, yaw, 0f);

        if (debugText != null) {
            debugText.text = "R valid:" + right.isValid
            + "\nL valid:" + left.isValid + " trig:" + lv.ToString("0.00")
            + "\nstick:" + s
            + "\nviewer:" + (viewer != null) + " origin:" + (xrOrigin != null)
            + " menu:" + (menu != null) + " canvas:" + (canvas != null)
            + "\npointing at: " + hovered;
        }
    }

    // Draws the ray from the left controller and returns the button under it (or null)
    Button UpdatePointer(InputDevice left) {
        hovered = "";
        if (cam == null || canvas == null || !left.isValid) { line.enabled = false; return null; }
        if (!left.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 lp) ||
            !left.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion lr)) {
            line.enabled = false; return null;
            }

            Transform space = cam.transform.parent != null ? cam.transform.parent : cam.transform;
        Vector3 origin = space.TransformPoint(lp);
        Vector3 dir = (space.rotation * lr) * Vector3.forward;

        line.enabled = true;
        line.SetPosition(0, origin);
        Vector3 end = origin + dir * rayLength;

        Vector3 n = canvas.transform.forward;
        if (Vector3.Dot(n, dir) > 0) n = -n;
        float denom = Vector3.Dot(n, dir);
        Button found = null;
        if (denom < -0.0001f) {
            float t = Vector3.Dot(canvas.transform.position - origin, n) / denom;
            if (t > 0f && t < rayLength) {
                Vector3 hit = origin + dir * t;
                end = hit;
                Vector2 sp = cam.WorldToScreenPoint(hit);
                foreach (var btn in canvas.GetComponentsInChildren<Button>()) {
                    if (!btn.isActiveAndEnabled || !btn.interactable) continue;
                    if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)btn.transform, sp, cam)) {
                        found = btn;
                        break;
                    }
                }
            }
        }
        line.SetPosition(1, end);
        line.startColor = line.endColor = found != null ? Color.green : Color.cyan;
        if (found != null) hovered = found.name;
        return found;
    }

    void JumpArea(int dir) {
        if (viewer == null || menu == null || viewer.Current == null) return;
        string cur = viewer.Current.areaName;
        for (int i = 0; i < menu.labels.Length; i++) {
            if (menu.labels[i] == cur) {
                int n = Mathf.Clamp(i + dir, 0, menu.targets.Length - 1);
                viewer.Travel(menu.targets[n]);
                return;
            }
        }
    }

    bool Pressed(InputDevice d, InputFeatureUsage<bool> usage) {
        return d.isValid && d.TryGetFeatureValue(usage, out bool v) && v;
    }

    bool TriggerDown(InputDevice d, out float value) {
        value = 0f;
        if (!d.isValid) return false;
        d.TryGetFeatureValue(CommonUsages.trigger, out value);
        if (d.TryGetFeatureValue(CommonUsages.triggerButton, out bool b) && b) return true;
        return value > 0.6f;
    }

    Vector2 Stick(InputDevice d) {
        if (d.isValid && d.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 v)) return v;
        return Vector2.zero;
    }
}

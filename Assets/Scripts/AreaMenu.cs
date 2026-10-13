using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AreaMenu : MonoBehaviour {
    public PanoViewer viewer;
    public PanoNode[] targets;
    public string[] labels;

    void Start() {
        var rt = (RectTransform)transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(15, -15);
        rt.sizeDelta = new Vector2(220, 0);

        var v = gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = 4;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        var fit = gameObject.AddComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        for (int i = 0; i < targets.Length; i++) {
            var node = targets[i];
            var go = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
            go.name = "Btn_" + labels[i];
            go.transform.SetParent(transform, false);

            var txt = go.GetComponentInChildren<TMP_Text>();
            txt.text = labels[i];
            txt.fontSize = 18;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 30;

            go.GetComponent<Button>().onClick.AddListener(() => viewer.Travel(node));
        }
    }
}

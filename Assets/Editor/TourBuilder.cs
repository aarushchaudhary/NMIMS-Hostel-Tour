using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public static class TourBuilder {
    const string FramesRoot = "Assets/Frames";

    // NOTE: static fields initialize top to bottom, so Places must stay ABOVE Route.

    // Places in walking order: (folder, label, corridorCut)
    //   corridorCut = last Corridor frame played BEFORE entering this place.
    //   corridorCut = 0 -> no corridor in between, goes straight after the previous place.
    // After the last place, the remaining corridor frames play until the end.
    static readonly (string folder, string label, int cut)[] Places = {
        ("TV_Room_1",       "TV Room 1",       11),
        ("Sick_Bay",        "Sick Bay",        15),
        ("TV_Room_2",       "TV Room 2",       21),
        ("Saloon",          "Saloon",          28),
        ("Laundry_Room",    "Laundry Room",     0),
        ("Badminton_Court", "Badminton Court",  0),
        ("Gym",             "Gym",             36),
        ("Game_Room",       "Game Room",       42),
        ("Mess_1",          "Mess 1",          67),
        ("Mess_2",          "Mess 2",          73),
    };

    static readonly (string area, int from, int to, string label)[] Route = BuildRoute();

    // Optional extra links (shortcuts or branches): (fromArea, fromFrame, toArea, toFrame, yaw)
    // Example: ("Corridor", 30, "Gym", 1, 50f) -> arrow at corridor frame 30, 50 degrees right
    static readonly (string from, int fromFrame, string to, int toFrame, float yaw)[] ExtraLinks = { };

    static (string area, int from, int to, string label)[] BuildRoute() {
        var r = new List<(string area, int from, int to, string label)>();
        r.Add(("Entrance", 1, 0, "Entrance"));
        r.Add(("Reception", 1, 0, "Reception"));

        int start = 1;
        int pieces = 0;
        foreach (var p in Places) {
            if (p.cut > 0) {
                pieces++;
                r.Add(("Corridor", start, p.cut, "Corridor " + pieces));
                start = p.cut + 1;
            }
            r.Add((p.folder, 1, 0, p.label));
        }

        pieces++;
        r.Add(("Corridor", start, 0, "Corridor " + pieces));   // 0 = until the last frame
        return r.ToArray();
    }

    // ---------------------------------------------------------------- 0
    [MenuItem("Tour/0 - Setup Scene")]
    static void SetupScene() {
        foreach (var n in new[] { "Screen", "Viewer", "Canvas", "EventSystem", "Directional Light" }) {
            var o = GameObject.Find(n);
            if (o) Object.DestroyImmediate(o);
        }

        EnsureFolder("Assets/Materials");
        EnsureFolder("Assets/Prefabs");

        // Camera
        var cam = Camera.main;
        if (cam == null) {
            var cg = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cg.tag = "MainCamera";
            cam = cg.GetComponent<Camera>();
        }
        cam.transform.position = Vector3.zero;
        cam.transform.rotation = Quaternion.identity;
        cam.fieldOfView = 65f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        if (cam.GetComponent<LimitedLook>() == null) cam.gameObject.AddComponent<LimitedLook>();

        // Materials
        var frameMat = GetOrCreateMat("Assets/Materials/FrameMat.mat", FrameShader());
        var arrowMat = GetOrCreateMat("Assets/Materials/ArrowMat.mat", ArrowShader());
        arrowMat.color = Color.yellow;
        EditorUtility.SetDirty(arrowMat);

        // Screen
        var screen = new GameObject("Screen");
        screen.AddComponent<CurvedScreen>();
        var mr = screen.GetComponent<MeshRenderer>();
        mr.sharedMaterial = frameMat;

        // Arrow prefab
        var arrowTmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        arrowTmp.name = "Arrow";
        arrowTmp.transform.localScale = Vector3.one * 0.4f;
        arrowTmp.GetComponent<MeshRenderer>().sharedMaterial = arrowMat;
        arrowTmp.AddComponent<Hotspot>();
        var arrowPrefab = PrefabUtility.SaveAsPrefabAsset(arrowTmp, "Assets/Prefabs/Arrow.prefab");
        Object.DestroyImmediate(arrowTmp);

        // Canvas + EventSystem
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // Fade panel
        var fade = new GameObject("FadePanel", typeof(RectTransform), typeof(Image));
        fade.transform.SetParent(canvasGO.transform, false);
        var frt = (RectTransform)fade.transform;
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
        frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
        var fimg = fade.GetComponent<Image>();
        fimg.color = new Color(0, 0, 0, 0);
        fimg.raycastTarget = false;

        // Area label
        var labelGO = TMP_DefaultControls.CreateText(new TMP_DefaultControls.Resources());
        labelGO.name = "AreaLabel";
        labelGO.transform.SetParent(canvasGO.transform, false);
        var lrt = (RectTransform)labelGO.transform;
        lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 1f);
        lrt.pivot = new Vector2(0.5f, 1f);
        lrt.anchoredPosition = new Vector2(0, -20);
        lrt.sizeDelta = new Vector2(700, 60);
        var ltxt = labelGO.GetComponent<TMP_Text>();
        ltxt.alignment = TextAlignmentOptions.Center;
        ltxt.fontSize = 40;
        ltxt.color = Color.white;
        ltxt.raycastTarget = false;
        ltxt.text = "";

        // Viewer
        var viewerGO = new GameObject("Viewer");
        var viewer = viewerGO.AddComponent<PanoViewer>();
        viewer.screenRenderer = mr;
        viewer.arrowPrefab = arrowPrefab;
        viewer.fadePanel = fimg;
        viewer.areaLabel = ltxt;
        EditorUtility.SetDirty(viewer);

        // Back button
        var backGO = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        backGO.name = "BackButton";
        backGO.transform.SetParent(canvasGO.transform, false);
        var brt = (RectTransform)backGO.transform;
        brt.anchorMin = brt.anchorMax = Vector2.zero;
        brt.pivot = Vector2.zero;
        brt.anchoredPosition = new Vector2(20, 20);
        brt.sizeDelta = new Vector2(180, 55);
        backGO.GetComponentInChildren<TMP_Text>().text = "Back";
        UnityEventTools.AddVoidPersistentListener(backGO.GetComponent<Button>().onClick, viewer.Back);

        // Area menu
        var menuGO = new GameObject("AreaMenu", typeof(RectTransform));
        menuGO.transform.SetParent(canvasGO.transform, false);
        var menu = menuGO.AddComponent<AreaMenu>();
        menu.viewer = viewer;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Scene set up. Now run Tour > 1 and Tour > 2.");
    }

    // ---------------------------------------------------------------- 1
    [MenuItem("Tour/1 - Fix Texture Import Settings")]
    static void FixImports() {
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { FramesRoot });
        AssetDatabase.StartAssetEditing();
        try {
            foreach (var g in guids) {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;
                imp.textureType = TextureImporterType.Default;
                imp.wrapMode = TextureWrapMode.Clamp;
                imp.mipmapEnabled = false;
                imp.maxTextureSize = 4096;
                imp.textureCompression = TextureImporterCompression.Compressed;
                imp.SaveAndReimport();
            }
        } finally {
            AssetDatabase.StopAssetEditing();
        }
        Debug.Log("Import settings fixed on " + guids.Length + " textures.");
    }

    // ---------------------------------------------------------------- 2
    [MenuItem("Tour/2 - Build Nodes From Frames")]
    static void Build() {
        // Check the cut numbers
        int lastCut = 0;
        foreach (var p in Places) {
            if (p.cut == 0) continue;
            if (p.cut <= lastCut)
                Debug.LogWarning("Cut numbers must increase. Problem at: " + p.label + " (" + p.cut + ")");
            lastCut = p.cut;
        }

        var old = GameObject.Find("Nodes");
        if (old) Object.DestroyImmediate(old);

        var root = new GameObject("Nodes");
        var folders = new Dictionary<string, List<PanoNode>>();
        var nb = new Dictionary<PanoNode, List<PanoNode>>();
        var yw = new Dictionary<PanoNode, List<float>>();

        // 1) Create one node per image
        foreach (var dirRaw in Directory.GetDirectories(FramesRoot).OrderBy(d => d)) {
            string dir = dirRaw.Replace('\\', '/');
            string area = Path.GetFileName(dir);
            var areaGO = new GameObject(area);
            areaGO.transform.SetParent(root.transform);

            var list = new List<PanoNode>();
            foreach (var fileRaw in Directory.GetFiles(dir, "*.jpg").OrderBy(f => f)) {
                string file = fileRaw.Replace('\\', '/');
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
                string num = Path.GetFileNameWithoutExtension(file).Replace("frame_", "");

                var go = new GameObject(area + "_" + num);
                go.transform.SetParent(areaGO.transform);
                var n = go.AddComponent<PanoNode>();
                n.areaName = area.Replace('_', ' ');
                n.frame = tex;
                nb[n] = new List<PanoNode>();
                yw[n] = new List<float>();
                list.Add(n);
            }
            folders[area] = list;
        }

        if (folders.ContainsKey("Corridor") && lastCut >= folders["Corridor"].Count)
            Debug.LogWarning("Last cut (" + lastCut + ") must be below the number of Corridor frames ("
            + folders["Corridor"].Count + ").");

        void Add(PanoNode a, PanoNode b, float yaw) { nb[a].Add(b); yw[a].Add(yaw); }
        void Link(PanoNode a, PanoNode b, float yaw) { Add(a, b, yaw); Add(b, a, 180f); }

        // 2) Follow the route
        var segFirst = new List<PanoNode>();
        var segLast = new List<PanoNode>();
        var labels = new List<string>();

        foreach (var seg in Route) {
            if (!folders.TryGetValue(seg.area, out var list) || list.Count == 0) {
                Debug.LogWarning("Route skipped, folder missing or empty: " + seg.area);
                continue;
            }
            int a = Mathf.Clamp(seg.from, 1, list.Count);
            int b = seg.to <= 0 ? list.Count : Mathf.Clamp(seg.to, a, list.Count);

            for (int i = a - 1; i < b - 1; i++) Link(list[i], list[i + 1], 0f);
            for (int i = a - 1; i < b; i++) list[i].areaName = seg.label;

            segFirst.Add(list[a - 1]);
            segLast.Add(list[b - 1]);
            labels.Add(seg.label);
        }

        // 3) Join consecutive segments
        for (int i = 0; i < segFirst.Count - 1; i++) Link(segLast[i], segFirst[i + 1], 0f);

        // 4) Optional extra links
        foreach (var k in ExtraLinks) {
            if (!folders.ContainsKey(k.from) || !folders.ContainsKey(k.to)
                || folders[k.from].Count == 0 || folders[k.to].Count == 0) {
                Debug.LogWarning("Extra link skipped: " + k.from + " -> " + k.to);
            continue;
                }
                var fl = folders[k.from];
                var tl = folders[k.to];
                Link(fl[Mathf.Clamp(k.fromFrame - 1, 0, fl.Count - 1)],
                     tl[Mathf.Clamp(k.toFrame - 1, 0, tl.Count - 1)], k.yaw);
        }

        // 5) Warn about folders that are not used in the route
        var used = new HashSet<string>(Route.Select(r => r.area));
        foreach (var f in folders.Keys)
            if (!used.Contains(f)) Debug.LogWarning("Folder not in Route (nodes created but unlinked): " + f);

            // 6) Write results
            foreach (var kv in nb) {
                kv.Key.neighbors = kv.Value.ToArray();
                kv.Key.neighborYaw = yw[kv.Key].ToArray();
                EditorUtility.SetDirty(kv.Key);
            }

            var viewer = Object.FindFirstObjectByType<PanoViewer>();
        if (viewer != null && segFirst.Count > 0) {
            viewer.startNode = segFirst[0];
            EditorUtility.SetDirty(viewer);
        }

        var menu = Object.FindFirstObjectByType<AreaMenu>();
        if (menu != null) {
            menu.targets = segFirst.ToArray();
            menu.labels = labels.ToArray();
            EditorUtility.SetDirty(menu);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Built " + nb.Count + " nodes, route has " + segFirst.Count + " segments.");
    }

    // ---------------------------------------------------------------- helpers
    static void EnsureFolder(string path) {
        if (AssetDatabase.IsValidFolder(path)) return;
        AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
    }

    static Material GetOrCreateMat(string path, Shader shader) {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) {
            m = new Material(shader);
            AssetDatabase.CreateAsset(m, path);
        } else {
            m.shader = shader;
        }
        return m;
    }

    static Shader FrameShader() {
        var s = Shader.Find("Universal Render Pipeline/Unlit");
        if (s == null) s = Shader.Find("Unlit/Texture");
        return s;
    }

    static Shader ArrowShader() {
        var s = Shader.Find("Universal Render Pipeline/Unlit");
        if (s == null) s = Shader.Find("Unlit/Color");
        return s;
    }
}

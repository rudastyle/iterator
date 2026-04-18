#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TimeLoop;

public static class SceneBuilder
{
    const string PrefabFolder = "Assets/Prefabs";
    const int    GroundLayer  = 6;
    const int    GhostLayer   = 7;
    const int    PlayerLayer  = 8;
    const int    ButtonLayer  = 9;

    // Button size in world units: 40×16 px ÷ 32 PPU
    const float BtnW = 1.25f;
    const float BtnH = 0.5f;

    [MenuItem("Tools/TimeLoop/Build Scene")]
    static void BuildAll()
    {
        SetupProjectSettings();
        EnsureFolder(PrefabFolder);

        var sp = GetOrCreateWhiteSprite();

        var platform      = CreatePlatformPrefab(sp);
        var pressurePlate = CreatePressurePlatePrefab(sp);
        var exitDoor      = CreateExitDoorPrefab(sp);
        var player        = CreatePlayerPrefab(sp);
        var ghost         = CreateGhostPrefab(sp);
        var btnItem       = CreateBtnItemPrefab();

        SetupScene(platform, pressurePlate, exitDoor, player, ghost, btnItem);

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[TimeLoop] Scene built! Press Play to test.");
    }

    // ── Project Settings ─────────────────────────────────────────────────────

    static void SetupProjectSettings()
    {
        var phys = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/Physics2DSettings.asset");
        if (phys.Length > 0)
        {
            var so = new SerializedObject(phys[0]);
            so.FindProperty("m_Gravity").vector2Value = new Vector2(0f, -25f);
            so.ApplyModifiedProperties();
        }

        var tm = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        AddTag(tm, "Player");
        AddTag(tm, "Ghost");
        AddLayer(tm, GroundLayer, "Ground");
        AddLayer(tm, GhostLayer,  "Ghost");
        AddLayer(tm, PlayerLayer, "Player");
        AddLayer(tm, ButtonLayer, "Button");
        tm.ApplyModifiedProperties();
    }

    static void AddTag(SerializedObject tm, string tag)
    {
        var tags = tm.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.arraySize++;
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
    }

    static void AddLayer(SerializedObject tm, int index, string name)
    {
        var layers = tm.FindProperty("layers");
        var elem   = layers.GetArrayElementAtIndex(index);
        if (string.IsNullOrEmpty(elem.stringValue)) elem.stringValue = name;
    }

    // ── White Sprite (32×32 px → 1×1 unit at PPU=32) ────────────────────────

    static Sprite GetOrCreateWhiteSprite()
    {
        const string path = "Assets/Textures/White.png";
        EnsureFolder("Assets/Textures");

        if (!File.Exists(Path.Combine(Application.dataPath, "../" + path)))
        {
            var tex = new Texture2D(32, 32);
            var px  = new Color[32 * 32];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(Path.Combine(Application.dataPath, "../" + path), tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
        }

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        if (imp != null)
        {
            imp.textureType         = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 32;
            imp.filterMode          = FilterMode.Point;
            imp.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ── Prefab Save Helper ───────────────────────────────────────────────────

    static GameObject SavePrefab(GameObject go, string name)
    {
        var path   = $"{PrefabFolder}/{name}.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ── Platform ─────────────────────────────────────────────────────────────

    static GameObject CreatePlatformPrefab(Sprite sp)
    {
        var go = new GameObject("Platform");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.color  = new Color(0.55f, 0.55f, 0.62f);

        // sprite is 1×1 unit; col.size must also be 1×1 so localScale == world size
        var col  = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        go.layer = GroundLayer;
        return SavePrefab(go, "Platform");
    }

    // ── PressurePlate ────────────────────────────────────────────────────────

    static GameObject CreatePressurePlatePrefab(Sprite sp)
    {
        var go = new GameObject("PressurePlate");

        // Body: size = 40×16 px = 1.25×0.5 units
        var body = go.AddComponent<SpriteRenderer>();
        body.sprite       = sp;
        body.color        = new Color(0.38f, 0.13f, 0.13f);
        body.drawMode     = SpriteDrawMode.Sliced;
        body.size         = new Vector2(BtnW, BtnH);
        body.sortingOrder = 1;

        // Indicator dot (top-center)
        var ind = new GameObject("Indicator");
        ind.transform.SetParent(go.transform, false);
        ind.transform.localPosition = new Vector3(0f, BtnH * 0.6f, 0f);
        ind.transform.localScale    = new Vector3(0.3f, 0.3f, 1f);
        var indSr = ind.AddComponent<SpriteRenderer>();
        indSr.sprite       = sp;
        indSr.sortingOrder = 2;

        go.layer = ButtonLayer;

        // Trigger collider
        var col       = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size      = new Vector2(BtnW, BtnH);

        var pp = go.AddComponent<PressurePlate>();
        var so = new SerializedObject(pp);
        so.FindProperty("_body").objectReferenceValue      = body;
        so.FindProperty("_indicator").objectReferenceValue = indSr;
        so.ApplyModifiedProperties();

        return SavePrefab(go, "PressurePlate");
    }

    // ── ExitDoor ─────────────────────────────────────────────────────────────

    static GameObject CreateExitDoorPrefab(Sprite sp)
    {
        var go = new GameObject("ExitDoor");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sp;
        sr.color        = new Color(0.48f, 0.23f, 0.06f);
        sr.sortingOrder = 1;

        // col.size = 1×1 so localScale (set by StageLoader) == world size
        var col  = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        var door = go.AddComponent<ExitDoor>();
        var so   = new SerializedObject(door);
        so.FindProperty("_col").objectReferenceValue    = col;
        so.FindProperty("_sprite").objectReferenceValue = sr;
        so.ApplyModifiedProperties();

        return SavePrefab(go, "ExitDoor");
    }

    // ── Characters ───────────────────────────────────────────────────────────

    static void SetupCharacterGO(GameObject go, Sprite sp, Color color)
    {
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sp;
        sr.color        = color;
        sr.drawMode     = SpriteDrawMode.Sliced;
        sr.size         = new Vector2(0.625f, 0.875f); // 20×28 px ÷ 32
        sr.sortingOrder = 3;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.constraints            = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col  = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.625f, 0.875f);

        var mover   = go.AddComponent<CharacterMover>();
        var moverSo = new SerializedObject(mover);
        moverSo.FindProperty("_groundMask").intValue = 1 << GroundLayer;
        moverSo.ApplyModifiedProperties();
    }

    static GameObject CreatePlayerPrefab(Sprite sp)
    {
        var go = new GameObject("Player");
        SetupCharacterGO(go, sp, Color.white);
        go.AddComponent<PlayerCharacter>();
        go.tag   = "Player";
        go.layer = PlayerLayer;
        return SavePrefab(go, "Player");
    }

    static GameObject CreateGhostPrefab(Sprite sp)
    {
        var go = new GameObject("Ghost");
        SetupCharacterGO(go, sp, new Color(1f, 1f, 1f, 0.45f));
        go.AddComponent<GhostCharacter>();
        go.tag   = "Ghost";
        go.layer = GhostLayer;
        return SavePrefab(go, "Ghost");
    }

    // ── BtnItem TMP Prefab ───────────────────────────────────────────────────

    static GameObject CreateBtnItemPrefab()
    {
        EnsureFolder("Assets/Prefabs/UI");
        var go  = new GameObject("BtnItem");
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize  = 18;
        tmp.color     = Color.white;
        tmp.text      = "○ Button";

        var rt       = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160f, 24f);

        var path   = "Assets/Prefabs/UI/BtnItem.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ── Scene Setup ──────────────────────────────────────────────────────────

    static void SetupScene(
        GameObject platform, GameObject pressurePlate, GameObject exitDoor,
        GameObject player,   GameObject ghost,         GameObject btnItem)
    {
        // GameManager object
        var gmGO     = FindOrCreate("GameManager");
        var loopMgr  = GetOrAdd<LoopManager>(gmGO);
        var stageLdr = GetOrAdd<StageLoader>(gmGO);
        var gm       = GetOrAdd<GameManager>(gmGO);

        var slSo = new SerializedObject(stageLdr);
        slSo.FindProperty("_platformPrefab").objectReferenceValue      = platform;
        slSo.FindProperty("_pressurePlatePrefab").objectReferenceValue = pressurePlate;
        slSo.FindProperty("_exitDoorPrefab").objectReferenceValue      = exitDoor;
        slSo.ApplyModifiedProperties();
        EditorUtility.SetDirty(stageLdr);

        var db   = AssetDatabase.LoadAssetAtPath<StageDatabase>("Assets/Resources/StageDatabase.asset");
        if (db == null)
            Debug.LogWarning("[TimeLoop] StageDatabase not found — run 'Create Stage Assets' first.");

        var gmSo = new SerializedObject(gm);
        if (db != null) gmSo.FindProperty("_database").objectReferenceValue = db;
        gmSo.FindProperty("_loopManager").objectReferenceValue  = loopMgr;
        gmSo.FindProperty("_stageLoader").objectReferenceValue  = stageLdr;
        gmSo.FindProperty("_playerPrefab").objectReferenceValue = player;
        gmSo.FindProperty("_ghostPrefab").objectReferenceValue  = ghost;
        gmSo.ApplyModifiedProperties();
        EditorUtility.SetDirty(gm);

        // Camera — zoom in for better visibility
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic     = true;
            cam.orthographicSize = 3.5f;
            cam.transform.position = new Vector3(10f, 5.625f, -10f);
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.08f, 0.08f, 0.12f);
            GetOrAdd<CameraFollow>(cam.gameObject);
            EditorUtility.SetDirty(cam);
        }

        // UI Canvas
        var canvasGO = FindOrCreate("UICanvas");
        var canvas   = GetOrAdd<Canvas>(canvasGO);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        GetOrAdd<GraphicRaycaster>(canvasGO);

        var scaler                   = GetOrAdd<CanvasScaler>(canvasGO);
        scaler.uiScaleMode           = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution   = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight    = 0.5f;

        BuildHUD(canvasGO, btnItem);
        BuildOverlay(canvasGO);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // ── HUD ──────────────────────────────────────────────────────────────────

    static void BuildHUD(GameObject canvasGO, GameObject btnItemPrefab)
    {
        var hud     = FindOrCreateChild(canvasGO, "HUD");
        StretchFull(hud);
        var hudCtrl = GetOrAdd<HUDController>(hud);

        // Timer bar background
        var timerBg    = FindOrCreateChild(hud, "TimerBarBG");
        var timerBgRt  = GetOrAdd<RectTransform>(timerBg);
        timerBgRt.anchorMin        = new Vector2(0.5f, 1f);
        timerBgRt.anchorMax        = new Vector2(0.5f, 1f);
        timerBgRt.pivot            = new Vector2(0.5f, 1f);
        timerBgRt.anchoredPosition = new Vector2(0f, -10f);
        timerBgRt.sizeDelta        = new Vector2(500f, 14f);
        GetOrAdd<Image>(timerBg).color = new Color(0.15f, 0.15f, 0.15f);

        // Timer fill
        var timerBarGO   = FindOrCreateChild(timerBg, "TimerBar");
        StretchFull(timerBarGO);
        var timerBar     = GetOrAdd<Image>(timerBarGO);
        timerBar.type        = Image.Type.Filled;
        timerBar.fillMethod  = Image.FillMethod.Horizontal;
        timerBar.color       = new Color(0.33f, 0.70f, 1f);

        // Timer label
        var timerLabelGO = FindOrCreateChild(hud, "TimerLabel");
        var timerLabelRt = GetOrAdd<RectTransform>(timerLabelGO);
        timerLabelRt.anchorMin        = new Vector2(0.5f, 1f);
        timerLabelRt.anchorMax        = new Vector2(0.5f, 1f);
        timerLabelRt.pivot            = new Vector2(0.5f, 1f);
        timerLabelRt.anchoredPosition = new Vector2(0f, -28f);
        timerLabelRt.sizeDelta        = new Vector2(100f, 28f);
        var timerLabel = GetOrAdd<TextMeshProUGUI>(timerLabelGO);
        timerLabel.text      = "10.0s";
        timerLabel.fontSize  = 22;
        timerLabel.alignment = TextAlignmentOptions.Center;
        timerLabel.color     = new Color(0.33f, 0.70f, 1f);

        // Loop label (top-left)
        var loopLabelGO = FindOrCreateChild(hud, "LoopLabel");
        var loopLabelRt = GetOrAdd<RectTransform>(loopLabelGO);
        loopLabelRt.anchorMin        = new Vector2(0f, 1f);
        loopLabelRt.anchorMax        = new Vector2(0f, 1f);
        loopLabelRt.pivot            = new Vector2(0f, 1f);
        loopLabelRt.anchoredPosition = new Vector2(12f, -12f);
        loopLabelRt.sizeDelta        = new Vector2(260f, 28f);
        var loopLabel   = GetOrAdd<TextMeshProUGUI>(loopLabelGO);
        loopLabel.text     = "Loop #1   Ghosts: 0";
        loopLabel.fontSize = 18;

        // Stage label (top-right)
        var stageLabelGO = FindOrCreateChild(hud, "StageLabel");
        var stageLabelRt = GetOrAdd<RectTransform>(stageLabelGO);
        stageLabelRt.anchorMin        = new Vector2(1f, 1f);
        stageLabelRt.anchorMax        = new Vector2(1f, 1f);
        stageLabelRt.pivot            = new Vector2(1f, 1f);
        stageLabelRt.anchoredPosition = new Vector2(-12f, -12f);
        stageLabelRt.sizeDelta        = new Vector2(200f, 28f);
        var stageLabel    = GetOrAdd<TextMeshProUGUI>(stageLabelGO);
        stageLabel.text      = "STAGE 1";
        stageLabel.fontSize  = 18;
        stageLabel.alignment = TextAlignmentOptions.Right;

        // Hint label (bottom-center)
        var hintLabelGO = FindOrCreateChild(hud, "HintLabel");
        var hintLabelRt = GetOrAdd<RectTransform>(hintLabelGO);
        hintLabelRt.anchorMin        = new Vector2(0.5f, 0f);
        hintLabelRt.anchorMax        = new Vector2(0.5f, 0f);
        hintLabelRt.pivot            = new Vector2(0.5f, 0f);
        hintLabelRt.anchoredPosition = new Vector2(0f, 16f);
        hintLabelRt.sizeDelta        = new Vector2(700f, 28f);
        var hintLabel   = GetOrAdd<TextMeshProUGUI>(hintLabelGO);
        hintLabel.text      = "hint";
        hintLabel.fontSize  = 16;
        hintLabel.alignment = TextAlignmentOptions.Center;
        hintLabel.color     = new Color(0.7f, 0.7f, 0.7f);

        // ButtonRoot (right side)
        var btnRoot = FindOrCreateChild(hud, "ButtonRoot");
        var btnRootRt = GetOrAdd<RectTransform>(btnRoot);
        btnRootRt.anchorMin        = new Vector2(1f, 0.5f);
        btnRootRt.anchorMax        = new Vector2(1f, 0.5f);
        btnRootRt.pivot            = new Vector2(1f, 0.5f);
        btnRootRt.anchoredPosition = new Vector2(-12f, 0f);
        btnRootRt.sizeDelta        = new Vector2(160f, 160f);
        var layout = GetOrAdd<VerticalLayoutGroup>(btnRoot);
        layout.spacing                  = 6f;
        layout.childForceExpandHeight   = false;
        layout.childAlignment           = TextAnchor.MiddleRight;

        var so = new SerializedObject(hudCtrl);
        so.FindProperty("_timerLabel").objectReferenceValue    = timerLabel;
        so.FindProperty("_timerBar").objectReferenceValue      = timerBar;
        so.FindProperty("_loopLabel").objectReferenceValue     = loopLabel;
        so.FindProperty("_stageLabel").objectReferenceValue    = stageLabel;
        so.FindProperty("_hintLabel").objectReferenceValue     = hintLabel;
        so.FindProperty("_btnRoot").objectReferenceValue       = btnRoot.transform;
        so.FindProperty("_btnItemPrefab").objectReferenceValue =
            btnItemPrefab.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(hudCtrl);
    }

    // ── Overlay ──────────────────────────────────────────────────────────────

    static void BuildOverlay(GameObject canvasGO)
    {
        var overlay     = FindOrCreateChild(canvasGO, "Overlay");
        StretchFull(overlay);
        var overlayCtrl = GetOrAdd<OverlayController>(overlay);

        // Stage Clear Panel
        var clearPanel = FindOrCreateChild(overlay, "StageClearPanel");
        SetCenterPanel(clearPanel, new Vector2(460f, 240f));
        GetOrAdd<Image>(clearPanel).color = new Color(0f, 0f, 0f, 0.82f);

        var clearTitleTmp = MakeLabel(clearPanel, "Title",
            new Vector2(0f, 80f), new Vector2(400f, 50f), "STAGE CLEAR", 34);
        clearTitleTmp.color = new Color(0.27f, 0.92f, 0.47f);

        var clearLoopTmp  = MakeLabel(clearPanel, "ClearLoopText",
            new Vector2(0f, 16f), new Vector2(360f, 30f), "Loops used: 0", 20);
        var clearGhostTmp = MakeLabel(clearPanel, "ClearGhostText",
            new Vector2(0f, -18f), new Vector2(360f, 30f), "Ghosts spawned: 0", 20);

        var nextTmp = MakeLabel(clearPanel, "NextHint",
            new Vector2(0f, -72f), new Vector2(360f, 24f), "Press SPACE to continue", 15);
        nextTmp.color = new Color(0.65f, 0.65f, 0.65f);

        // Game Won Panel
        var wonPanel = FindOrCreateChild(overlay, "GameWonPanel");
        SetCenterPanel(wonPanel, new Vector2(460f, 200f));
        GetOrAdd<Image>(wonPanel).color = new Color(0f, 0f, 0f, 0.85f);

        var wonTitleTmp = MakeLabel(wonPanel, "Title",
            new Vector2(0f, 30f), new Vector2(380f, 56f), "YOU WIN!", 42);
        wonTitleTmp.color = new Color(1f, 0.85f, 0.3f);

        var wonSubTmp = MakeLabel(wonPanel, "Sub",
            new Vector2(0f, -26f), new Vector2(320f, 28f), "Press R to restart", 16);
        wonSubTmp.color = new Color(0.7f, 0.7f, 0.7f);

        clearPanel.SetActive(false);
        wonPanel.SetActive(false);

        var so = new SerializedObject(overlayCtrl);
        so.FindProperty("_stageClearPanel").objectReferenceValue = clearPanel;
        so.FindProperty("_clearLoopText").objectReferenceValue   = clearLoopTmp;
        so.FindProperty("_clearGhostText").objectReferenceValue  = clearGhostTmp;
        so.FindProperty("_gameWonPanel").objectReferenceValue    = wonPanel;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(overlayCtrl);
    }

    static TextMeshProUGUI MakeLabel(GameObject parent, string name,
        Vector2 anchoredPos, Vector2 size, string text, float fontSize)
    {
        var go  = FindOrCreateChild(parent, name);
        var rt  = GetOrAdd<RectTransform>(go);
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = size;
        var tmp = GetOrAdd<TextMeshProUGUI>(go);
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    // ── Layout Helpers ───────────────────────────────────────────────────────

    static void StretchFull(GameObject go)
    {
        var rt       = GetOrAdd<RectTransform>(go);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void SetCenterPanel(GameObject go, Vector2 size)
    {
        var rt              = GetOrAdd<RectTransform>(go);
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = size;
        rt.anchoredPosition = Vector2.zero;
    }

    // ── General Helpers ──────────────────────────────────────────────────────

    static GameObject FindOrCreate(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go : new GameObject(name);
    }

    static GameObject FindOrCreateChild(GameObject parent, string childName)
    {
        var t = parent.transform.Find(childName);
        if (t != null) return t.gameObject;
        var child = new GameObject(childName);
        child.transform.SetParent(parent.transform, false);
        return child;
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path[..slash], path[(slash + 1)..]);
    }
}
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TimeLoop;

/// <summary>
/// HTML 원본 픽셀 좌표 → Unity 월드 좌표 변환 후 StageData / StageDatabase 에셋을 자동 생성.
/// PPU=32, 캔버스 640×360 기준.
/// Tools > TimeLoop > Create Stage Assets 로 실행.
/// </summary>
public static class StageAssetCreator
{
    const float PPU = 32f;
    const float CH  = 360f; // 캔버스 높이

    // ── 좌표 변환 헬퍼 ────────────────────────────────────────────────────────
    // HTML: top-left origin, Y↓ → Unity: bottom-left origin, Y↑
    static Vector2 Center(float px, float py, float pw, float ph)
        => new((px + pw * 0.5f) / PPU, (CH - py - ph * 0.5f) / PPU);

    static Vector2 Size(float pw, float ph)
        => new(pw / PPU, ph / PPU);

    static PlatformEntry P(float x, float y, float w, float h)
        => new() { center = Center(x, y, w, h), size = Size(w, h) };

    // 버튼 크기: BTN_W=40, BTN_H=16
    static ButtonEntry B(float x, float y, Color col)
        => new() { center = Center(x, y, 40f, 16f), indicatorColor = col };

    static DoorEntry D(float x, float y, float w, float h)
        => new() { center = Center(x, y, w, h), size = Size(w, h) };

    // 스폰: 캐릭터 20×28 px, 좌표는 좌상단 기준
    static Vector2 Spawn(float x, float y)
        => Center(x, y, 20f, 28f);

    // ── 스테이지 정의 ──────────────────────────────────────────────────────────
    static StageData MakeStage1()
    {
        var s = ScriptableObject.CreateInstance<StageData>();
        s.stageName       = "STAGE 1";
        s.hint            = "Step on the button to open the door";
        s.backgroundColor = Hex("#16213e");
        s.spawnPoint      = Spawn(50f, 288f);
        s.platforms = new[]
        {
            P(0f,   320f, 640f, 40f),
            P(220f, 230f, 130f, 14f),
        };
        s.buttons = new[]
        {
            B(255f, 214f, Hex("#c44444")),
        };
        s.door = D(530f, 256f, 22f, 64f);
        return s;
    }

    static StageData MakeStage2()
    {
        var s = ScriptableObject.CreateInstance<StageData>();
        s.stageName       = "STAGE 2";
        s.hint            = "Both buttons must be pressed at the same time — use a ghost!";
        s.backgroundColor = Hex("#1a2e1a");
        s.spawnPoint      = Spawn(50f, 288f);
        s.platforms = new[]
        {
            P(0f,   320f, 640f, 40f),
            P(40f,  230f, 130f, 14f),
            P(470f, 230f, 130f, 14f),
        };
        s.buttons = new[]
        {
            B(75f,  214f, Hex("#c44444")),
            B(505f, 214f, Hex("#44cc44")),
        };
        s.door = D(309f, 256f, 22f, 64f);
        return s;
    }

    static StageData MakeStage3()
    {
        var s = ScriptableObject.CreateInstance<StageData>();
        s.stageName       = "STAGE 3";
        s.hint            = "Three buttons on three levels — two ghosts needed";
        s.backgroundColor = Hex("#2e1a1a");
        s.spawnPoint      = Spawn(30f, 288f);
        s.platforms = new[]
        {
            P(0f,   320f, 640f, 40f),
            P(0f,   240f, 130f, 14f),
            P(510f, 240f, 130f, 14f),
            P(255f, 170f, 130f, 14f),
        };
        s.buttons = new[]
        {
            B(25f,  224f, Hex("#c44444")),
            B(545f, 224f, Hex("#44cc44")),
            B(290f, 154f, Hex("#4488ff")),
        };
        s.door = D(309f, 256f, 22f, 64f);
        return s;
    }

    static StageData MakeStage4()
    {
        var s = ScriptableObject.CreateInstance<StageData>();
        s.stageName       = "STAGE 4";
        s.hint            = "There's a gap in the ground — the door must open for you to cross";
        s.backgroundColor = Hex("#1e1e0a");
        s.spawnPoint      = Spawn(30f, 280f);
        s.platforms = new[]
        {
            P(0f,   320f, 200f, 40f),  // 왼쪽 지면
            P(440f, 320f, 200f, 40f),  // 오른쪽 지면
            P(40f,  215f, 120f, 14f),
            P(480f, 215f, 120f, 14f),
            P(260f, 160f, 120f, 14f),
        };
        s.buttons = new[]
        {
            B(70f,  199f, Hex("#c44444")),
            B(510f, 199f, Hex("#44cc44")),
            B(290f, 144f, Hex("#ffaa55")),
        };
        s.door = D(309f, 256f, 22f, 64f);
        return s;
    }

    static StageData MakeStage5()
    {
        var s = ScriptableObject.CreateInstance<StageData>();
        s.stageName       = "STAGE 5";
        s.hint            = "Four buttons. Plan every loop. You can do it.";
        s.backgroundColor = Hex("#1e0a1e");
        s.spawnPoint      = Spawn(30f, 280f);
        s.platforms = new[]
        {
            P(0f,   320f, 640f, 40f),
            P(0f,   245f, 110f, 14f),
            P(530f, 245f, 110f, 14f),
            P(140f, 190f, 110f, 14f),
            P(390f, 190f, 110f, 14f),
            P(265f, 135f, 110f, 14f),
        };
        s.buttons = new[]
        {
            B(15f,  229f, Hex("#c44444")),
            B(555f, 229f, Hex("#44cc44")),
            B(165f, 174f, Hex("#ffaa55")),
            B(415f, 174f, Hex("#aa55ff")),
        };
        s.door = D(309f, 256f, 22f, 64f);
        return s;
    }

    // ── 실행 ──────────────────────────────────────────────────────────────────
    [MenuItem("Tools/TimeLoop/Create Stage Assets")]
    static void CreateAll()
    {
        const string folder = "Assets/Resources/Stages";
        EnsureFolder("Assets/Resources");
        EnsureFolder(folder);

        var stages = new[]
        {
            SaveAsset(MakeStage1(), $"{folder}/Stage1.asset"),
            SaveAsset(MakeStage2(), $"{folder}/Stage2.asset"),
            SaveAsset(MakeStage3(), $"{folder}/Stage3.asset"),
            SaveAsset(MakeStage4(), $"{folder}/Stage4.asset"),
            SaveAsset(MakeStage5(), $"{folder}/Stage5.asset"),
        };

        // StageDatabase 생성
        const string dbPath = "Assets/Resources/StageDatabase.asset";
        var db = AssetDatabase.LoadAssetAtPath<StageDatabase>(dbPath)
              ?? ScriptableObject.CreateInstance<StageDatabase>();
        db.stages = stages;
        if (AssetDatabase.GetAssetPath(db) == "")
            AssetDatabase.CreateAsset(db, dbPath);
        else
            EditorUtility.SetDirty(db);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[TimeLoop] Stage assets created at Assets/Resources/Stages/");
        Selection.activeObject = db;
    }

    // ── 유틸 ─────────────────────────────────────────────────────────────────
    static T SaveAsset<T>(T asset, string path) where T : Object
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(asset, existing);
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(asset);
            return existing;
        }
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path[..slash], path[(slash + 1)..]);
        }
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}
#endif

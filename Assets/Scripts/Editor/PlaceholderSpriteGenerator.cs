using UnityEngine;
using UnityEditor;
using System.IO;

namespace TimeLoop
{
    /// <summary>
    /// Tools > Generate Placeholder Sprites 를 실행하면
    /// Assets/Resources/Sprites/Player/ 에 idle / walk_0~3 / jump 스프라이트를 생성합니다.
    /// </summary>
    public static class PlaceholderSpriteGenerator
    {
        const int W = 20;
        const int H = 28;
        const string OutDir = "Assets/Resources/Sprites/Player";

        static readonly Color32 CSkin    = new(255, 200, 150, 255);
        static readonly Color32 CShirt   = new( 60, 110, 230, 255);
        static readonly Color32 CPants   = new( 30,  50, 150, 255);
        static readonly Color32 COutline = new( 15,  15,  30, 255);
        static readonly Color32 CEye     = new( 15,  15,  30, 255);
        static readonly Color32 CClear   = new(  0,   0,   0,   0);

        [MenuItem("Tools/Setup Character Animators")]
        public static void SetupAnimators()
        {
            SetupPrefab("Assets/Prefabs/Player.prefab", "Sprites/Player", "");
            SetupPrefab("Assets/Prefabs/Ghost.prefab",  "Sprites/Ghost",  "Sprites/Player");
            AssetDatabase.SaveAssets();
            Debug.Log("[SpriteGen] CharacterAnimator 설정 완료");
            EditorUtility.DisplayDialog("완료", "Player / Ghost 프리팹에 CharacterAnimator 추가 완료!", "OK");
        }

        static void SetupPrefab(string prefabPath, string folder, string fallback)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) { Debug.LogWarning($"프리팹 없음: {prefabPath}"); return; }

            using var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath);
            var root = scope.prefabContentsRoot;

            var anim = root.GetComponent<CharacterAnimator>();
            if (anim == null) anim = root.AddComponent<CharacterAnimator>();

            var so = new SerializedObject(anim);
            so.FindProperty("_folderPath").stringValue         = folder;
            so.FindProperty("_fallbackFolderPath").stringValue = fallback;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("Tools/Generate Placeholder Sprites")]
        public static void Generate()
        {
            Directory.CreateDirectory(OutDir);

            Save(BuildIdle(),    "idle");
            Save(BuildWalk(0),   "walk_0");
            Save(BuildWalk(1),   "walk_1");
            Save(BuildWalk(2),   "walk_2");
            Save(BuildWalk(3),   "walk_3");
            Save(BuildJump(),    "jump");

            AssetDatabase.Refresh();
            ApplyImportSettings();

            Debug.Log($"[SpriteGen] 스프라이트 생성 완료 → {OutDir}");
            EditorUtility.DisplayDialog("완료", $"스프라이트 생성 완료!\n{OutDir}", "OK");
        }

        // ── 상태별 프레임 ────────────────────────────────────────────────────────

        static Texture2D BuildIdle()
        {
            var t = NewTex();
            DrawHead(t, 0);
            DrawBody(t, 0);
            DrawArms(t, 0, 0);
            DrawLegs(t, 7, 0, 10, 10, 0, 10);
            t.Apply(); return t;
        }

        static Texture2D BuildWalk(int frame)
        {
            // [leftX, leftY, leftH, rightX, rightY, rightH]
            int[][] cfg = {
                new[] { 6, 0, 11,  11, 0,  9 },   // 왼발 앞
                new[] { 7, 0, 10,  10, 0, 10 },   // 중립
                new[] { 7, 0,  9,  10, 0, 11 },   // 오른발 앞
                new[] { 7, 0, 10,  10, 0, 10 },   // 중립
            };
            int armSwing = frame == 0 ? 1 : frame == 2 ? -1 : 0;

            var t = NewTex();
            DrawHead(t, 0);
            DrawBody(t, 0);
            DrawArms(t, armSwing, 0);
            var c = cfg[frame];
            DrawLegs(t, c[0], c[1], c[2], c[3], c[4], c[5]);
            t.Apply(); return t;
        }

        static Texture2D BuildJump()
        {
            var t = NewTex();
            DrawHead(t, 0);
            DrawBody(t, 0);
            DrawArms(t, 0, -3);   // 팔 위로
            DrawLegs(t, 6, 2, 8, 11, 2, 8);   // 다리 구부림
            t.Apply(); return t;
        }

        // ── 부위별 드로우 ────────────────────────────────────────────────────────

        static void DrawHead(Texture2D t, int yOff)
        {
            FillRect(t, 6, 20 + yOff, 8, 8, CSkin);
            DrawOutline(t, 6, 20 + yOff, 8, 8, COutline);
            SetPx(t, 8,  24 + yOff, CEye);
            SetPx(t, 11, 24 + yOff, CEye);
        }

        static void DrawBody(Texture2D t, int yOff)
        {
            FillRect(t, 7, 10 + yOff, 6, 10, CShirt);
            DrawOutline(t, 7, 10 + yOff, 6, 10, COutline);
        }

        static void DrawArms(Texture2D t, int swingX, int swingY)
        {
            FillRect(t, 4 + swingX, 13 + swingY, 3, 6, CShirt);
            DrawOutline(t, 4 + swingX, 13 + swingY, 3, 6, COutline);
            FillRect(t, 13 - swingX, 13 + swingY, 3, 6, CShirt);
            DrawOutline(t, 13 - swingX, 13 + swingY, 3, 6, COutline);
        }

        static void DrawLegs(Texture2D t, int lx, int ly, int lh, int rx, int ry, int rh)
        {
            FillRect(t, lx, ly, 3, lh, CPants);
            DrawOutline(t, lx, ly, 3, lh, COutline);
            FillRect(t, rx, ry, 3, rh, CPants);
            DrawOutline(t, rx, ry, 3, rh, COutline);
        }

        // ── 픽셀 유틸 ────────────────────────────────────────────────────────────

        static void FillRect(Texture2D t, int x, int y, int w, int h, Color32 c)
        {
            for (int px = x; px < x + w; px++)
                for (int py = y; py < y + h; py++)
                    SetPx(t, px, py, c);
        }

        static void DrawOutline(Texture2D t, int x, int y, int w, int h, Color32 c)
        {
            for (int px = x; px < x + w; px++)
            {
                SetPx(t, px, y,         c);
                SetPx(t, px, y + h - 1, c);
            }
            for (int py = y; py < y + h; py++)
            {
                SetPx(t, x,         py, c);
                SetPx(t, x + w - 1, py, c);
            }
        }

        static void SetPx(Texture2D t, int x, int y, Color32 c)
        {
            if (x >= 0 && x < W && y >= 0 && y < H) t.SetPixel(x, y, c);
        }

        static Texture2D NewTex()
        {
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            var buf = new Color32[W * H];
            for (int i = 0; i < buf.Length; i++) buf[i] = CClear;
            tex.SetPixels32(buf);
            return tex;
        }

        static void Save(Texture2D tex, string name)
        {
            File.WriteAllBytes($"{OutDir}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        static void ApplyImportSettings()
        {
            foreach (var file in Directory.GetFiles(OutDir, "*.png"))
            {
                var assetPath = file.Replace("\\", "/");
                var importer  = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;

                importer.textureType         = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode          = FilterMode.Point;
                importer.textureCompression  = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }
    }
}

using System;
using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// 스테이지 레이아웃 데이터. Inspector / Editor 스크립트로 세팅.
    /// PPU=32 기준 월드 좌표 (center 기준).
    /// </summary>
    [CreateAssetMenu(menuName = "TimeLoop/StageData", fileName = "StageData")]
    public class StageData : ScriptableObject
    {
        public string stageName;
        [TextArea] public string hint;
        public Color  backgroundColor;
        public Vector2 spawnPoint;

        public PlatformEntry[] platforms;
        public ButtonEntry[]   buttons;
        public DoorEntry       door;
    }

    [Serializable]
    public struct PlatformEntry
    {
        public Vector2 center;
        public Vector2 size;
    }

    [Serializable]
    public struct ButtonEntry
    {
        public Vector2 center;
        public Color   indicatorColor;
    }

    [Serializable]
    public struct DoorEntry
    {
        public Vector2 center;
        public Vector2 size;
    }
}

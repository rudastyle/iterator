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

        public float loopDuration = 10f;
        public int   maxGhosts    = 99;

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
        /// <summary>0 = 영구 유지, &gt;0 = 밟고 떠난 뒤 유지되는 초</summary>
        public float   holdDuration;
        /// <summary>&gt;0 = 밟는 순간 N초 활성, 시간 끝나면 비활성. 나갔다 와야 재트리거.</summary>
        public float   activeDuration;
    }

    [Serializable]
    public struct DoorEntry
    {
        public Vector2 center;
        public Vector2 size;
    }
}

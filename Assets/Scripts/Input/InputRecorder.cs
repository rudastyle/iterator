using System.Collections.Generic;

namespace TimeLoop
{
    /// <summary>
    /// 플레이어 입력을 타임스탬프와 함께 기록. 고스트 재생에 사용.
    /// </summary>
    public class InputRecorder
    {
        readonly List<InputEvent> _log = new();

        public void Record(float time, InputAction action, bool pressed)
            => _log.Add(new InputEvent(time, action, pressed));

        /// <summary>현재 로그의 독립적 복사본을 반환. GhostCharacter 생성 시 사용.</summary>
        public List<InputEvent> GetSnapshot() => new(_log);

        public void Clear() => _log.Clear();
    }
}

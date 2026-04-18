using System.Collections.Generic;
using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// 녹화된 InputLog 를 loopTime 기준으로 재생.
    /// PlayerCharacter 와 동일한 CharacterMover 를 구동하므로 물리가 100% 일치.
    /// </summary>
    [RequireComponent(typeof(CharacterMover))]
    public class GhostCharacter : MonoBehaviour
    {
        CharacterMover    _mover;
        List<InputEvent>  _log;
        int               _ptr;
        bool              _left;
        bool              _right;

        public void Init(List<InputEvent> log, Color color, Vector2 spawnPos)
        {
            _mover = GetComponent<CharacterMover>();
            _log   = log;

            // 고스트 색상 적용
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr) sr.color = new Color(color.r, color.g, color.b, 0.45f);

            ResetState(spawnPos);
        }

        public void Tick(float loopTime)
        {
            // 현재 루프 시간까지의 이벤트를 순서대로 처리
            while (_ptr < _log.Count && _log[_ptr].Time <= loopTime)
            {
                var ev = _log[_ptr++];
                switch (ev.Action)
                {
                    case InputAction.MoveLeft:  _left  = ev.Pressed; break;
                    case InputAction.MoveRight: _right = ev.Pressed; break;
                    case InputAction.Jump:
                        if (ev.Pressed) _mover.TryJump();
                        break;
                }
            }

            _mover.SetMoveAxis(_right ? 1f : _left ? -1f : 0f);
        }

        public void Respawn(Vector2 pos) => ResetState(pos);

        void ResetState(Vector2 pos)
        {
            _ptr   = 0;
            _left  = _right = false;
            _mover.Respawn(pos);
        }
    }
}

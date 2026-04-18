using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// 실제 키보드 입력을 읽고 InputRecorder 에 기록 후 CharacterMover 에 전달.
    /// </summary>
    [RequireComponent(typeof(CharacterMover))]
    public class PlayerCharacter : MonoBehaviour
    {
        CharacterMover _mover;
        InputRecorder  _recorder;

        bool _prevLeft;
        bool _prevRight;

        public void Init(InputRecorder recorder, Vector2 spawnPos)
        {
            _mover    = GetComponent<CharacterMover>();
            _recorder = recorder;
            _prevLeft = _prevRight = false;
            _mover.Respawn(spawnPos);
        }

        public void Tick(float loopTime)
        {
            bool left  = Input.GetKey(KeyCode.LeftArrow)  || Input.GetKey(KeyCode.A);
            bool right = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);

            RecordEdge(loopTime, InputAction.MoveLeft,  left,  ref _prevLeft);
            RecordEdge(loopTime, InputAction.MoveRight, right, ref _prevRight);

            _mover.SetMoveAxis(right ? 1f : left ? -1f : 0f);

            if (Input.GetKeyDown(KeyCode.Space)    ||
                Input.GetKeyDown(KeyCode.W)        ||
                Input.GetKeyDown(KeyCode.UpArrow))
            {
                _recorder.Record(loopTime, InputAction.Jump, true);
                _mover.TryJump();
            }
        }

        public void Respawn(Vector2 pos)
        {
            _prevLeft = _prevRight = false;
            _mover.Respawn(pos);
        }

        void RecordEdge(float t, InputAction action, bool cur, ref bool prev)
        {
            if (cur == prev) return;
            _recorder.Record(t, action, cur);
            prev = cur;
        }
    }
}

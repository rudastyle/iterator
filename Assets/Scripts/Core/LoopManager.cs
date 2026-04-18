using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// 루프 타이머 전담. 만료 여부만 반환하고 게임 로직은 GameManager 책임.
    /// </summary>
    public class LoopManager : MonoBehaviour
    {
        [SerializeField] float _duration = 10f;

        public float LoopTime  { get; private set; }
        public float Duration  => _duration;
        public float TimeLeft  => Mathf.Max(0f, _duration - LoopTime);

        bool _expired;

        public void ResetLoop()
        {
            LoopTime = 0f;
            _expired = false;
        }

        /// <returns>true = 이번 Tick 에서 루프가 만료됨</returns>
        public bool Tick(float dt)
        {
            if (_expired) return false;

            LoopTime += dt;
            if (LoopTime >= _duration)
            {
                LoopTime = _duration;
                _expired = true;
                return true;
            }
            return false;
        }
    }
}

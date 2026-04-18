using System.Linq;
using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// 모든 PressurePlate 가 동시에 활성화되면 열리는 출구 문.
    /// 닫힘: 물리 벽(isTrigger=false) / 열림: 통과 가능 + 플레이어 감지(isTrigger=true).
    /// </summary>
    public class ExitDoor : MonoBehaviour
    {
        [SerializeField] Collider2D     _col;
        [SerializeField] SpriteRenderer _sprite;

        static readonly Color ClosedColor = new(0.48f, 0.23f, 0.06f, 1f);
        static readonly Color OpenColor   = new(0.27f, 0.92f, 0.47f, 0.40f);

        PressurePlate[] _plates;
        bool            _open;

        public bool IsOpen => _open;

        public void Init(PressurePlate[] plates)
        {
            _plates        = plates;
            _open          = false;
            _col.isTrigger = false;
            _sprite.color  = ClosedColor;
        }

        void Update()
        {
            bool allActive = _plates != null
                          && _plates.Length > 0
                          && _plates.All(p => p.IsActive);

            if (allActive == _open) return;

            _open          = allActive;
            _col.isTrigger = _open;
            _sprite.color  = _open ? OpenColor : ClosedColor;
        }

        void FixedUpdate()
        {
            // isTrigger 전환 직후 Overlap 이 OnTriggerEnter 를 놓칠 수 있으므로
            // 열린 상태에서는 매 물리 프레임 직접 체크.
            if (!_open) return;

            var b    = _col.bounds;
            var hits = Physics2D.OverlapBoxAll(b.center, b.size * 0.9f, 0f);
            foreach (var h in hits)
            {
                if (h.CompareTag("Player"))
                {
                    GameManager.Instance.OnPlayerReachedDoor();
                    return;
                }
            }
        }
    }
}

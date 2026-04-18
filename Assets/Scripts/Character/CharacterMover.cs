using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// Rigidbody2D 기반 이동·점프 물리 처리.
    /// Player / Ghost 공용 컴포넌트. 입력 제공은 각 캐릭터 클래스가 담당.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class CharacterMover : MonoBehaviour
    {
        [SerializeField] float _moveSpeed  = 6.25f;   // 200 px/s ÷ 32
        [SerializeField] float _jumpForce  = 13.125f; // 420 px/s ÷ 32
        [SerializeField] LayerMask _groundMask;

        Rigidbody2D   _rb;
        BoxCollider2D _col;

        void Awake()
        {
            _rb  = GetComponent<Rigidbody2D>();
            _col = GetComponent<BoxCollider2D>();
            _rb.constraints             = RigidbodyConstraints2D.FreezeRotation;
            _rb.collisionDetectionMode  = CollisionDetectionMode2D.Continuous;
        }

        // ── Public API ───────────────────────────────────────────────────────────

        public void SetMoveAxis(float axis)
        {
            _rb.linearVelocity = new Vector2(axis * _moveSpeed, _rb.linearVelocity.y);
        }

        public void TryJump()
        {
            if (IsGrounded())
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
        }

        public void Respawn(Vector2 worldPos)
        {
            _rb.linearVelocity       = Vector2.zero;
            transform.position = worldPos;
        }

        public bool IsGrounded()
        {
            var b      = _col.bounds;
            int mask   = _groundMask.value != 0 ? (int)_groundMask : Physics2D.AllLayers;
            var origin = new Vector2(b.center.x, b.min.y + 0.02f);
            return Physics2D.BoxCast(
                origin,
                new Vector2(b.size.x * 0.8f, 0.04f),
                0f, Vector2.down, 0.1f, mask);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// 캐릭터 무게에 따라 기울어지는 시소 플랫폼.
    /// - 왼쪽 > 오른쪽 : 왼쪽 하강 / 오른쪽 상승
    /// - 양쪽 같거나 아무도 없으면 현재 각도 유지 (sticky)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class SeesawPlatform : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _body;

        float _maxAngle;
        float _rotateSpeed;

        Rigidbody2D _rb;
        readonly HashSet<Rigidbody2D> _passengers = new();
        float _currentAngle;

        static readonly Color NeutralColor = new(0.45f, 0.30f, 0.55f);
        static readonly Color TiltColor    = new(0.27f, 0.85f, 1f);
        static readonly Collider2D[] _sensorBuf = new Collider2D[8];

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType                = RigidbodyType2D.Kinematic;
            _rb.useFullKinematicContacts = true;
            _rb.gravityScale            = 0f;
        }

        public void Init(float maxAngle, float rotateSpeed)
        {
            _maxAngle    = maxAngle    > 0f ? maxAngle    : 22f;
            _rotateSpeed = rotateSpeed > 0f ? rotateSpeed : 60f;
        }

        void FixedUpdate()
        {
            Vector2 pivot    = _rb.position;
            float   worldLen = Mathf.Abs(transform.localScale.x);

            // 매 프레임 OverlapBox로 탑승자 갱신 — 트리거 콜백보다 신뢰성 높음
            _passengers.Clear();
            int n = Physics2D.OverlapBoxNonAlloc(
                pivot + new Vector2(0f, 0.5f),
                new Vector2(worldLen, 3f),
                0f, _sensorBuf);

            int leftCount = 0, rightCount = 0;
            for (int i = 0; i < n; i++)
            {
                if (!IsCharacter(_sensorBuf[i])) continue;
                var rb = _sensorBuf[i].attachedRigidbody;
                if (rb == null || !_passengers.Add(rb)) continue;
                if (rb.position.x < pivot.x) leftCount++;
                else                          rightCount++;
            }

            float targetAngle;
            if      (leftCount  > rightCount) targetAngle = +_maxAngle; // 왼쪽 하강
            else if (rightCount > leftCount)  targetAngle = -_maxAngle; // 오른쪽 하강
            else                              targetAngle = _currentAngle; // sticky

            float prevAngle = _currentAngle;
            _currentAngle = Mathf.MoveTowards(_currentAngle, targetAngle, _rotateSpeed * Time.fixedDeltaTime);
            float deltaAngle = _currentAngle - prevAngle;

            if (Mathf.Abs(deltaAngle) > 0.0001f)
            {
                foreach (var p in _passengers)
                    if (p != null) p.position = RotateAround(p.position, pivot, deltaAngle);

                _rb.MoveRotation(_currentAngle);
            }

            if (_body)
                _body.color = Mathf.Abs(_currentAngle) > 1f ? TiltColor : NeutralColor;
        }

        public void ResetSeesaw()
        {
            _passengers.Clear();
            _currentAngle = 0f;
            _rb.MoveRotation(0f);
            if (_body) _body.color = NeutralColor;
        }

        static Vector2 RotateAround(Vector2 point, Vector2 pivot, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            Vector2 d = point - pivot;
            return pivot + new Vector2(d.x * cos - d.y * sin, d.x * sin + d.y * cos);
        }

        static bool IsCharacter(Collider2D c)
            => c.CompareTag("Player") || c.CompareTag("Ghost");
    }
}

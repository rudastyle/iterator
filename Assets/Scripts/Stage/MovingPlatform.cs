using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// N명 이상 탑승 시 pointA → pointB 로 이동하는 플랫폼.
    /// 인원이 부족하면 멈추고, pointB 도달 후에는 정지.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _body;

        int     _requiredCount;
        Vector2 _pointA;
        Vector2 _pointB;
        float   _speed;

        Rigidbody2D               _rb;
        readonly HashSet<Rigidbody2D> _passengers = new();
        bool _reached;

        TextMeshPro _counterText;

        static readonly Color IdleColor   = new(0.35f, 0.35f, 0.50f);
        static readonly Color ReadyColor  = new(0.27f, 1f,    0.27f);
        static readonly Color ReachedColor = new(0.27f, 0.85f, 1f);

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType                = RigidbodyType2D.Kinematic;
            _rb.useFullKinematicContacts = true;
            _rb.gravityScale            = 0f;
            _rb.interpolation           = RigidbodyInterpolation2D.Interpolate;
            _rb.constraints             = RigidbodyConstraints2D.FreezeRotation;
        }

        public void Init(Vector2 pointA, Vector2 pointB, float speed, int requiredCount)
        {
            _pointA        = pointA;
            _pointB        = pointB;
            _speed         = speed;
            _requiredCount = requiredCount;
            _rb.position   = pointA;

            CreateCounterText();
            UpdateVisuals(0);
        }

        void CreateCounterText()
        {
            var go = new GameObject("CounterLabel");
            go.transform.SetParent(transform);

            // 플랫폼 스케일에 역보정해서 텍스트가 항상 월드 단위 크기 유지
            var s = transform.localScale;
            go.transform.localPosition = new Vector3(0f, 1f, 0f);
            go.transform.localScale    = new Vector3(1f / s.x, 1f / s.y, 1f);

            _counterText                    = go.AddComponent<TextMeshPro>();
            _counterText.alignment          = TextAlignmentOptions.Center;
            _counterText.fontSize           = 2f;
            _counterText.sortingOrder       = 5;
            _counterText.enableWordWrapping = false;
        }

        void FixedUpdate()
        {
            if (_reached) return;

            int  count      = _passengers.Count;
            bool shouldMove = count >= _requiredCount;

            if (shouldMove)
            {
                Vector2 curr   = _rb.position;
                Vector2 newPos = Vector2.MoveTowards(curr, _pointB, _speed * Time.fixedDeltaTime);
                Vector2 delta  = newPos - curr;

                _rb.MovePosition(newPos);

                foreach (var p in _passengers)
                    if (p != null) p.position += delta;

                if (Vector2.Distance(newPos, _pointB) < 0.02f)
                    _reached = true;
            }

            UpdateVisuals(count);
        }

        void UpdateVisuals(int count)
        {
            if (_body)
            {
                _body.color = _reached
                    ? ReachedColor
                    : (count >= _requiredCount ? ReadyColor : IdleColor);
            }

            if (_counterText)
            {
                _counterText.text  = _reached ? "" : $"{count}/{_requiredCount}";
                _counterText.color = count >= _requiredCount ? ReadyColor : Color.white;
            }
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (!IsCharacter(col.collider)) return;
            if (col.rigidbody == null) return;
            // 캐릭터 중심이 플랫폼 중심보다 위에 있으면 탑승으로 판정
            // (수평 진입 & 낙하 진입 모두 처리)
            if (col.transform.position.y >= _rb.position.y)
                _passengers.Add(col.rigidbody);
        }

        void OnCollisionExit2D(Collision2D col)
        {
            if (!IsCharacter(col.collider)) return;
            if (col.rigidbody != null) _passengers.Remove(col.rigidbody);
        }

        public void ResetPlatform()
        {
            _passengers.Clear();
            _reached = false;
            _rb.MovePosition(_pointA);
            UpdateVisuals(0);
        }

        static bool IsCharacter(Collider2D c)
            => c.CompareTag("Player") || c.CompareTag("Ghost");
    }
}

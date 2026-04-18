using System.Collections.Generic;
using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// 캐릭터가 올라서면 활성화되는 압력판.
    /// holdDuration > 0 이면 캐릭터가 떠난 뒤에도 그 시간만큼 활성 상태를 유지한다.
    /// </summary>
    public class PressurePlate : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _body;
        [SerializeField] SpriteRenderer _indicator;

        static readonly Color ActiveColor   = new(0.27f, 1f,   0.27f);
        static readonly Color InactiveColor = new(0.38f, 0.13f, 0.13f);
        static readonly Color TimedColor    = new(1f,    0.85f, 0.27f); // 타이머 유지 중

        Color _baseColor;
        float _holdDuration;   // 0 = 영구, >0 = 떠난 뒤 유지 초
        float _holdTimer;      // 남은 유지 시간
        bool  _holding;        // 현재 타이머로 유지 중

        readonly HashSet<Collider2D> _contacts = new();

        public bool IsActive => _contacts.Count > 0 || _holding;

        public void SetColor(Color c)
        {
            _baseColor = c;
            if (_indicator) _indicator.color = c;
        }

        public void SetHoldDuration(float duration) => _holdDuration = duration;

        void Update()
        {
            if (_holding)
            {
                _holdTimer -= Time.deltaTime;
                if (_holdTimer <= 0f)
                {
                    _holdTimer = 0f;
                    _holding   = false;
                }
            }

            if (_body)
            {
                if (_contacts.Count > 0)      _body.color = ActiveColor;
                else if (_holding)             _body.color = TimedColor;
                else                           _body.color = InactiveColor;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsCharacter(other)) return;
            _contacts.Add(other);
            _holding = false; // 다시 밟히면 타이머 취소
        }

        void OnTriggerExit2D(Collider2D other)
        {
            _contacts.Remove(other);
            if (_contacts.Count == 0 && _holdDuration > 0f)
            {
                _holding   = true;
                _holdTimer = _holdDuration;
            }
        }

        public void ResetContacts()
        {
            _contacts.Clear();
            _holding   = false;
            _holdTimer = 0f;
        }

        static bool IsCharacter(Collider2D c)
            => c.CompareTag("Player") || c.CompareTag("Ghost");
    }
}

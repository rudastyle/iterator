using System.Collections.Generic;
using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// 캐릭터가 올라서면 활성화되는 압력판.
    /// Trigger Collider 로 Player / Ghost 태그를 감지.
    /// </summary>
    public class PressurePlate : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _body;
        [SerializeField] SpriteRenderer _indicator;

        static readonly Color ActiveColor   = new(0.27f, 1f,   0.27f);
        static readonly Color InactiveColor = new(0.38f, 0.13f, 0.13f);

        Color _baseColor;
        readonly HashSet<Collider2D> _contacts = new();

        public bool IsActive => _contacts.Count > 0;

        public void SetColor(Color c)
        {
            _baseColor = c;
            if (_indicator) _indicator.color = c;
        }

        void Update()
        {
            if (_body) _body.color = IsActive ? ActiveColor : InactiveColor;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (IsCharacter(other)) _contacts.Add(other);
        }

        void OnTriggerExit2D(Collider2D other)
            => _contacts.Remove(other);

        /// <summary>루프 리셋 시 텔레포트로 인한 Exit 이벤트 누락 방지.</summary>
        public void ResetContacts() => _contacts.Clear();

        static bool IsCharacter(Collider2D c)
            => c.CompareTag("Player") || c.CompareTag("Ghost");
    }
}

using System.Collections.Generic;
using TMPro;
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

        // holdDuration 모드: 밟고 떠난 뒤 N초 유지
        float _holdDuration;
        float _holdTimer;
        bool  _holding;

        // activeDuration 모드: 밟는 순간 N초 활성, 나갔다 와야 재트리거
        float _activeDuration;
        float _activeTimer;
        bool  _canRetrigger = true; // 현재 트리거 영역 밖에 있는지

        readonly HashSet<Collider2D> _contacts = new();

        bool IsTimedMode => _activeDuration > 0f;

        public bool IsActive => IsTimedMode
            ? _activeTimer > 0f
            : _contacts.Count > 0 || _holding;

        public void SetColor(Color c)
        {
            _baseColor = c;
            if (_indicator) _indicator.color = c;
        }

        TextMeshPro _timerText;

        public void SetHoldDuration(float duration) => _holdDuration = duration;

        public void SetActiveDuration(float duration)
        {
            _activeDuration = duration;
            if (duration > 0f && _timerText == null)
                CreateTimerText();
        }

        void CreateTimerText()
        {
            var go = new GameObject("TimerLabel");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            go.transform.localScale    = Vector3.one;

            _timerText                     = go.AddComponent<TextMeshPro>();
            _timerText.alignment           = TextAlignmentOptions.Center;
            _timerText.fontSize            = 2.5f;
            _timerText.color               = _baseColor != default ? _baseColor : Color.white;
            _timerText.sortingOrder        = 5;
            _timerText.enableWordWrapping  = false;
            go.SetActive(false);
        }

        void Update()
        {
            if (IsTimedMode)
            {
                if (_activeTimer > 0f)
                {
                    _activeTimer -= Time.deltaTime;
                    if (_activeTimer <= 0f) _activeTimer = 0f;
                }
            }
            else
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
            }

            if (_body)
            {
                if (IsTimedMode)
                    _body.color = _activeTimer > 0f ? ActiveColor : InactiveColor;
                else if (_contacts.Count > 0) _body.color = ActiveColor;
                else if (_holding)            _body.color = TimedColor;
                else                          _body.color = InactiveColor;
            }

            if (_timerText != null)
            {
                bool show = _activeTimer > 0f;
                _timerText.gameObject.SetActive(show);
                if (show)
                    _timerText.text = Mathf.CeilToInt(_activeTimer).ToString();
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsCharacter(other)) return;
            _contacts.Add(other);

            if (IsTimedMode)
            {
                if (_canRetrigger)
                {
                    _activeTimer  = _activeDuration;
                    _canRetrigger = false;
                }
            }
            else
            {
                _holding = false;
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            _contacts.Remove(other);

            if (IsTimedMode)
            {
                if (_contacts.Count == 0)
                    _canRetrigger = true;
            }
            else
            {
                if (_contacts.Count == 0 && _holdDuration > 0f)
                {
                    _holding   = true;
                    _holdTimer = _holdDuration;
                }
            }
        }

        public void ResetContacts()
        {
            _contacts.Clear();
            _holding      = false;
            _holdTimer    = 0f;
            _activeTimer  = 0f;
            _canRetrigger = true;
            if (_timerText != null) _timerText.gameObject.SetActive(false);
        }

        static bool IsCharacter(Collider2D c)
            => c.CompareTag("Player") || c.CompareTag("Ghost");
    }
}

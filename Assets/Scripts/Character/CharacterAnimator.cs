using System.Collections.Generic;
using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// Resources/Sprites/{folderPath}/ 에서 idle/walk/jump 스프라이트를 로드해 애니메이션 처리.
    /// 파일명 규칙: idle.png / idle_0.png, idle_1.png ... (다중 프레임)
    ///             walk_0.png, walk_1.png ...
    ///             jump.png / jump_0.png ...
    /// Ghost 는 ghostFolderPath 우선, 없으면 playerFolderPath 폴백.
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        [SerializeField] string _folderPath = "Sprites/Player";
        [SerializeField] string _fallbackFolderPath = "";   // Ghost 가 비어있을 때 폴백
        [SerializeField] float  _animFps = 8f;

        SpriteRenderer _sr;
        CharacterMover _mover;
        Rigidbody2D    _rb;

        Sprite[] _idleSprites;
        Sprite[] _walkSprites;
        Sprite[] _jumpSprites;

        enum State { Idle, Walk, Jump }
        State _state;
        int   _frame;
        float _frameTimer;
        bool  _hasSprites;

        void Awake()
        {
            _sr    = GetComponentInChildren<SpriteRenderer>();
            _mover = GetComponent<CharacterMover>();
            _rb    = GetComponent<Rigidbody2D>();

            _idleSprites = Load("idle", _folderPath, _fallbackFolderPath);
            _walkSprites = Load("walk", _folderPath, _fallbackFolderPath);
            _jumpSprites = Load("jump", _folderPath, _fallbackFolderPath);

            _hasSprites = _idleSprites.Length > 0 || _walkSprites.Length > 0 || _jumpSprites.Length > 0;

            if (_hasSprites && _sr != null)
                _sr.drawMode = SpriteDrawMode.Simple;
        }

        void Update()
        {
            if (!_hasSprites || _sr == null) return;

            UpdateState();
            UpdateFrame();
        }

        void UpdateState()
        {
            float vx = _rb.linearVelocity.x;

            State next;
            if (!_mover.IsGrounded())
                next = State.Jump;
            else if (Mathf.Abs(vx) > 0.05f)
                next = State.Walk;
            else
                next = State.Idle;

            if (next != _state)
            {
                _state = next;
                _frame = 0;
                _frameTimer = 0f;
            }

            if (vx > 0.05f)       _sr.flipX = false;
            else if (vx < -0.05f) _sr.flipX = true;
        }

        void UpdateFrame()
        {
            Sprite[] sprites = _state switch
            {
                State.Walk => _walkSprites.Length > 0 ? _walkSprites : _idleSprites,
                State.Jump => _jumpSprites.Length > 0 ? _jumpSprites : _idleSprites,
                _          => _idleSprites,
            };

            if (sprites.Length == 0) return;

            _frameTimer += Time.deltaTime;
            if (_frameTimer >= 1f / _animFps)
            {
                _frameTimer = 0f;
                _frame = (_frame + 1) % sprites.Length;
            }

            _sr.sprite = sprites[_frame];
        }

        // ── 로딩 ────────────────────────────────────────────────────────────────

        static Sprite[] Load(string state, string folder, string fallback)
        {
            var sprites = LoadFromFolder(state, folder);
            if (sprites.Length == 0 && !string.IsNullOrEmpty(fallback))
                sprites = LoadFromFolder(state, fallback);
            return sprites;
        }

        static Sprite[] LoadFromFolder(string state, string folder)
        {
            var list = new List<Sprite>();

            // 다중 프레임: state_0, state_1, ...
            for (int i = 0; ; i++)
            {
                var sp = Resources.Load<Sprite>($"{folder}/{state}_{i}");
                if (sp == null) break;
                list.Add(sp);
            }

            // 단일 프레임: state
            if (list.Count == 0)
            {
                var sp = Resources.Load<Sprite>($"{folder}/{state}");
                if (sp != null) list.Add(sp);
            }

            return list.ToArray();
        }
    }
}

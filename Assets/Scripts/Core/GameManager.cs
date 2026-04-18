using System.Collections.Generic;
using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// 게임 전체 상태 머신. 스테이지 로딩·루프 리셋·클리어 판정을 총괄.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ── Inspector ────────────────────────────────────────────────────────────
        [Header("Data")]
        [SerializeField] StageDatabase _database;

        [Header("Systems")]
        [SerializeField] LoopManager  _loopManager;
        [SerializeField] StageLoader  _stageLoader;

        [Header("Prefabs")]
        [SerializeField] GameObject _playerPrefab;
        [SerializeField] GameObject _ghostPrefab;

        // ── Public State ─────────────────────────────────────────────────────────
        public GameState    State        { get; private set; }
        public int          StageIndex   { get; private set; }
        public StageData    CurrentStage => _database.stages[StageIndex];
        public int          LoopCount    { get; private set; }
        public int          GhostCount   => _ghosts.Count;
        public float        TimeLeft     => _loopManager.TimeLeft;
        public float        LoopDuration => _loopManager.Duration;
        public PressurePlate[] CurrentPlates  { get; private set; }
        public Transform        PlayerTransform => _player != null ? _player.transform : null;

        // ── Ghost Colors ─────────────────────────────────────────────────────────
        static readonly Color[] GhostPalette =
        {
            new Color(1f,  0.50f, 0f),
            new Color(1f,  0.30f, 1f),
            new Color(0f,  1f,   1f),
            new Color(1f,  1f,   0.30f),
            new Color(0.5f,1f,   0.5f),
            new Color(1f,  0.53f, 0.53f),
            new Color(0.63f,0.5f,1f),
            new Color(1f,  0.65f, 0.40f),
        };

        // ── Private ──────────────────────────────────────────────────────────────
        PlayerCharacter          _player;
        readonly List<GhostCharacter> _ghosts   = new();
        readonly InputRecorder        _recorder = new();
        Vector2                       _spawn;

        // ─────────────────────────────────────────────────────────────────────────
        void Awake()
        {
            Instance = this;
            SetupLayerCollisions();
        }
        void Start() => LoadStage(0);

        static void SetupLayerCollisions()
        {
            const int ghost  = 7; // Ghost layer
            const int player = 8; // Player layer
            Physics2D.IgnoreLayerCollision(ghost, ghost,  true); // Ghost ↔ Ghost
            Physics2D.IgnoreLayerCollision(ghost, player, true); // Ghost ↔ Player
            // Ghost(7) ↔ Button(9) 는 무시하지 않음 → 트리거 작동
        }

        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.N))
            {
                int next = StageIndex + 1;
                if (next < _database.stages.Length) LoadStage(next);
            }
            if (Input.GetKeyDown(KeyCode.B) && StageIndex > 0)
                LoadStage(StageIndex - 1);
#endif

            switch (State)
            {
                case GameState.Playing:
                    HandlePlaying();
                    break;

                case GameState.StageClear:
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                        LoadStage(StageIndex + 1);
                    break;

                case GameState.GameWon:
                    if (Input.GetKeyDown(KeyCode.R)) LoadStage(0);
                    break;
            }
        }

        void HandlePlaying()
        {
            bool expired = _loopManager.Tick(Time.deltaTime);

            _player.Tick(_loopManager.LoopTime);

            foreach (var g in _ghosts)
                g.Tick(_loopManager.LoopTime);

            if (expired || Input.GetKeyDown(KeyCode.R))
                ResetLoop(spawnGhost: true);
        }

        // ── Stage Loading ────────────────────────────────────────────────────────
        void LoadStage(int idx)
        {
            StageIndex = idx;
            var data   = _database.stages[idx];
            _spawn     = data.spawnPoint;

            // 이전 오브젝트 정리
            if (_player != null) Destroy(_player.gameObject);
            foreach (var g in _ghosts) Destroy(g.gameObject);
            _ghosts.Clear();
            _recorder.Clear();
            LoopCount = 0;
            _loopManager.SetDuration(data.loopDuration);
            _loopManager.ResetLoop();

            // 스테이지 빌드
            _stageLoader.Build(data, out var plates, out var door);
            CurrentPlates = plates;
            door.Init(plates);

            // 플레이어 스폰
            _player = Instantiate(_playerPrefab).GetComponent<PlayerCharacter>();
            _player.gameObject.tag   = "Player";
            _player.gameObject.layer = 8; // Player layer
            _player.Init(_recorder, _spawn);

            State = GameState.Playing;
        }

        // ── Loop Reset ───────────────────────────────────────────────────────────
        void ResetLoop(bool spawnGhost)
        {
            if (spawnGhost)
            {
                var log = _recorder.GetSnapshot();
                if (log.Count > 0 && _ghosts.Count < CurrentStage.maxGhosts)
                {
                    var color   = GhostPalette[_ghosts.Count % GhostPalette.Length];
                    var ghostGO = Instantiate(_ghostPrefab);
                    ghostGO.tag   = "Ghost";
                    ghostGO.layer = 7; // Ghost layer
                    var ghost   = ghostGO.GetComponent<GhostCharacter>();
                    ghost.Init(log, color, _spawn);
                    _ghosts.Add(ghost);
                }
            }

            LoopCount++;
            _recorder.Clear();
            _loopManager.ResetLoop();
            _player.Respawn(_spawn);

            foreach (var g in _ghosts)
                g.Respawn(_spawn);

            // 텔레포트 후 버튼 접촉 초기화
            foreach (var p in CurrentPlates)
                p.ResetContacts();
        }

        // ── Door Callback ────────────────────────────────────────────────────────
        public void OnPlayerReachedDoor()
        {
            if (State != GameState.Playing) return;

            State = StageIndex >= _database.stages.Length - 1
                ? GameState.GameWon
                : GameState.StageClear;
        }
    }
}

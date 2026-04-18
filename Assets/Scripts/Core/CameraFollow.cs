using System.Collections.Generic;
using UnityEngine;

namespace TimeLoop
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] float _smoothTime = 0.12f;
        [SerializeField] float _orthoSize  = 3.5f;

        const float StageW = 20f;
        const float StageH = 11.25f;

        const float TourSmoothTime       = 0.10f;
        const float TourHoldTime         = 0.4f;
        const float TourArriveThreshold  = 0.08f;

        Camera  _cam;
        Vector3 _vel;

        readonly List<Vector2> _tourPoints = new();
        int   _tourIdx;
        float _tourHold;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographicSize = _orthoSize;
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (gm.State == GameState.Intro)
            {
                // 스테이지가 바뀔 때마다 첫 진입 시 투어 초기화
                if (_tourPoints.Count == 0) StartTour(gm);
                RunTour(gm);
                return;
            }

            // Intro 끝나면 다음 스테이지를 위해 초기화
            _tourPoints.Clear();

            // ── 일반 플레이어 추적 ────────────────────────────────────────────────
            var target = gm.PlayerTransform;
            if (target == null) return;

            var desired = Clamped(target.position);
            transform.position = Vector3.SmoothDamp(
                transform.position, desired, ref _vel, _smoothTime);
        }

        void StartTour(GameManager gm)
        {
            var stage = gm.CurrentStage;
            foreach (var b in stage.buttons) _tourPoints.Add(b.center);
            _tourPoints.Add(stage.door.center);
            _tourIdx  = 0;
            _tourHold = 0f;
            _vel      = Vector3.zero;
        }

        void RunTour(GameManager gm)
        {
            if (_tourIdx < _tourPoints.Count)
            {
                // 투어 중에는 클램프 없이 정확히 포인트로 이동
                var desired = new Vector3(_tourPoints[_tourIdx].x, _tourPoints[_tourIdx].y, transform.position.z);
                transform.position = Vector3.SmoothDamp(
                    transform.position, desired, ref _vel, TourSmoothTime);

                if (Vector3.Distance(transform.position, desired) < TourArriveThreshold)
                {
                    _tourHold += Time.deltaTime;
                    if (_tourHold >= TourHoldTime)
                    {
                        _tourIdx++;
                        _tourHold = 0f;
                        _vel      = Vector3.zero;
                    }
                }
            }
            else
            {
                // 플레이어 위치로 복귀 후 게임 시작
                var pt = gm.PlayerTransform;
                if (pt == null) { gm.BeginPlay(); return; }

                var desired = Clamped(pt.position);
                transform.position = Vector3.SmoothDamp(
                    transform.position, desired, ref _vel, TourSmoothTime);

                if (Vector3.Distance(transform.position, desired) < TourArriveThreshold)
                    gm.BeginPlay();
            }
        }

        Vector3 Clamped(Vector3 pos)
        {
            float hw = _cam.orthographicSize * _cam.aspect;
            float hh = _cam.orthographicSize;
            return new Vector3(
                Mathf.Clamp(pos.x, hw,  StageW - hw),
                Mathf.Clamp(pos.y, hh,  StageH - hh),
                transform.position.z);
        }

        Vector3 Clamped(Vector2 pos) => Clamped((Vector3)pos);
    }
}

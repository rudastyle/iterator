using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// 플레이어를 부드럽게 추적하며 스테이지 경계 안에 머무름.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] float _smoothTime   = 0.12f;
        [SerializeField] float _orthoSize    = 3.5f;

        // Stage world bounds (640×360 canvas ÷ 32 PPU)
        const float StageW = 20f;
        const float StageH = 11.25f;

        Camera  _cam;
        Vector3 _vel;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographicSize = _orthoSize;
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            var target = gm.PlayerTransform;
            if (target == null) return;

            float hw = _cam.orthographicSize * _cam.aspect;
            float hh = _cam.orthographicSize;

            float tx = Mathf.Clamp(target.position.x, hw,      StageW - hw);
            float ty = Mathf.Clamp(target.position.y, hh,      StageH - hh);

            var desired = new Vector3(tx, ty, transform.position.z);
            transform.position = Vector3.SmoothDamp(
                transform.position, desired, ref _vel, _smoothTime);
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLoop
{
    /// <summary>
    /// 인게임 HUD: 타이머, 루프 정보, 버튼 상태 표시.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Timer")]
        [SerializeField] TextMeshProUGUI _timerLabel;
        [SerializeField] Image           _timerBar;

        [Header("Info")]
        [SerializeField] TextMeshProUGUI _loopLabel;
        [SerializeField] TextMeshProUGUI _stageLabel;
        [SerializeField] TextMeshProUGUI _hintLabel;

        [Header("Button States")]
        [SerializeField] Transform           _btnRoot;
        [SerializeField] TextMeshProUGUI     _btnItemPrefab;

        static readonly Color ColorCold = new(0.33f, 0.70f, 1f);
        static readonly Color ColorWarm = new(1f,    0.60f, 0.20f);
        static readonly Color ColorHot  = new(1f,    0.33f, 0.33f);

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            bool playing = gm.State == GameState.Playing;
            gameObject.SetActive(playing || gm.State == GameState.StageClear);

            if (!playing) return;

            float tl    = gm.TimeLeft;
            var   tColor = tl < 3f ? ColorHot : tl < 6f ? ColorWarm : ColorCold;

            _timerLabel.text  = tl.ToString("F1") + "s";
            _timerLabel.color = tColor;

            _timerBar.fillAmount = gm.LoopDuration > 0f ? tl / gm.LoopDuration : 0f;
            _timerBar.color      = tColor;

            _loopLabel.text  = $"Loop #{gm.LoopCount + 1}   Ghosts: {gm.GhostCount}";
            _stageLabel.text = gm.CurrentStage.stageName;
            _hintLabel.text  = gm.CurrentStage.hint;

            SyncButtonItems(gm);
        }

        void SyncButtonItems(GameManager gm)
        {
            var plates = gm.CurrentPlates;
            if (plates == null) return;

            // 필요한 수만큼 아이템 증감
            while (_btnRoot.childCount < plates.Length)
                Instantiate(_btnItemPrefab, _btnRoot);

            for (int i = _btnRoot.childCount - 1; i >= plates.Length; i--)
                Destroy(_btnRoot.GetChild(i).gameObject);

            for (int i = 0; i < plates.Length; i++)
            {
                bool on = plates[i].IsActive;
                var  t  = _btnRoot.GetChild(i).GetComponent<TextMeshProUGUI>();
                t.text  = (on ? "● " : "○ ") + $"Button {i + 1}";
                t.color = on ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f);
            }
        }
    }
}

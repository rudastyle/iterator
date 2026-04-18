using TMPro;
using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// Stage Clear / Game Won 오버레이 패널 제어.
    /// </summary>
    public class OverlayController : MonoBehaviour
    {
        [Header("Stage Clear")]
        [SerializeField] GameObject      _stageClearPanel;
        [SerializeField] TextMeshProUGUI _clearLoopText;
        [SerializeField] TextMeshProUGUI _clearGhostText;

        [Header("Game Won")]
        [SerializeField] GameObject _gameWonPanel;

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            bool clear = gm.State == GameState.StageClear;
            bool won   = gm.State == GameState.GameWon;

            _stageClearPanel.SetActive(clear);
            _gameWonPanel.SetActive(won);

            if (clear)
            {
                _clearLoopText.text  = $"Loops used: {gm.LoopCount}";
                _clearGhostText.text = $"Ghosts spawned: {gm.GhostCount}";
            }
        }
    }
}

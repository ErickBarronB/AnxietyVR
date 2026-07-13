using UnityEngine;
using TMPro;

namespace Minigame
{
    public class MinigameTextCycler : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private ChaseCubeMinigame minigame;
        [SerializeField] private TMP_Text uiText;
        [SerializeField] private TMP_Text countdownText;

        [Header("Textos de Fases (Tramo 1-4)")]
        [SerializeField] private string[] segmentTexts = { "Inhalá", "Mantené", "Exhalá", "Mantené" };
        [SerializeField] private float segmentDuration = 2.5f;

        [Header("Textos de Estado")]
        [SerializeField] private string inactiveText = "Presiona iniciar para calmar tu mente";
        [SerializeField] private string completionText = "¡Vamos, lo lograste!";

        [SerializeField] private System_PlayerAnxiety anxiety;
        [SerializeField] private float calmDuration = 7f;

        private bool isCompleted = false;

        private void Start()
        {
            if (minigame == null)
                minigame = FindObjectOfType<ChaseCubeMinigame>();

            if (uiText == null)
                uiText = GetComponent<TMP_Text>();

            if (minigame != null)
            {
                minigame.onMinigameStarted.AddListener(OnMinigameStarted);
                minigame.onMinigameCompleted.AddListener(OnMinigameCompleted);
                minigame.onWaypointReached.AddListener(OnWaypointReached);
            }
            else
            {
                Debug.LogError("[MinigameTextCycler] No se encontro ChaseCubeMinigame.");
            }

            ShowInactiveState();
        }

        private void Update()
        {
            if (minigame == null || !minigame.IsMinigameActive || isCompleted) return;

            int segmentIndex = minigame.CurrentSegmentIndex % segmentTexts.Length;
            if (uiText != null)
                uiText.text = segmentTexts[segmentIndex];

            float elapsed = minigame.SegmentTimer;
            int countdown = Mathf.Clamp(Mathf.CeilToInt(segmentDuration - elapsed), 1, (int)segmentDuration);
            if (countdownText != null)
                countdownText.text = countdown.ToString();
        }

        private void OnWaypointReached(int waypointIndex) { }

        private void OnMinigameStarted()
        {
            isCompleted = false;
            if (uiText != null && segmentTexts != null && segmentTexts.Length > 0)
                uiText.text = segmentTexts[0];
            if (countdownText != null)
                countdownText.text = ((int)segmentDuration).ToString();
        }

        private void OnMinigameCompleted()
        {
            isCompleted = true;
            if (uiText != null)
                uiText.text = completionText;
            if (countdownText != null)
                countdownText.text = "";

            if (anxiety != null)
                anxiety.TriggerCalm(calmDuration);
        }

        private void ShowInactiveState()
        {
            if (uiText != null)
                uiText.text = isCompleted ? completionText : inactiveText;
            if (countdownText != null)
                countdownText.text = "";
        }
    }
}

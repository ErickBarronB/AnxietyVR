using UnityEngine;
using TMPro;

namespace Minigame
{
    public class MinigameTextCycler : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private ChaseCubeMinigame minigame;
        [SerializeField] private TMP_Text uiText;

        [Header("Pila / Lista de Palabras")]
        [SerializeField] private string[] words = { "Respira profundamente...", "Siente tu cuerpo...", "Suelta la tensión...", "Mantén el enfoque...", "Todo está bien...", "Estás a salvo..." };
        [SerializeField] private string inactiveText = "Presiona iniciar para calmar tu mente";
        [SerializeField] private string completionText = "¡Excelente trabajo!";

        [Header("Tiempos & Reglas")]
        [SerializeField] private float wordDuration = 2.0f;
        [SerializeField] private int minWordsPerWaypoint = 3;
        [SerializeField] private bool requireContactToCycle = true;

        private int currentWordIndex = -1;
        private int wordsShownThisWaypoint = 0;
        private float timer = 0f;
        private bool isCompleted = false;

        private void Start()
        {
            if (minigame == null)
            {
                minigame = FindObjectOfType<ChaseCubeMinigame>();
            }

            if (uiText == null)
            {
                uiText = GetComponent<TMP_Text>();
            }

            if (minigame != null)
            {
                minigame.shouldWaitCallback = ShouldWaitAtWaypoint;
                minigame.onMinigameStarted.AddListener(OnMinigameStarted);
                minigame.onMinigameCompleted.AddListener(OnMinigameCompleted);
                minigame.onWaypointReached.AddListener(OnWaypointReached);
            }
            else
            {
                Debug.LogError("[MinigameTextCycler] No se encontro ChaseCubeMinigame.");
            }

            ShowInactiveText();
        }

        private void Update()
        {
            if (minigame == null || !minigame.IsMinigameActive) return;

            if (requireContactToCycle && !minigame.IsHandTouching) return;

            timer += Time.deltaTime;
            if (timer >= wordDuration)
            {
                timer = 0f;
                ShowNextWord();
            }
        }

        private bool ShouldWaitAtWaypoint()
        {
            return wordsShownThisWaypoint < minWordsPerWaypoint;
        }

        private void OnWaypointReached(int waypointIndex)
        {
            if (!minigame.IsWaitingAtWaypoint)
            {
                wordsShownThisWaypoint = 0;
            }
        }

        private void OnMinigameStarted()
        {
            isCompleted = false;
            currentWordIndex = 0;
            wordsShownThisWaypoint = 0;
            timer = 0f;

            if (words != null && words.Length > 0)
            {
                ShowWord(words[0]);
                wordsShownThisWaypoint = 1;
            }
        }

        private void OnMinigameCompleted()
        {
            isCompleted = true;
            ShowWord(completionText);
        }

        private void ShowNextWord()
        {
            if (words == null || words.Length == 0) return;

            currentWordIndex = (currentWordIndex + 1) % words.Length;
            ShowWord(words[currentWordIndex]);
            wordsShownThisWaypoint++;

            if (minigame.IsWaitingAtWaypoint && wordsShownThisWaypoint >= minWordsPerWaypoint)
            {
                wordsShownThisWaypoint = 0;
                minigame.ResumeMovement();
            }
        }

        private void ShowWord(string word)
        {
            if (uiText != null)
            {
                uiText.text = word;
            }
        }

        private void ShowInactiveText()
        {
            if (uiText != null)
            {
                uiText.text = isCompleted ? completionText : inactiveText;
            }
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Minigame
{
    public class PMRMinigame : MonoBehaviour
    {
        private enum PMRState
        {
            Idle,
            Intro,
            TensePrompt,
            Tensing,
            ReleasePrompt,
            Releasing,
            Rest,
            Complete
        }

        [Header("Input VR (gatillos Meta XR)")]
        [Tooltip("Valor mínimo del grip para considerar que está apretado (0-1)")]
        [SerializeField] private float gripThreshold = 0.7f;

        [Header("Flujo del ejercicio")]
        [SerializeField] private int totalRounds = 4;
        [SerializeField] private float tenseHoldDuration = 5f;
        [SerializeField] private float releaseDuration = 10f;
        [SerializeField] private float restBetweenRounds = 2f;

        [Header("Sistema de Ansiedad")]
        [SerializeField] private float anxietyReductionPerRound = 8f;
        [SerializeField] private float anxietyReductionOnComplete = 15f;

        [Header("UI Diegética")]
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text roundText;
        [Tooltip("Panel UI diegética (para activar/desactivar)")]
        [SerializeField] private GameObject uiPanel;

        [Header("Inicio")]
        [SerializeField] private bool startOnStart = false;

        [Header("Eventos")]
        public UnityEvent onMinigameStarted;
        public UnityEvent onRoundCompleted;
        public UnityEvent onMinigameCompleted;

        private PMRState state = PMRState.Idle;
        private int currentRound = 0;
        private float stateTimer = 0f;
        private bool bothGripsHeld = false;

        [Header("Sistema de Ansiedad - Referencia")]
        [SerializeField] private System_PlayerAnxiety anxietySystem;
        private Coroutine activeCoroutine;

        private void Awake()
        {
            if (anxietySystem == null)
                anxietySystem = FindObjectOfType<System_PlayerAnxiety>();
        }

        private void Update()
        {
            if (state == PMRState.Idle || state == PMRState.Intro || state == PMRState.Complete)
                return;

            bothGripsHeld = ReadGrips();

            HandleState();
        }

        public void StartMinigame()
        {
            if (state != PMRState.Idle && state != PMRState.Complete) return;

            currentRound = 0;
            SetUIActive(true);
            onMinigameStarted?.Invoke();

            if (activeCoroutine != null) StopCoroutine(activeCoroutine);
            activeCoroutine = StartCoroutine(IntroSequence());
        }

        public void StopMinigame()
        {
            if (activeCoroutine != null) StopCoroutine(activeCoroutine);
            SetState(PMRState.Idle);
            SetUIActive(false);
        }

        private IEnumerator IntroSequence()
        {
            SetState(PMRState.Intro);
            SetInstruction("Vamos a hacer\nRelajación Muscular\nProgresiva de Jacobson.\n\nAhora, tensar y soltar.");
            SetTimerText("");
            SetRoundText($"0 / {totalRounds}");
            yield return new WaitForSeconds(4f);
            BeginNextRound();
        }

        private void BeginNextRound()
        {
            currentRound++;
            SetRoundText($"{currentRound} / {totalRounds}");
            SetState(PMRState.TensePrompt);
            stateTimer = 0f;
            SetInstruction("Apretá los puños\ncon los gatillos.\nMantenelos apretados.");
            SetTimerText("esperando...");
        }

        private void HandleState()
        {
            stateTimer += Time.deltaTime;

            switch (state)
            {
                case PMRState.TensePrompt:
                    if (bothGripsHeld)
                    {
                        SetState(PMRState.Tensing);
                        stateTimer = 0f;
                        SetInstruction($"¡Bien! Mantené la tensión...");
                    }
                    break;

                case PMRState.Tensing:
                    float tensionLeft = tenseHoldDuration - stateTimer;
                    SetTimerText(FormatTimer(tensionLeft));

                    if (!bothGripsHeld)
                    {
                        SetInstruction("Seguí apretando\nlos dos gatillos.");
                        SetState(PMRState.TensePrompt);
                        stateTimer = 0f;
                        break;
                    }

                    if (stateTimer >= tenseHoldDuration)
                    {
                        SetState(PMRState.ReleasePrompt);
                        stateTimer = 0f;
                        SetInstruction("¡Soltá los puños!\nDejá que las manos\ncaigan relajadas.");
                        SetTimerText("soltá...");
                    }
                    break;

                case PMRState.ReleasePrompt:
                    if (!bothGripsHeld)
                    {
                        SetState(PMRState.Releasing);
                        stateTimer = 0f;
                        SetInstruction("Sentí la diferencia...\nRespirate y relajate.");
                    }
                    break;

                case PMRState.Releasing:
                    float releaseLeft = releaseDuration - stateTimer;
                    SetTimerText(FormatTimer(releaseLeft));

                    if (stateTimer >= releaseDuration)
                    {
                        CompleteRound();
                    }
                    break;

                case PMRState.Rest:
                    if (stateTimer >= restBetweenRounds)
                    {
                        if (currentRound < totalRounds)
                            BeginNextRound();
                        else
                            CompleteMinigame();
                    }
                    break;
            }
        }

        private void CompleteRound()
        {
            if (anxietySystem != null)
                anxietySystem.RemoveAnxiety(anxietyReductionPerRound);

            onRoundCompleted?.Invoke();

            SetState(PMRState.Rest);
            stateTimer = 0f;

            if (currentRound < totalRounds)
            {
                SetInstruction($"Ronda {currentRound} completada.\nPreparate para\nla siguiente.");
                SetTimerText("");
            }
        }

        private void CompleteMinigame()
        {
            if (anxietySystem != null)
                anxietySystem.RemoveAnxiety(anxietyReductionOnComplete);

            SetState(PMRState.Complete);
            SetInstruction("¡Ejercicio completado!\nTus pulsaciones\nbajaron.");
            SetTimerText("");
            onMinigameCompleted?.Invoke();

            if (activeCoroutine != null) StopCoroutine(activeCoroutine);
            activeCoroutine = StartCoroutine(HideUIDelayed(4f));
        }

        private IEnumerator HideUIDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            SetUIActive(false);
        }

        private void SetInstruction(string text)
        {
            if (instructionText != null) instructionText.text = text;
        }

        private void SetTimerText(string text)
        {
            if (timerText != null) timerText.text = text;
        }

        private void SetRoundText(string text)
        {
            if (roundText != null) roundText.text = $"Ronda {text}";
        }

        private void SetUIActive(bool active)
        {
            if (uiPanel != null) uiPanel.SetActive(active);
        }

        private string FormatTimer(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            return $"{Mathf.CeilToInt(seconds)}s";
        }

        private bool ReadGrips()
        {
            bool leftHeld  = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger,  OVRInput.Controller.LTouch) >= gripThreshold && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) >= gripThreshold;
            bool rightHeld = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger,  OVRInput.Controller.RTouch) >= gripThreshold && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) >= gripThreshold;
            return leftHeld && rightHeld;
        }

        private void SetState(PMRState newState)
        {
            state = newState;
        }

        public bool IsActive => state != PMRState.Idle && state != PMRState.Complete;
        public int CurrentRound => currentRound;
        public int TotalRounds => totalRounds;
    }
}

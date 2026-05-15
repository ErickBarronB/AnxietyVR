using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class System_PlayerAnxiety : MonoBehaviour, IAnxietySystem
{
    public enum AnxietyFlowState
    {
        None,
        Increasing,
        Decreasing
    }

    [Header("Values")]
    [SerializeField] private float anxiety = 70f;
    [SerializeField] private float minAnxiety = 60f; // floor, always Normal
    [SerializeField] private float maxAnxiety = 180f;
    [SerializeField] private AnxietyFlowState flowState = AnxietyFlowState.None;

    public bool debugsEnabled = false;

    [Header("Rates")]
    [SerializeField] private float increaseRate = 10f;
    [SerializeField] private float decreaseRate = 1.5f;
    [SerializeField] private float increaseRatePerTrigger = 5f;

    [Header("State")]
    [SerializeField] private Enum_AnxietyLevel currentLevel;

    [Header("Events")]
    [SerializeField] private UnityEvent<Enum_AnxietyLevel> onLevelChanged;

    private int AnxietyTriggers = 0;
    private Coroutine calmCoroutine;


    public event Action<Enum_AnxietyLevel> AnxietyLevelChanged;
    public event Action<int> AnxietyTriggerAdded;
    public event Action<int> AnxietyTriggerRemoved;
    public event Action<bool> IsIncreasingChanged;

    private void Awake()
    {
        UpdateLevel();
    }

    private void Update()
    {
        TickAnxiety(Time.deltaTime);

        if (debugsEnabled)
        {
            Debug.Log("Anxiety: " + anxiety + " Level: " + currentLevel + " Triggers: " + AnxietyTriggers);
        }
    }

    private void TickAnxiety(float deltaTime)
    {
        switch (flowState)
        {
            case AnxietyFlowState.Increasing:
                float dynamicIncrease =
                    increaseRate + (increaseRatePerTrigger * (AnxietyTriggers - 1));

                anxiety += dynamicIncrease * deltaTime;
                break;

            case AnxietyFlowState.Decreasing:
                float normalized = anxiety / maxAnxiety;
                float dynamicDecrease =
                    decreaseRate * Mathf.Lerp(0.5f, 2f, normalized);

                anxiety -= dynamicDecrease * deltaTime;
                break;

            case AnxietyFlowState.None:
                break;
        }

        anxiety = Mathf.Clamp(anxiety, minAnxiety, maxAnxiety);
        UpdateLevel();
    }

    private void UpdateLevel()
    {
        Enum_AnxietyLevel newLevel = GetLevelFromValue(anxiety);

        if (newLevel == currentLevel) return;

        currentLevel = newLevel;

        AnxietyLevelChanged?.Invoke(newLevel);
        onLevelChanged?.Invoke(newLevel);
    }

    private Enum_AnxietyLevel GetLevelFromValue(float value)
    {
        if (value >= 160) return Enum_AnxietyLevel.Max;
        if (value >= 120) return Enum_AnxietyLevel.Critical;
        if (value >= 80) return Enum_AnxietyLevel.High;
        return Enum_AnxietyLevel.Normal;
    }

    public bool IsAnxious() => anxiety >= 80f;

    public void CheckAnxietyTriggers()
    {
        if (AnxietyTriggers > 0)
        {
            SetFlowState(AnxietyFlowState.Increasing);
        }
        else
        {
            SetFlowState(AnxietyFlowState.None);
        }
    }

    public void TriggerCalm(float duration)
    {
        if (calmCoroutine != null)
        {
            StopCoroutine(calmCoroutine);
        }

        calmCoroutine = StartCoroutine(CalmRoutine(duration));
    }

    private IEnumerator CalmRoutine(float duration)
    {
        AnxietyFlowState previousState = flowState;

        SetFlowState(AnxietyFlowState.Decreasing);

        yield return new WaitForSeconds(duration);

        SetFlowState(previousState);
    }

    public void SetFlowState(AnxietyFlowState newState)
    {
        flowState = newState;

        bool isIncreasingNow = flowState == AnxietyFlowState.Increasing;
        IsIncreasingChanged?.Invoke(isIncreasingNow);
    }

    #region Interface Methods

    public void AddAnxiety(float amount)
    {
        anxiety += amount;
        anxiety = Mathf.Clamp(anxiety, minAnxiety, maxAnxiety);
        UpdateLevel();
    }

    public void RemoveAnxiety(float amount)
    {
        anxiety -= amount;
        anxiety = Mathf.Clamp(anxiety, minAnxiety, maxAnxiety);
        UpdateLevel();
    }

    public float GetAnxiety() => anxiety;
    public bool GetIsAnxious() => IsAnxious();
    public Enum_AnxietyLevel GetAnxietyLevel() => currentLevel;
    public bool GetIsIncreasing()
    {
        return flowState == AnxietyFlowState.Increasing;
    }

    public void SetIsIncreasing(bool newIncreasing)
    {
        SetFlowState(
            newIncreasing
                ? AnxietyFlowState.Increasing
                : AnxietyFlowState.None
        );
    }

    public void AddAnxietyTrigger(int TriggerAmounts)
    {
        AnxietyTriggers += TriggerAmounts;
        AnxietyTriggerAdded?.Invoke(TriggerAmounts);
        CheckAnxietyTriggers();
    }

    public void RemoveAnxietyTrigger(int TriggerAmounts)
    {
        AnxietyTriggers = Mathf.Max(0, AnxietyTriggers - TriggerAmounts);
        AnxietyTriggerRemoved?.Invoke(TriggerAmounts);
        CheckAnxietyTriggers();
    }

    #endregion
}
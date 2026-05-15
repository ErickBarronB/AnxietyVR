using TMPro;
using UnityEngine;

public class AnxietyUI : MonoBehaviour
{
    [SerializeField] private System_PlayerAnxiety anxietySystem;
    [SerializeField] private TMP_Text anxietyText;

    void Update()
    {
        float value = anxietySystem.GetAnxiety();

        anxietyText.text = $"Anxiety: {Mathf.RoundToInt(value)}";
    }
}
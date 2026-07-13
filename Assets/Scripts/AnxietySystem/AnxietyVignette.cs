using UnityEngine;
using UnityEngine.UI;

public class AnxietyVignette : MonoBehaviour
{
    [SerializeField] private System_PlayerAnxiety anxietySystem;
    [SerializeField] private Image vignetteImage;

    [Header("Rango de opacidad")]
    [SerializeField] private float minAlpha = 0f;
    [SerializeField] private float maxAlpha = 0.9f;

    [Header("Velocidad de transición")]
    [SerializeField] private float lerpSpeed = 2f;

    private void Awake()
    {
        if (anxietySystem == null)
            anxietySystem = FindObjectOfType<System_PlayerAnxiety>();
        if (vignetteImage == null)
            vignetteImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (anxietySystem == null || vignetteImage == null) return;

        float t = Mathf.InverseLerp(anxietySystem.GetMinAnxiety(), anxietySystem.GetMaxAnxiety(), anxietySystem.GetAnxiety());
        float targetAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = vignetteImage.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * lerpSpeed);
        vignetteImage.color = c;
    }
}

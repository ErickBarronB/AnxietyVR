using UnityEngine;

public class AnxietyVignetteAspect : MonoBehaviour
{
    [SerializeField] private System_PlayerAnxiety anxietySystem;
    [SerializeField] private OVRVignette vignette;

    [Header("Rango de Field Of View del Vignette")]
    [SerializeField] private float fovAtMinAnxiety = 90f;
    [SerializeField] private float fovAtMaxAnxiety = 25f;

    [Header("Curva de respuesta")]
    [SerializeField] private AnimationCurve responseCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Aspect Ratio fijo")]
    [SerializeField] private float fixedAspectRatio = 1f;

    [Header("Velocidad de transición")]
    [SerializeField] private float lerpSpeed = 2f;

    private void Awake()
    {
        if (anxietySystem == null)
            anxietySystem = FindObjectOfType<System_PlayerAnxiety>();
        if (vignette == null)
            vignette = GetComponent<OVRVignette>();

        if (vignette != null)
            vignette.VignetteAspectRatio = fixedAspectRatio;
    }

    private void Update()
    {
        if (anxietySystem == null || vignette == null) return;

        float t = Mathf.InverseLerp(anxietySystem.GetMinAnxiety(), anxietySystem.GetMaxAnxiety(), anxietySystem.GetAnxiety());

        float curvedT = responseCurve.Evaluate(t);

        float targetFov = Mathf.Lerp(fovAtMinAnxiety, fovAtMaxAnxiety, curvedT);

        vignette.VignetteFieldOfView = Mathf.Lerp(vignette.VignetteFieldOfView, targetFov, Time.deltaTime * lerpSpeed);

        vignette.VignetteAspectRatio = fixedAspectRatio;
    }
}
using UnityEngine;

public class HeartbeatAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private System_PlayerAnxiety anxietySystem;

    [Header("Volumen")]
    [SerializeField] private float minVolume = 0f;
    [SerializeField] private float maxVolume = 1f;

    private void Awake()
    {
        if (anxietySystem == null)
            anxietySystem = FindObjectOfType<System_PlayerAnxiety>();
    }

    private void Update()
    {
        if (anxietySystem == null || audioSource == null) return;

        float t = Mathf.InverseLerp(anxietySystem.GetMinAnxiety(), anxietySystem.GetMaxAnxiety(), anxietySystem.GetAnxiety());

        audioSource.volume = Mathf.Lerp(minVolume, maxVolume, t);
    }
}

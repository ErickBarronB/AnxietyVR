using System.Collections;
using UnityEngine;

public class AnxietyEvent_LaughingPeople : MonoBehaviour
{
    [SerializeField] private System_PlayerAnxiety anxiety;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip laughingClip;
    [SerializeField] private AudioClip thoughtClip;

    [Header("Minigame")]
    [SerializeField] private GameObject miniGame;

    [Header("Ansiedad")]
    [SerializeField] private float anxietyRatePerSecond = 5f;
    [SerializeField] private float maxAnxietyGain = 50f;
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (triggerOnlyOnce && triggered)
            return;

        triggered = true;

        StartCoroutine(EventRoutine());

        miniGame.SetActive(true);
    }

    private IEnumerator EventRoutine()
    {
        float totalAdded = 0f;

        audioSource.PlayOneShot(laughingClip);
        float elapsed = 0f;
        while (elapsed < laughingClip.length)
        {
            totalAdded = AddGradual(totalAdded);
            elapsed += Time.deltaTime;
            yield return null;
        }

        audioSource.PlayOneShot(thoughtClip);
        elapsed = 0f;
        while (elapsed < thoughtClip.length)
        {
            totalAdded = AddGradual(totalAdded);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private float AddGradual(float totalAdded)
    {
        if (totalAdded >= maxAnxietyGain) return totalAdded;
        float toAdd = Mathf.Min(anxietyRatePerSecond * Time.deltaTime, maxAnxietyGain - totalAdded);
        anxiety.AddAnxiety(toAdd);
        return totalAdded + toAdd;
    }
}

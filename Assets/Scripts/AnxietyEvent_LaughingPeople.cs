using System.Collections;
using UnityEngine;

public class AnxietyEvent_LaughingPeople : MonoBehaviour
{
    [SerializeField] private System_PlayerAnxiety anxiety;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip laughingClip;
    [SerializeField] private AudioClip thoughtClip;

    [SerializeField] private int triggerAmount = 50;
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
    }

    private IEnumerator EventRoutine()
    {
        anxiety.AddAnxietyTrigger(triggerAmount);

        audioSource.PlayOneShot(laughingClip);

        yield return new WaitForSeconds(laughingClip.length);

        audioSource.PlayOneShot(thoughtClip);

        yield return new WaitForSeconds(thoughtClip.length);
    }
}

using UnityEngine;

public class TriggerSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string playerTag = "Player";

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;
        if (audioSource != null)
            audioSource.Play();
    }
}

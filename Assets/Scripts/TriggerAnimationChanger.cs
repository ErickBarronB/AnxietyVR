using UnityEngine;

public class TriggerAnimationChanger : MonoBehaviour
{
    [SerializeField] private Animator[] characters;
    [SerializeField] private string triggerName = "Activate";

    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;

            foreach (Animator animator in characters)
            {
                animator.SetTrigger(triggerName);
            }
        }
    }
}
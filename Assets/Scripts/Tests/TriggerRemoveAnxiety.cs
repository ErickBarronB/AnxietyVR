using UnityEngine;

public class TriggerRemoveAnxiety : MonoBehaviour
{
    [SerializeField] private System_PlayerAnxiety anxiety;
    [SerializeField] private int TriggerAmount = 1;
    [SerializeField] private bool triggerOnlyOnce = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && triggerOnlyOnce)
        {
            anxiety.RemoveAnxietyTrigger(TriggerAmount);
            triggerOnlyOnce = false;
        }
    }
}

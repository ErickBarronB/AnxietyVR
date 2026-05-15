using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Base_TriggerZone : MonoBehaviour, Iinteractable
{
    [Header("Trigger Settings")]
    [SerializeField] private bool triggeredByCollision = true;
    [SerializeField] private string triggerTag = "Player";
    [SerializeField] private List<GameObject> interactablesToTrigger;

    private bool isInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isInside) return;
        if (other.CompareTag(triggerTag) && triggeredByCollision)
        {
            isInside = true;
            Interact(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            isInside = false;
        }
    }

    public void Interact(GameObject Instigator)
    {
        foreach (var interactable in interactablesToTrigger)
        {
            if (interactable == null) continue;

            if (interactable.TryGetComponent(out Iinteractable i))
            {
                i.Interact(Instigator);
            }
            else
            {
                Debug.LogWarning($"{interactable.name} is in the trigger list but has no Iinteractable component");
            }
        }
    }
}
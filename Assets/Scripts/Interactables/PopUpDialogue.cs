using UnityEngine;
using System.Collections.Generic;

public class PopUpDialogue : MonoBehaviour, Iinteractable
{
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    [SerializeField] private bool triggersOnce = true;
    [SerializeField] private bool parentToInstigator = false;
    [SerializeField] private float fontSize = 1f;
    private bool hasTriggered = false;

    public void Interact(GameObject Instigator)
    {
        if (triggersOnce && hasTriggered) return;
        hasTriggered = true;

        Camera mainCamera = Camera.main;
        Vector3 forward = mainCamera.transform.forward;
        float desiredDistance = 2f;

        if (Physics.Raycast(mainCamera.transform.position, forward, out RaycastHit hit, desiredDistance))
        {
            desiredDistance = hit.distance - 0.1f;
        }

        Vector3 spawnPosition = mainCamera.transform.position + forward * desiredDistance;

        // PlayDialogue first (resets parent internally), THEN reparent
        WorldSpaceDialogueSystem.Instance.PlayDialogue(dialogueLines, spawnPosition, fontSize);

        if (parentToInstigator)
        {
            WorldSpaceDialogueSystem.Instance.SetParent(Instigator.transform);
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

public class PopUpDialogue : MonoBehaviour, Iinteractable
{
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    [SerializeField] private bool triggersOnce = true;
    [SerializeField] private bool parentToInstigator = false;
    [SerializeField] private float fontSize = 1f;
    [SerializeField] private Transform textPosition;
    private bool hasTriggered = false;

    public void Interact(GameObject Instigator)
    {
        if (triggersOnce && hasTriggered) return;
        hasTriggered = true;

        Vector3 spawnPosition = textPosition != null ? textPosition.position : transform.position;

        WorldSpaceDialogueSystem.Instance.PlayDialogue(dialogueLines, spawnPosition, fontSize);

        if (parentToInstigator)
        {
            WorldSpaceDialogueSystem.Instance.SetParent(Instigator.transform);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalManager : MonoBehaviour
{
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    [SerializeField] private float delayAfterDialogue = 1.5f;
    [SerializeField] private Transform TextPosition;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dialogueAudio;

    private bool hasBeenTriggered = false;

    public void Load()
    {
        if (hasBeenTriggered) return;
        hasBeenTriggered = true;

        if (audioSource != null && dialogueAudio != null)
        {
            audioSource.clip = dialogueAudio;
            audioSource.Play();
        }

      

        if (WorldSpaceDialogueSystem.Instance == null)
        {
            Debug.LogError("FinalManager: WorldSpaceDialogueSystem.Instance es null. " +
                "Asegurate de arrancar desde la escena MainMenu (ahi vive el singleton) o de colocarlo tambien en GameScene.", this);
            return;
        }

        if (TextPosition == null)
        {
            Debug.LogError("FinalManager: TextPosition no esta asignado en el Inspector.", this);
            return;
        }

        WorldSpaceDialogueSystem.Instance.PlayDialogue(dialogueLines, TextPosition.position);
        StartCoroutine(WaitForDialogueThenLoad());
    }

    private IEnumerator WaitForDialogueThenLoad()
    {
        float readingDelay = WorldSpaceDialogueSystem.Instance.readingDelay;

        float totalTime = 0f;
        foreach (DialogueLine line in dialogueLines)
        {
            totalTime += line.line.Length * line.typingSpeed + readingDelay;
        }

        yield return new WaitForSeconds(totalTime + delayAfterDialogue);

        yield return FadeManager.Instance.FadeOut();

        Application.Quit();
    }
}

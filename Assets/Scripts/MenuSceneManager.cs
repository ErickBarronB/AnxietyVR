using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MenuSceneManager : MonoBehaviour
{
    [SerializeField] private string sceneName = "GameScene";
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    [SerializeField] private float delayAfterDialogue = 1.5f;
    [SerializeField] private Transform textPosition;

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


        WorldSpaceDialogueSystem.Instance.PlayDialogue(dialogueLines, textPosition.position);
        StartCoroutine(WaitForDialogueThenLoad());
        Debug.Log("snake puto");
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

        SceneManager.LoadScene(sceneName);
    }

}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FinalManager : MonoBehaviour
{
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    [SerializeField] private float delayAfterDialogue = 1.5f;
    [SerializeField] private Transform TextPosition;
    [SerializeField] private TMP_Text minigamesText;
    [SerializeField] private int minigameIndex = 0;
    [SerializeField] private int totalMinigamesRequired = 3;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dialogueAudio;

    private bool hasBeenTriggered = false;

    public int MinigameIndex { get => minigameIndex; set { minigameIndex = value; UpdateMinigamesText(); } }

    private void Start()
    {
        UpdateMinigamesText();
    }

    private void UpdateMinigamesText()
    {
        if (minigamesText != null)
            minigamesText.text = $"{minigameIndex} / {totalMinigamesRequired}";
    }

    public void Load()
    {
        if (MinigameIndex < totalMinigamesRequired)
            return;

        if (hasBeenTriggered) return;
        hasBeenTriggered = true;

        if (audioSource != null && dialogueAudio != null)
        {
            audioSource.clip = dialogueAudio;
            audioSource.Play();
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

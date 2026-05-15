using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MenuSceneManager : MonoBehaviour
{
    [SerializeField] private string sceneName = "GameScene";
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    [SerializeField] private float delayAfterDialogue = 1.5f;

    private bool hasBeenTriggered = false;

    public void Load()
    {
        if (hasBeenTriggered) return;
        hasBeenTriggered = true;

        Camera mainCamera = Camera.main;
        Vector3 forward = mainCamera.transform.forward;
        float desiredDistance = 2f;

        if (Physics.Raycast(mainCamera.transform.position, forward, out RaycastHit hit, desiredDistance))
        {
            desiredDistance = hit.distance - 0.1f;
        }

        Vector3 spawnPosition = mainCamera.transform.position + forward * desiredDistance;

        WorldSpaceDialogueSystem.Instance.PlayDialogue(dialogueLines, spawnPosition);
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

    SceneManager.LoadScene(sceneName);
}

}
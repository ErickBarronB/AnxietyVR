using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public struct DialogueLine
{
    public string line;
    public float typingSpeed;

    public DialogueLine(string line, float typingSpeed)
    {
        this.line = line;
        this.typingSpeed = typingSpeed;
    }
}

public class WorldSpaceDialogueSystem : MonoBehaviour
{
    public static WorldSpaceDialogueSystem Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject textPrefab;
    [HideInInspector] public float typingSpeed = 0.05f;
    public float readingDelay = 2f;

    private TextMeshPro dialogueTextComponent;
    private GameObject dialogueObject;

    private Coroutine typingCoroutine;
    private List<DialogueLine> currentLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeDialogueObject();
    }

    private void InitializeDialogueObject()
    {
        if (textPrefab != null)
        {
            dialogueObject = Instantiate(textPrefab);
            dialogueTextComponent = dialogueObject.GetComponentInChildren<TextMeshPro>();
        }
        else
        {
            dialogueObject = new GameObject("WorldSpaceDialogueText");
            dialogueTextComponent = dialogueObject.AddComponent<TextMeshPro>();
            dialogueTextComponent.alignment = TextAlignmentOptions.Center;
            dialogueTextComponent.fontSize = 5;
            dialogueTextComponent.color = Color.white;
            RectTransform rect = dialogueTextComponent.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(10, 5);
        }

        dialogueObject.transform.SetParent(transform);
        dialogueTextComponent.fontMaterial = new Material(dialogueTextComponent.fontMaterial);
        dialogueTextComponent.fontMaterial.shader = Shader.Find("TextMeshPro/Distance Field Overlay");
        SetRenderActive(false);
    }

    public void SetRenderActive(bool isActive)
    {
        if (dialogueTextComponent != null)
            dialogueTextComponent.enabled = isActive;
    }

    // --- String overloads (backwards compatible) ---
    public void PlayDialogue(List<string> linesToDisplay, Vector3 position)
    {
        PlayDialogue(ToDialogueLines(linesToDisplay, typingSpeed), position);
    }

    public void PlayDialogue(List<string> linesToDisplay, Vector3 position, float fontSize)
    {
        dialogueTextComponent.fontSize = fontSize;
        PlayDialogue(ToDialogueLines(linesToDisplay, typingSpeed), position);
    }

    // --- DialogueLine overloads ---
public void PlayDialogue(List<DialogueLine> linesToDisplay, Vector3 position)
{
    if (linesToDisplay == null || linesToDisplay.Count == 0) return;

    if (typingCoroutine != null) StopCoroutine(typingCoroutine);

    // Reset parent before positioning so world position is always correct
    dialogueObject.transform.SetParent(transform);
    dialogueObject.transform.position = position;
    SetRenderActive(true);

    currentLines = linesToDisplay;
    currentLineIndex = 0;
    isTyping = false;

    ShowNextLine();
}
    public void PlayDialogue(List<DialogueLine> linesToDisplay, Vector3 position, float fontSize)
    {
        dialogueTextComponent.fontSize = fontSize;
        PlayDialogue(linesToDisplay, position);
    }

    public void ShowNextLine()
    {
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogueTextComponent.text = currentLines[currentLineIndex - 1].line;
            isTyping = false;
            return;
        }

        if (currentLineIndex < currentLines.Count)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(currentLines[currentLineIndex]));
            currentLineIndex++;
        }
        else
        {
            SetRenderActive(false);
            dialogueTextComponent.text = string.Empty;
            dialogueObject.transform.SetParent(transform); // reset parent back to manager
        }
    }

    private IEnumerator TypeText(DialogueLine dialogueLine)
    {
        isTyping = true;
        dialogueTextComponent.text = "";

        foreach (char letter in dialogueLine.line.ToCharArray())
        {
            dialogueTextComponent.text += letter;
            yield return new WaitForSeconds(dialogueLine.typingSpeed);
        }

        isTyping = false;
        yield return new WaitForSeconds(readingDelay);
        ShowNextLine();
    }

    private void Update()
    {
        if (dialogueTextComponent != null && dialogueTextComponent.enabled)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                dialogueObject.transform.rotation = Quaternion.LookRotation(dialogueObject.transform.position - mainCamera.transform.position);
        }
    }

    public void SetParent(Transform parent)
    {
        if (dialogueObject != null)
            dialogueObject.transform.SetParent(parent);
    }

    private List<DialogueLine> ToDialogueLines(List<string> lines, float speed)
    {
        List<DialogueLine> result = new List<DialogueLine>();
        foreach (string line in lines)
            result.Add(new DialogueLine(line, speed));
        return result;
    }
}
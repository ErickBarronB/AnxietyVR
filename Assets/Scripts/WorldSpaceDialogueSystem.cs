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
    [Tooltip("Tamaño de fuente en espacio mundo (VR: ~0.1 a 0.3)")]
    public float defaultFontSize = 0.15f;

    private TextMeshPro dialogueTextComponent;
    private GameObject dialogueObject;

    private Coroutine typingCoroutine;
    private List<DialogueLine> currentLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Vector3 targetPosition;

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
            dialogueTextComponent.color = Color.white;
            RectTransform rect = dialogueTextComponent.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(2, 1);
        }

        dialogueTextComponent.fontSize = defaultFontSize;

        dialogueObject.transform.SetParent(transform);
        dialogueTextComponent.fontMaterial = new Material(dialogueTextComponent.fontMaterial);
        SetRenderActive(false);
    }

    public void SetRenderActive(bool isActive)
    {
        if (dialogueTextComponent != null)
            dialogueTextComponent.enabled = isActive;
    }

    public void PlayDialogue(List<string> linesToDisplay, Vector3 position)
    {
        PlayDialogue(ToDialogueLines(linesToDisplay, typingSpeed), position);
    }

    public void PlayDialogue(List<string> linesToDisplay, Vector3 position, float fontSize)
    {
        dialogueTextComponent.fontSize = fontSize;
        PlayDialogue(ToDialogueLines(linesToDisplay, typingSpeed), position);
    }

    public void PlayDialogue(List<DialogueLine> linesToDisplay, Vector3 position)
    {
        if (linesToDisplay == null || linesToDisplay.Count == 0) return;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        targetPosition = position;

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
            dialogueObject.transform.SetParent(transform); 
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
            Camera cam = Camera.main;
            if (cam != null)
            {
                dialogueObject.transform.rotation =
                    Quaternion.LookRotation(dialogueObject.transform.position - cam.transform.position);

                Vector3 dir = targetPosition - cam.transform.position;

                if (Physics.Raycast(cam.transform.position, dir.normalized,
                    out RaycastHit hit, dir.magnitude))
                {
                    if (hit.distance < dir.magnitude - 0.05f)
                    {
                        dialogueObject.transform.position =
                            cam.transform.position + dir.normalized * Mathf.Max(hit.distance - 0.2f, 0.5f);
                    }
                    else
                    {
                        dialogueObject.transform.position = targetPosition;
                    }
                }
                else
                {
                    dialogueObject.transform.position = targetPosition;
                }
            }
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
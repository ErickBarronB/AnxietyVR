using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GroundingMinigame : MonoBehaviour
{
    [Header("Prefabs por Categoría")]
    [SerializeField] private GameObject[] vistaPrefabs;
    [SerializeField] private GameObject[] tactoPrefabs;
    [SerializeField] private GameObject[] oidoPrefabs;
    [SerializeField] private GameObject[] olfatoPrefabs;
    [SerializeField] private GameObject[] gustoPrefabs;

    [Header("Cajas Sensoriales")]
    [SerializeField] private GroundingBox vistaBox;
    [SerializeField] private GroundingBox tactoBox;
    [SerializeField] private GroundingBox oidoBox;
    [SerializeField] private GroundingBox olfatoBox;
    [SerializeField] private GroundingBox gustoBox;

    [Header("Layout (grid en frente del jugador)")]
    [SerializeField] private float spawnDistance = 2.5f;
    [SerializeField] private float xSpacing = 0.45f;
    [SerializeField] private float ySpacing = 0.4f;
    [SerializeField] private float heightOffset = 0.1f;

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;

    [Header("Ansiedad")]
    [SerializeField] private System_PlayerAnxiety anxietySystem;
    [SerializeField] private float anxietyReductionOnComplete = 25f;

    [Header("Eventos")]
    public UnityEvent onMinigameStarted;
    public UnityEvent onMinigameFailed;
    public UnityEvent onMinigameCompleted;

    private bool active;
    private int correctCount;
    private int totalObjects;
    private readonly List<GroundingObject> activeObjects = new List<GroundingObject>();

    public bool IsActive => active;

    private void Awake()
    {
        if (anxietySystem == null)
            anxietySystem = FindObjectOfType<System_PlayerAnxiety>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void StartMinigame()
    {
        if (active) return;

        correctCount = 0;
        activeObjects.Clear();

        vistaBox.Init(this);
        tactoBox.Init(this);
        oidoBox.Init(this);
        olfatoBox.Init(this);
        gustoBox.Init(this);


        var queue = BuildSpawnQueue();
        totalObjects = queue.Count;
        SpawnAllObjects(queue);

        active = true;
        PlaySound(correctSound);
        onMinigameStarted?.Invoke();
    }

    public void OnCorrectDeposit()
    {
        correctCount++;
        PlaySound(correctSound);
        CheckEnd();
    }

    public void OnWrongDeposit()
    {
        PlaySound(wrongSound);
    }

    private void PlaySound(AudioClip clip)
    {
        Debug.Log($"PlaySound: {clip}");

        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void CheckEnd()
    {
        if (!active) return;

        if (correctCount < totalObjects)
            return;

        if (anxietySystem != null)
            anxietySystem.RemoveAnxiety(anxietyReductionOnComplete);

        active = false;

        activeObjects.RemoveAll(o => o == null);

        foreach (var obj in activeObjects)
            if (obj != null)
                Destroy(obj.gameObject);

        activeObjects.Clear();

        onMinigameCompleted?.Invoke();
    }

    private void SpawnAllObjects(List<(GameObject prefab, SensoryCategory cat)> queue)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = cam.transform.right;
        right.y = 0;
        right.Normalize();

        // Grid 5 columnas x 3 filas (para los objetos)
        const int cols = 5;
        for (int i = 0; i < queue.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float x = (col - (cols - 1) * 0.5f) * xSpacing;
            float y = -row * ySpacing + heightOffset;

            Vector3 pos = cam.transform.position + forward * spawnDistance + right * x + Vector3.up * y;

            var (prefab, cat) = queue[i];
            if (prefab == null) continue;

            GameObject go = Instantiate(prefab, pos, prefab.transform.rotation);

            GroundingObject obj = go.GetComponent<GroundingObject>();
            if (obj == null) obj = go.AddComponent<GroundingObject>();
            obj.Init(this, cat, prefab);
            activeObjects.Add(obj);
        }
    }

    public void RegisterObject(GroundingObject obj)
    {
        activeObjects.Add(obj);
    }

    private List<(GameObject, SensoryCategory)> BuildSpawnQueue()
    {
        var list = new List<(GameObject, SensoryCategory)>();
        AddObjects(list, vistaPrefabs,  SensoryCategory.Vista);
        AddObjects(list, tactoPrefabs,  SensoryCategory.Tacto);
        AddObjects(list, oidoPrefabs,   SensoryCategory.Oido);
        AddObjects(list, olfatoPrefabs, SensoryCategory.Olfato);
        AddObjects(list, gustoPrefabs,  SensoryCategory.Gusto);

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    private void AddObjects(List<(GameObject, SensoryCategory)> list, GameObject[] prefabs, SensoryCategory cat)
    {
        if (prefabs == null) return;
        foreach (var prefab in prefabs)
            if (prefab != null) list.Add((prefab, cat));
    }
}

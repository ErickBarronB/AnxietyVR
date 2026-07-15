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
    [SerializeField] private Transform spawnOrigin;
    [SerializeField] private float spawnDistance = 2.5f;
    [SerializeField] private float xSpacing = 0.45f;
    [SerializeField] private float ySpacing = 0.4f;
    [SerializeField] private float heightOffset = 0.1f;
    [SerializeField] private int columns = 5;
    [SerializeField] private bool centerRowsVertically = true;

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

    [SerializeField] private FinalManager finalManager;
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

        finalManager.MinigameIndex++;

        onMinigameCompleted?.Invoke();
    }

    private bool TryGetGridBasis(out Vector3 basePosition, out Vector3 forward, out Vector3 right)
    {
        if (spawnOrigin != null)
        {
            basePosition = spawnOrigin.position;
            forward = spawnOrigin.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            forward.Normalize();

            right = spawnOrigin.right;
            right.y = 0f;
            if (right.sqrMagnitude < 0.0001f) right = Vector3.right;
            right.Normalize();
            return true;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            basePosition = Vector3.zero;
            forward = Vector3.forward;
            right = Vector3.right;
            return false;
        }

        basePosition = cam.transform.position;

        forward = cam.transform.forward;
        forward.y = 0;
        forward.Normalize();

        right = cam.transform.right;
        right.y = 0;
        right.Normalize();
        return true;
    }

    private Vector3 GetGridPosition(int index, int count, Vector3 basePosition, Vector3 forward, Vector3 right)
    {
        int cols = Mathf.Max(1, columns);
        int col = index % cols;
        int row = index / cols;

        float x = (col - (cols - 1) * 0.5f) * xSpacing;

        float y;
        if (centerRowsVertically)
        {
            int totalRows = Mathf.Max(1, Mathf.CeilToInt((float)count / cols));
            y = -(row - (totalRows - 1) * 0.5f) * ySpacing + heightOffset;
        }
        else
        {
            y = -row * ySpacing + heightOffset;
        }

        return basePosition + forward * spawnDistance + right * x + Vector3.up * y;
    }

    private void SpawnAllObjects(List<(GameObject prefab, SensoryCategory cat)> queue)
    {
        if (!TryGetGridBasis(out Vector3 basePosition, out Vector3 forward, out Vector3 right))
            return;

        for (int i = 0; i < queue.Count; i++)
        {
            Vector3 pos = GetGridPosition(i, queue.Count, basePosition, forward, right);

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
        AddObjects(list, vistaPrefabs, SensoryCategory.Vista);
        AddObjects(list, tactoPrefabs, SensoryCategory.Tacto);
        AddObjects(list, oidoPrefabs, SensoryCategory.Oido);
        AddObjects(list, olfatoPrefabs, SensoryCategory.Olfato);
        AddObjects(list, gustoPrefabs, SensoryCategory.Gusto);

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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!TryGetGridBasis(out Vector3 basePosition, out Vector3 forward, out Vector3 right))
            return;

        int count = CountConfigured(vistaPrefabs) + CountConfigured(tactoPrefabs) + CountConfigured(oidoPrefabs) + CountConfigured(olfatoPrefabs) + CountConfigured(gustoPrefabs);

        if (count <= 0) count = 1; 

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetGridPosition(i, count, basePosition, forward, right);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pos, 0.08f);
            UnityEditor.Handles.Label(pos + Vector3.up * 0.12f, i.ToString());
        }

        if (spawnOrigin != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(spawnOrigin.position, 0.05f);
            Gizmos.DrawLine(spawnOrigin.position, spawnOrigin.position + forward * spawnDistance);
        }
    }

    private int CountConfigured(GameObject[] prefabs)
    {
        if (prefabs == null) return 0;
        int c = 0;
        foreach (var p in prefabs)
            if (p != null) c++;
        return c;
    }
#endif
}
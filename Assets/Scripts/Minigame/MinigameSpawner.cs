using UnityEngine;

public class MinigameSpawner : MonoBehaviour
{
    [Header("SetUp")]
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private Transform player;

    [Header("SpawnConfigs")]
    [SerializeField] private float spawnDistance = 2.5f;
    [SerializeField] private float spawnInterval = 1f;

    [SerializeField] private int maxTargetsToSpawn = 15;

    private float timer;
    private bool minigameActive = false;
    private int spawnedTargets = 0;

    private void Update()
    {
        if (!minigameActive) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval && spawnedTargets < maxTargetsToSpawn)
        {
            SpawnTarget();
            timer = 0;
        }

        if (spawnedTargets >= maxTargetsToSpawn && GameObject.FindGameObjectsWithTag("MinigameTarget").Length == 0)
            StopMinigame();
    }

    public void StartMinigame()
    {
        minigameActive = true;
        timer = 0;
        spawnedTargets = 0;
    }

    public void StopMinigame()
    {
        minigameActive = false;
    }

    private void SpawnTarget()
    {
        Transform centerEye = Camera.main.transform;

        Vector3 forward = centerEye.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = centerEye.right;

        float horizontalOffset = Random.Range(-0.8f, 0.8f);
        float verticalOffset = Random.Range(-0.2f, 0.3f);

        Vector3 spawnPosition =
            centerEye.position +
            forward * spawnDistance +
            right * horizontalOffset +
            Vector3.up * verticalOffset;

        GameObject target = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);

        MoveToPlayer moveScript = target.GetComponent<MoveToPlayer>();
        if (moveScript != null)
        {
            moveScript.SetTarget(Camera.main.transform);
        }

        spawnedTargets++;
    }
}
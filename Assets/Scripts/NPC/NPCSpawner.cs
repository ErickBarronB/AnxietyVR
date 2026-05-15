using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private NPCPool pool;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] targets;

    [SerializeField] private float minSpawnTime = 1f;
    [SerializeField] private float maxSpawnTime = 3f;

    void Start()
    {
        ScheduleNextSpawn();
    }

    void ScheduleNextSpawn()
    {
        float randomTime = Random.Range(minSpawnTime, maxSpawnTime);
        Invoke(nameof(SpawnNPC), randomTime);
    }

    void SpawnNPC()
    {
        GameObject npc = pool.GetNPC();

        if (npc != null)
        {
            int index = Random.Range(0, spawnPoints.Length);

            Transform spawn = spawnPoints[index];
            Transform target = targets[index];

            npc.transform.position = spawn.position;
            npc.transform.rotation = Quaternion.identity;

            npc.GetComponent<NPCMovement>().SetTarget(target.position);
        }

        ScheduleNextSpawn();
    }
}
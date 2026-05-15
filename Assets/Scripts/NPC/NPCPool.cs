using System.Collections.Generic;
using UnityEngine;

public class NPCPool : MonoBehaviour
{
    [SerializeField] private GameObject[] npcPrefabs;
    [Tooltip("How many of each prefab type to instantiate at the start")]
    [SerializeField] private int initialPoolSizePerPrefab = 10;
    
    private Dictionary<string, List<GameObject>> pools = new Dictionary<string, List<GameObject>>();
    private Dictionary<string, float> typeWeights = new Dictionary<string, float>();
    private Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();

    void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        foreach (GameObject prefab in npcPrefabs)
        {
            string type = GetNPCType(prefab);
            
            if (!pools.ContainsKey(type))
            {
                pools[type] = new List<GameObject>();
                typeWeights[type] = 1f;
                prefabMap[type] = prefab;
                
                for (int i = 0; i < initialPoolSizePerPrefab; i++)
                {
                    CreateNewNPC(type);
                }
            }
            else
            {
                Debug.LogWarning($"[NPCPool] Duplicate NPC Type found: {type}. Ignoring prefab {prefab.name}. Make sure NPCIdentifiers are unique.");
            }
        }
    }

    private string GetNPCType(GameObject go)
    {
        NPCIdentifier identifier = go.GetComponent<NPCIdentifier>();
        return identifier != null ? identifier.npcType : go.name;
    }

    private GameObject CreateNewNPC(string type)
    {
        GameObject prefab = prefabMap[type];
        GameObject npc = Instantiate(prefab, transform);
        npc.SetActive(false);
        pools[type].Add(npc);
        return npc;
    }

    public GameObject GetNPC()
    {
        if (pools.Count == 0) return null;

        string selectedType = SelectTypeByWeight();
        
        foreach (GameObject npc in pools[selectedType])
        {
            if (!npc.activeInHierarchy)
            {
                npc.SetActive(true);
                return npc;
            }
        }

        GameObject newNpc = CreateNewNPC(selectedType);
        newNpc.SetActive(true);
        return newNpc;
    }

    private string SelectTypeByWeight()
    {
        float totalWeight = 0f;
        foreach (var weight in typeWeights.Values)
        {
            totalWeight += weight;
        }

        float randomVal = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        foreach (var kvp in typeWeights)
        {
            currentSum += kvp.Value;
            if (randomVal <= currentSum)
            {
                return kvp.Key;
            }
        }

        foreach (var key in typeWeights.Keys) return key; 
        return null;
    }

    public void IncreaseWeight(string npcType, float amount, float maxWeight)
    {
        if (typeWeights.ContainsKey(npcType))
        {
            typeWeights[npcType] += amount;
            if (typeWeights[npcType] > maxWeight)
            {
                typeWeights[npcType] = maxWeight;
            }
        }
    }

    public void DecayWeights(float decayAmount, float minWeight = 1f)
    {
        List<string> keys = new List<string>(typeWeights.Keys);
        foreach (string key in keys)
        {
            if (typeWeights[key] > minWeight)
            {
                typeWeights[key] -= decayAmount;
                if (typeWeights[key] < minWeight)
                {
                    typeWeights[key] = minWeight;
                }
            }
        }
    }
}
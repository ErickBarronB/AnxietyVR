using UnityEngine;

public class VisualFixationSystem : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The camera used for the raycast (e.g. VR Headset or Main Camera).")]
    public Transform vrCamera;
    public System_PlayerAnxiety anxietySystem;
    public NPCPool npcPool;

    [Header("Raycast Settings")]
    public LayerMask npcLayer;
    public float rayDistance = 50f;
    [Tooltip("El grosor del rayo. A mayor radio, más fácil será 'mirar' a un NPC sin tener que apuntarle con precisión milimétrica.")]
    public float rayRadius = 1.5f;

    [Header("Weight Settings")]
    [Tooltip("How much weight is added per second while looking at the NPC under anxiety.")]
    public float weightIncreaseRate = 5f;
    [Tooltip("The maximum spawn weight an NPC type can reach.")]
    public float maxWeight = 20f;
    [Tooltip("How much weight decays per second for all NPCs.")]
    public float weightDecayRate = 1f;

    [Header("Debug")]
    [Tooltip("Enable to see the raycast in Scene view.")]
    public bool showDebugs = true;

    [Header("Current State")]
    [Tooltip("Shows the NPC type currently being looked at.")]
    [SerializeField] private string currentlyObservedNPC = "None";

    private void Start()
    {
        if (vrCamera == null && Camera.main != null)
        {
            vrCamera = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (npcPool != null)
        {
            npcPool.DecayWeights(weightDecayRate * Time.deltaTime, 1f);
        }

        PerformRaycast();
    }

    private void PerformRaycast()
    {
        if (vrCamera == null || npcPool == null) return;

        if (Physics.SphereCast(vrCamera.position, rayRadius, vrCamera.forward, out RaycastHit hit, rayDistance, npcLayer))
        {
            if (showDebugs) Debug.DrawLine(vrCamera.position, hit.point, Color.green);

            NPCIdentifier identifier = hit.collider.GetComponentInParent<NPCIdentifier>();
            string targetName = identifier != null ? identifier.npcType : hit.collider.gameObject.name.Replace("(Clone)", "").Trim();
            
            currentlyObservedNPC = targetName;

            if (anxietySystem != null && anxietySystem.GetIsAnxious())
            {
                npcPool.IncreaseWeight(targetName, weightIncreaseRate * Time.deltaTime, maxWeight);
            }
        }
        else
        {
            currentlyObservedNPC = "None";
            if (showDebugs) Debug.DrawRay(vrCamera.position, vrCamera.forward * rayDistance, Color.red);
        }
    }
}

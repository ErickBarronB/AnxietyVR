using UnityEngine;

public class NPCIdentifier : MonoBehaviour
{
    [Tooltip("Unique identifier for this type of NPC (e.g. 'Stalker', 'Civilian'). Used by the Visual Fixation System to adjust spawn probabilities.")]
    public string npcType = "DefaultNPC";
}

using UnityEngine;

public class NPCDistanceDisable : MonoBehaviour
{
    public Transform player;
    public float disableDistance = 40f;

    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > disableDistance)
            gameObject.SetActive(false);
    }
}
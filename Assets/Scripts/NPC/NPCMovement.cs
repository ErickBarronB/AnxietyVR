using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float speed = 2f;
    private Vector3 target;

    public float arriveDistance = 0.1f;

    public void SetTarget(Vector3 newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        Vector3 dir = (target - transform.position);

        if (dir.magnitude < arriveDistance)
        {
            gameObject.SetActive(false);
            return;
        }

        dir.Normalize();

        transform.position += dir * speed * Time.deltaTime;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 5f * Time.deltaTime);
        }
    }
}
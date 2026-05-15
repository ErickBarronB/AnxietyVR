using UnityEngine;

public class MoveToPlayer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float stopDistance = 0.4f;

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > stopDistance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        transform.LookAt(target);
    }
}
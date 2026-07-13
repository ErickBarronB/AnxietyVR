using UnityEngine;

public class FloatingArrow : MonoBehaviour
{

    [SerializeField] private float distance = 0.25f;
    [SerializeField] private float speed = 2f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * distance;

        transform.localPosition = startPosition + Vector3.up * offset;
    }
}
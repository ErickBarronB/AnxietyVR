using UnityEngine;

public class WatchZoom : MonoBehaviour
{
    private Vector3 normalScale;
    [SerializeField] private Vector3 zoomScale = Vector3.one * 2f;
    [SerializeField] private float speed = 5f;

    private bool zoomed = false;

    private void Start()
    {
        normalScale = transform.localScale;
    }

    public void ToggleZoom()
    {
        zoomed = !zoomed;
    }

    private void Update()
    {
        Vector3 target = zoomed ? zoomScale : normalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * speed);
    }
}

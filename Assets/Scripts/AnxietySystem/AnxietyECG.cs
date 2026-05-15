using UnityEngine;

public class AnxietyECG : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private int resolution = 120;
    [SerializeField] private float baseSpeed = 1.5f;
    [SerializeField] private float xSpacing = 0.03f;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private float verticalOffset = -0.4f;

    [SerializeField] private System_PlayerAnxiety anxietySystem;

    private float time;
    private Vector3[] positions;
    private float width;
    private float height;

    void Start()
    {
        positions = new Vector3[resolution];
        width = canvasRect.rect.width;
        height = canvasRect.rect.height;
    }

    void Update()
    {
        if (anxietySystem == null || line == null) return;

        float anxiety = anxietySystem.GetAnxiety() / 180f;

        float dynamicSpeed = baseSpeed + anxiety * 3f;
        time += Time.deltaTime * dynamicSpeed;

        line.positionCount = resolution;

        for (int i = 0; i < resolution; i++)
        {
            float normalizedX = (float)i / (resolution - 1);
            float x = normalizedX * width;

            float t = (normalizedX + time) % 1f;

            float y = GenerateECG(t, anxiety);

            float maxHeight = height * 0.45f;

            float normalizedY = y / 3.2f;

            float scaledY = normalizedY * maxHeight;

            positions[i] = new Vector3(x, scaledY + verticalOffset, -0.1f);
        }
        line.SetPositions(positions);
    }

    float GenerateECG(float t, float anxiety)
    {
        float y = 0;

        float peak = 1.2f + anxiety * 2f;

        if (t < 0.05f)
        {
            y = Mathf.Lerp(0, peak, t / 0.05f); 
        }
        else if (t < 0.1f)
        {
            y = Mathf.Lerp(peak, -0.6f, (t - 0.05f) / 0.05f); 
        }
        else if (t < 0.2f)
        {
            y = Mathf.Lerp(-0.6f, 0, (t - 0.1f) / 0.1f);
        }
        else
        {
            y = Mathf.Sin(t * 25f) * 0.03f;
        }

        return y;
    }
}

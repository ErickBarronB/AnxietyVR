using System.Text;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    private const float UpdateInterval = 0.3f;
    private const float DistanceFromCamera = 1.5f;
    private const float VerticalOffset = 0.35f;
    private const float HorizontalOffset = -0.55f;

    private TMP_Text label;
    private readonly StringBuilder sb = new StringBuilder(16);

    private float timer;
    private int frameCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<FPSCounter>() != null) return;

        GameObject root = new GameObject("FPSCounter");
        DontDestroyOnLoad(root);
        root.AddComponent<FPSCounter>();
    }

    private void Awake()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        GameObject canvasGO = new GameObject("Canvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(300, 80);
        canvasRect.localScale = Vector3.one * 0.001f;

        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(canvasGO.transform, false);

        label = textGO.AddComponent<TextMeshProUGUI>();
        label.fontSize = 48;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.green;

        RectTransform textRect = label.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private void LateUpdate()
    {
        FollowCamera();
        UpdateCounter();
    }

    private void FollowCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Transform camT = cam.transform;
        transform.SetPositionAndRotation(
            camT.position
                + camT.forward * DistanceFromCamera
                + camT.up * VerticalOffset
                + camT.right * HorizontalOffset,
            camT.rotation);
    }

    private void UpdateCounter()
    {
        frameCount++;
        timer += Time.unscaledDeltaTime;
        if (timer < UpdateInterval) return;

        float fps = ComputeFps();
        float referenceFps = ReferenceFramerate();

        label.color = fps >= referenceFps * 0.9f ? Color.green
            : fps >= referenceFps * 0.6f ? Color.yellow
            : Color.red;

        sb.Clear();
        sb.Append((int)fps).Append(" FPS");
        label.SetText(sb);

        timer = 0f;
        frameCount = 0;
    }

    private float ComputeFps()
    {
        if (OVRManager.display != null && OVRManager.display.appFramerate > 0f)
            return OVRManager.display.appFramerate;

        return timer > 0f ? frameCount / timer : 0f;
    }

    private float ReferenceFramerate()
    {
        if (OVRManager.display != null && OVRManager.display.displayFrequency > 0f)
            return OVRManager.display.displayFrequency;

        return 72f;
    }
}

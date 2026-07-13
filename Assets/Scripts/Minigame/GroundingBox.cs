using UnityEngine;
using TMPro;
using Oculus.Interaction;

public class GroundingBox : MonoBehaviour
{
    [SerializeField] private SensoryCategory category;
    [SerializeField] private int capacity;
    [SerializeField] private TMP_Text countLabel;

    private GroundingMinigame minigame;
    private int count;
    private SnapInteractable snapInteractable;

    public SensoryCategory Category => category;
    public bool IsFull => count >= capacity;
    public SnapInteractable SnapInteractable => snapInteractable;

    public void Init(GroundingMinigame game)
    {
        minigame = game;
        count = 0;
        RefreshLabel();
        EnsureSnapInteractable();
    }

    private void EnsureSnapInteractable()
    {
        if (snapInteractable != null) return;

        snapInteractable = GetComponent<SnapInteractable>();
        if (snapInteractable == null)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            snapInteractable = gameObject.AddComponent<SnapInteractable>();
            snapInteractable.InjectRigidbody(rb);
        }
    }

    public bool TryDeposit(GroundingObject obj)
    {
        if (obj.Category != category || IsFull)
        {
            minigame?.OnWrongDeposit();
            return false;
        }

        count++;
        RefreshLabel();
        minigame?.OnCorrectDeposit();
        return true;
    }

    private void RefreshLabel()
    {
        if (countLabel != null)
            countLabel.text = $"{count}/{capacity}";
    }
}

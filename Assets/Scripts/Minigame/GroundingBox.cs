using UnityEngine;
using TMPro;

public class GroundingBox : MonoBehaviour
{
    [SerializeField] private SensoryCategory category;
    [SerializeField] private int capacity;
    [SerializeField] private TMP_Text countLabel;

    private GroundingMinigame minigame;
    private int count;

    public SensoryCategory Category => category;
    public bool IsFull => count >= capacity;

    public void Init(GroundingMinigame game)
    {
        minigame = game;
        count = 0;
        RefreshLabel();
    }

    // Detecta si el objeto físicamente entra al trigger de la caja
    private void OnTriggerEnter(Collider other)
    {
        GroundingObject obj = other.GetComponent<GroundingObject>();
        if (obj == null || obj.IsDone) return;
        obj.DepositIntoBox(this);
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

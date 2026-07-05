using System.Collections;
using UnityEngine;
using Oculus.Interaction;

public class GroundingObject : MonoBehaviour
{
    [SerializeField] private SensoryCategory category;

    private GroundingMinigame minigame;
    private Grabbable grabbable;
    private bool done;

    public SensoryCategory Category => category;
    public bool IsDone => done;

    public void Init(GroundingMinigame game, SensoryCategory cat)
    {
        minigame = game;
        category = cat;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void Start()
    {
        grabbable = GetComponent<Grabbable>();
        if (grabbable != null)
            grabbable.WhenPointerEventRaised += OnPointerEvent;
    }

    private void OnDestroy()
    {
        if (grabbable != null)
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Unselect)
            OnReleased();
    }

    private void OnReleased()
    {
        if (done) return;

        Collider[] cols = Physics.OverlapSphere(transform.position, 0.4f, ~0, QueryTriggerInteraction.Collide);
        foreach (var col in cols)
        {
            GroundingBox box = col.GetComponent<GroundingBox>();
            if (box != null)
            {
                DepositIntoBox(box);
                return;
            }
        }

        StartCoroutine(FreezeNextFrame());
    }

    private IEnumerator FreezeNextFrame()
    {
        yield return null;
        if (done) yield break;
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    public void DepositIntoBox(GroundingBox box)
    {
        if (done) return;
        done = true;
        box.TryDeposit(this);
        Destroy(gameObject);
    }
}

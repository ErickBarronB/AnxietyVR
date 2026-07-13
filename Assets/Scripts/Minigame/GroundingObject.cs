using System.Collections;
using UnityEngine;
using Oculus.Interaction;

public class GroundingObject : MonoBehaviour
{
    [SerializeField] private SensoryCategory category;

    private GroundingMinigame minigame;
    private Grabbable grabbable;
    private bool done;
    private GameObject originalPrefab;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    public SensoryCategory Category => category;
    public bool IsDone => done;

    public void Init(GroundingMinigame game, SensoryCategory cat, GameObject prefab)
    {
        minigame = game;
        category = cat;
        originalPrefab = prefab;

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

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

        bool correct = box.TryDeposit(this);

        if (correct)
        {
            done = true;
            Destroy(gameObject);
        }
        else
        {
            GameObject go = Instantiate(originalPrefab, spawnPosition, spawnRotation);

            GroundingObject obj = go.GetComponent<GroundingObject>();
            obj.Init(minigame, category, originalPrefab);
            minigame.RegisterObject(obj);

            Destroy(gameObject);
        }
    }
}

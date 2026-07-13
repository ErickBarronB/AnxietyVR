using System.Collections;
using UnityEngine;
using Oculus.Interaction;

public class GroundingObject : MonoBehaviour
{
    private const float SnapResolveDelay = 0.15f;

    [SerializeField] private SensoryCategory category;

    private GroundingMinigame minigame;
    private Grabbable grabbable;
    private Rigidbody rb;
    private SnapInteractor snapInteractor;
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

        rb = GetComponent<Rigidbody>();
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

        EnsureSnapInteractor();
    }

    private void EnsureSnapInteractor()
    {
        if (grabbable == null) return;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        snapInteractor = GetComponent<SnapInteractor>();
        if (snapInteractor == null)
            snapInteractor = gameObject.AddComponent<SnapInteractor>();

        snapInteractor.InjectPointableElement(grabbable);
        snapInteractor.InjectRigidbody(rb);
        snapInteractor.WhenInteractableSelected.Action += HandleSnapSelected;
    }

    private void OnDestroy()
    {
        if (grabbable != null)
            grabbable.WhenPointerEventRaised -= OnPointerEvent;

        if (snapInteractor != null)
            snapInteractor.WhenInteractableSelected.Action -= HandleSnapSelected;
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Unselect)
            OnReleased();
    }

    private void OnReleased()
    {
        if (done) return;
        StartCoroutine(FreezeNextFrame());
    }

    private void HandleSnapSelected(SnapInteractable interactable)
    {
        if (done || interactable == null) return;

        GroundingBox box = interactable.GetComponent<GroundingBox>();
        if (box == null) return;

        StartCoroutine(ResolveDepositAfterSnap(box, interactable));
    }

    private IEnumerator ResolveDepositAfterSnap(GroundingBox box, SnapInteractable interactable)
    {
        yield return new WaitForSeconds(SnapResolveDelay);

        if (done) yield break;
        if (snapInteractor == null || snapInteractor.SelectedInteractable != interactable) yield break;

        DepositIntoBox(box);
    }

    private IEnumerator FreezeNextFrame()
    {
        yield return null;
        if (done) yield break;
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
        done = true;

        if (correct)
        {
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

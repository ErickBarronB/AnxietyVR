using UnityEngine;
using UnityEngine.Events;

namespace Minigame
{
    [RequireComponent(typeof(Collider))]
    public class ChaseCubeMinigame : MonoBehaviour
    {
        public enum InteractionMode
        {
            HandCollision,
            CameraRaycast
        }

        [System.Serializable]
        public class WaypointReachedEvent : UnityEvent<int> { }

        [Header("Modo de Interacción")]
        [SerializeField] private InteractionMode interactionMode = InteractionMode.HandCollision;

        [Header("Waypoints & Spawn")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Transform spawnPoint;

        [Header("Configuraciones de Movimiento")]
        [SerializeField] private float moveSpeed = 1.8f;
        [SerializeField] private float backwardSpeed = 1.2f;
        [SerializeField] private float waypointThreshold = 0.05f;

        [Header("Modo: Contacto de Mano")]
        [SerializeField] private string handTag = "Hand";

        [Header("Modo: Raycast de Cámara")]
        [SerializeField] private Transform vrCamera;
        [SerializeField] private float raycastRadius = 0.5f;
        [SerializeField] private float raycastDistance = 50f;
        [SerializeField] private LayerMask raycastLayers = ~0;
        [SerializeField] private bool showRaycastDebug = true;

        [Header("Sistema de Ansiedad")]
        [SerializeField] private float anxietyReductionAmount = 30f;

        [Header("Bucle y Flujo de Juego")]
        [SerializeField] private bool startOnStart = false;
        [SerializeField] private bool resetOnComplete = true;

        [Header("Eventos de Unity")]
        public UnityEvent onMinigameStarted;
        public UnityEvent onMinigameCompleted;
        public UnityEvent onHandContactLost;
        public UnityEvent onHandContactRegained;
        public WaypointReachedEvent onWaypointReached;

        private System_PlayerAnxiety anxietySystem;
        private Collider myCollider;
        private bool minigameActive = false;
        private bool isHandTouching = false;
        private bool handContactThisFrame = false;
        private int currentWaypointIndex = 0;
        private bool isWaitingAtWaypoint = false;
        private Quaternion startRotation;

        public System.Func<bool> shouldWaitCallback;

        public InteractionMode CurrentInteractionMode => interactionMode;
        public bool IsMinigameActive => minigameActive;
        public bool IsHandTouching => isHandTouching;
        public int CurrentWaypointIndex => currentWaypointIndex;
        public bool IsWaitingAtWaypoint => isWaitingAtWaypoint;

        private void Start()
        {
            myCollider = GetComponent<Collider>();
            anxietySystem = FindObjectOfType<System_PlayerAnxiety>();
            startRotation = transform.rotation;

            if (interactionMode == InteractionMode.CameraRaycast && vrCamera == null && Camera.main != null)
            {
                vrCamera = Camera.main.transform;
            }

            if (startOnStart)
            {
                StartMinigame();
            }
        }

        private void Update()
        {
            if (!minigameActive) return;

            if (waypoints == null || waypoints.Length < 2)
            {
                Debug.LogWarning("[ChaseCubeMinigame] Se necesitan al menos 2 waypoints.");
                minigameActive = false;
                return;
            }

            if (interactionMode == InteractionMode.CameraRaycast)
            {
                PerformCameraRaycast();
            }

            UpdateHandContactState();

            if (isHandTouching)
            {
                MoveForward();
            }
            else
            {
                MoveBackward();
            }

            // Mantener siempre la rotación inicial
            transform.rotation = startRotation;

            handContactThisFrame = false;
        }

        public void StartMinigame()
        {
            if (waypoints == null || waypoints.Length < 2)
            {
                Debug.LogError("[ChaseCubeMinigame] Faltan waypoints para iniciar.");
                return;
            }

            minigameActive = true;
            currentWaypointIndex = 1;
            isHandTouching = false;
            handContactThisFrame = false;
            isWaitingAtWaypoint = false;

            Transform startTransform = spawnPoint != null ? spawnPoint : waypoints[0];
            transform.position = startTransform.position;
            startRotation = startTransform.rotation;
            transform.rotation = startRotation;

            gameObject.SetActive(true);
            onMinigameStarted?.Invoke();
        }

        public void StopMinigame()
        {
            minigameActive = false;
        }

        private void PerformCameraRaycast()
        {
            if (vrCamera == null)
            {
                if (Camera.main != null)
                {
                    vrCamera = Camera.main.transform;
                }
                else
                {
                    return;
                }
            }

            RaycastHit hit;
            bool hitDetected = false;

            if (raycastRadius > 0f)
            {
                hitDetected = Physics.SphereCast(vrCamera.position, raycastRadius, vrCamera.forward, out hit, raycastDistance, raycastLayers);
            }
            else
            {
                hitDetected = Physics.Raycast(vrCamera.position, vrCamera.forward, out hit, raycastDistance, raycastLayers);
            }

            if (hitDetected)
            {
                if (hit.collider == myCollider || hit.transform.IsChildOf(transform))
                {
                    handContactThisFrame = true;

                    if (showRaycastDebug)
                    {
                        Debug.DrawLine(vrCamera.position, hit.point, Color.green);
                    }
                }
                else
                {
                    if (showRaycastDebug)
                    {
                        Debug.DrawLine(vrCamera.position, hit.point, Color.red);
                    }
                }
            }
            else
            {
                if (showRaycastDebug)
                {
                    Debug.DrawRay(vrCamera.position, vrCamera.forward * raycastDistance, Color.red);
                }
            }
        }

        private void UpdateHandContactState()
        {
            bool previouslyTouching = isHandTouching;
            isHandTouching = handContactThisFrame;

            if (isHandTouching != previouslyTouching)
            {
                if (isHandTouching)
                {
                    onHandContactRegained?.Invoke();
                }
                else
                {
                    onHandContactLost?.Invoke();
                }
            }
        }

        private void MoveForward()
        {
            if (isWaitingAtWaypoint) return;

            Vector3 targetPosition = waypoints[currentWaypointIndex].position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < waypointThreshold)
            {
                if (shouldWaitCallback != null && shouldWaitCallback())
                {
                    isWaitingAtWaypoint = true;
                    onWaypointReached?.Invoke(currentWaypointIndex);
                }
                else
                {
                    onWaypointReached?.Invoke(currentWaypointIndex);
                    ProceedToNextWaypoint();
                }
            }
        }

        private void ProceedToNextWaypoint()
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                CompleteMinigame();
            }
        }

        public void ResumeMovement()
        {
            if (isWaitingAtWaypoint)
            {
                isWaitingAtWaypoint = false;
                ProceedToNextWaypoint();
            }
        }

        private void MoveBackward()
        {
            int targetIndex = currentWaypointIndex - 1;
            Vector3 targetPosition = waypoints[targetIndex].position;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, backwardSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < waypointThreshold)
            {
                if (targetIndex > 0)
                {
                    currentWaypointIndex--;
                }
            }
        }

        private void CompleteMinigame()
        {
            minigameActive = false;

            if (anxietySystem != null)
            {
                anxietySystem.RemoveAnxiety(anxietyReductionAmount);
            }

            onMinigameCompleted?.Invoke();

            if (resetOnComplete)
            {
                Transform startTransform = spawnPoint != null ? spawnPoint : waypoints[0];
                transform.position = startTransform.position;
                transform.rotation = startTransform.rotation;
                currentWaypointIndex = 0;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (interactionMode == InteractionMode.HandCollision)
            {
                CheckHandContact(other);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (interactionMode == InteractionMode.HandCollision)
            {
                CheckHandContact(collision.collider);
            }
        }

        private void CheckHandContact(Collider other)
        {
            if (other.CompareTag(handTag))
            {
                handContactThisFrame = true;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;

                Gizmos.color = (i == 0) ? Color.green : (i == waypoints.Length - 1) ? Color.red : Color.cyan;
                Gizmos.DrawSphere(waypoints[i].position, 0.12f);

                UnityEditor.Handles.Label(waypoints[i].position + Vector3.up * 0.22f, $"Punto {i}");

                if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);

                    Vector3 midPoint = (waypoints[i].position + waypoints[i + 1].position) * 0.5f;
                    Vector3 direction = (waypoints[i + 1].position - waypoints[i].position).normalized;
                    
                    if (direction != Vector3.zero)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawRay(midPoint - direction * 0.08f, direction * 0.16f);
                        Vector3 rightDir = Vector3.Cross(direction, Vector3.up).normalized;
                        Gizmos.DrawLine(midPoint + direction * 0.08f, midPoint - direction * 0.08f + rightDir * 0.04f);
                        Gizmos.DrawLine(midPoint + direction * 0.08f, midPoint - direction * 0.08f - rightDir * 0.04f);
                    }
                }
            }
        }
#endif
    }
}

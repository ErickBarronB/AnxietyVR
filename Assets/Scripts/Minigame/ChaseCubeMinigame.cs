using UnityEngine;
using UnityEngine.Events;

namespace Minigame
{
    public class ChaseCubeMinigame : MonoBehaviour
    {
        [System.Serializable]
        public class WaypointReachedEvent : UnityEvent<int> { }

        [Header("Waypoints & Spawn")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Transform spawnPoint;

        [Header("Referencias del Globo")]
        [SerializeField] private GameObject balloon;

        [Header("Pinchos")]
        [SerializeField] private SpikeWaypointMover[] spikes;

        [Header("Constraint de Movimiento del Globo")]
        [SerializeField] private bool lockXAxis = true;
        [SerializeField] private bool lockYAxis = false;
        [SerializeField] private bool lockZAxis = false;

        [Header("Detección de Waypoints del Globo")]
        [SerializeField] private float waypointThreshold = 0.5f;

        [Header("Sistema de Ansiedad")]
        [SerializeField] private float anxietyReductionAmount = 30f;

        [Header("Bucle y Flujo de Juego")]
        [SerializeField] private bool startOnStart = false;

        [Header("Eventos de Unity")]
        public UnityEvent onMinigameStarted;
        public UnityEvent onMinigameCompleted;
        public WaypointReachedEvent onWaypointReached;

        private System_PlayerAnxiety anxietySystem;
        private bool minigameActive = false;
        private int currentWaypointIndex = 0;
        private Vector3 lockedPosition;
        private float lastResetTime = -1f;

        private bool prevLockX = false;
        private bool prevLockY = false;
        private bool prevLockZ = false;

        public bool IsMinigameActive => minigameActive;
        public int CurrentWaypointIndex => currentWaypointIndex;

        private void Start()
        {
            anxietySystem = FindObjectOfType<System_PlayerAnxiety>();
            
            Transform startTransform = spawnPoint != null ? spawnPoint : (waypoints != null && waypoints.Length > 0 ? waypoints[0] : null);
            if (startTransform != null)
            {
                lockedPosition = startTransform.position;
            }

            if (balloon != null)
            {
                var detector = balloon.GetComponent<BalloonCollisionDetector>();
                if (detector == null)
                {
                    detector = balloon.AddComponent<BalloonCollisionDetector>();
                }
                detector.OnHitSpike = ResetBalloon;
            }

            if (spikes != null)
            {
                foreach (var spike in spikes)
                {
                    if (spike != null)
                    {
                        spike.onHitBalloon.AddListener(ResetBalloon);
                    }
                }
            }

            if (startOnStart)
                StartMinigame();
        }

        private void Update()
        {
            if (!minigameActive) return;
            CheckBalloonWaypointProximity();
        }

        private void LateUpdate()
        {
            if (!minigameActive || balloon == null) return;

            UpdateAxisLocksBasedOnSpikes();

            Vector3 currentPos = balloon.transform.position;
            
            if (lockXAxis && !prevLockX) lockedPosition.x = currentPos.x;
            if (lockYAxis && !prevLockY) lockedPosition.y = currentPos.y;
            if (lockZAxis && !prevLockZ) lockedPosition.z = currentPos.z;

            prevLockX = lockXAxis;
            prevLockY = lockYAxis;
            prevLockZ = lockZAxis;

            Vector3 pos = currentPos;
            if (lockXAxis) pos.x = lockedPosition.x;
            if (lockYAxis) pos.y = lockedPosition.y;
            if (lockZAxis) pos.z = lockedPosition.z;
            balloon.transform.position = pos;
        }

        private void UpdateAxisLocksBasedOnSpikes()
        {
            if (spikes == null || spikes.Length == 0) return;

            foreach (var spike in spikes)
            {
                if (spike == null || !spike.IsActive) continue;

                Vector3 direction = spike.GetMovementDirection();
                if (direction == Vector3.zero) continue;

                float absX = Mathf.Abs(direction.x);
                float absY = Mathf.Abs(direction.y);
                float absZ = Mathf.Abs(direction.z);

                if (absX > absY && absX > absZ)
                {
                    lockXAxis = false;
                    lockYAxis = true;
                    lockZAxis = true;
                    return;
                }
                else if (absZ > absX && absZ > absY)
                {
                    lockXAxis = true;
                    lockYAxis = true;
                    lockZAxis = false;
                    return;
                }
                else if (absY > absX && absY > absZ)
                {
                    lockXAxis = true;
                    lockYAxis = false;
                    lockZAxis = true;
                    return;
                }
            }
        }

        private void CheckBalloonWaypointProximity()
        {
            if (balloon == null) return;
            if (waypoints == null || currentWaypointIndex >= waypoints.Length) return;

            Transform target = waypoints[currentWaypointIndex];
            if (target == null) return;

            if (Vector3.Distance(balloon.transform.position, target.position) < waypointThreshold)
            {
                onWaypointReached?.Invoke(currentWaypointIndex);
                currentWaypointIndex++;

                if (currentWaypointIndex >= waypoints.Length)
                    CompleteMinigame();
            }
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

            if (balloon != null)
            {
                Transform startTransform = spawnPoint != null ? spawnPoint : waypoints[0];
                
                balloon.SetActive(false);

                Rigidbody rb = balloon.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.position = startTransform.position;
                }

                balloon.transform.position = startTransform.position;
                balloon.transform.rotation = startTransform.rotation;
                lockedPosition = startTransform.position;

                prevLockX = false;
                prevLockY = false;
                prevLockZ = false;

                balloon.SetActive(true);
            }

            gameObject.SetActive(true);

            if (spikes != null)
                foreach (var spike in spikes)
                    spike?.StartMoving();

            onMinigameStarted?.Invoke();
        }

        public void StopMinigame()
        {
            minigameActive = false;

            if (spikes != null)
                foreach (var spike in spikes)
                    spike?.StopMoving();
        }

        public void ResetBalloon()
        {
            if (!minigameActive) return;

            if (Time.time - lastResetTime < 0.1f) return;
            lastResetTime = Time.time;

            currentWaypointIndex = 1;

            if (balloon != null)
            {
                Transform startTransform = spawnPoint != null ? spawnPoint : waypoints[0];

                balloon.SetActive(false);

                Rigidbody rb = balloon.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.position = startTransform.position;
                }

                balloon.transform.position = startTransform.position;
                balloon.transform.rotation = startTransform.rotation;
                lockedPosition = startTransform.position;

                prevLockX = false;
                prevLockY = false;
                prevLockZ = false;

                balloon.SetActive(true);
            }

            if (spikes != null)
            {
                foreach (var spike in spikes)
                {
                    if (spike != null)
                    {
                        spike.StartMoving();
                    }
                }
            }
        }

        private void CompleteMinigame()
        {
            minigameActive = false;

            if (spikes != null)
                foreach (var spike in spikes)
                    spike?.StopMoving();

            if (anxietySystem != null)
                anxietySystem.RemoveAnxiety(anxietyReductionAmount);

            onMinigameCompleted?.Invoke();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;

                Gizmos.color = i == 0 ? Color.green : i == waypoints.Length - 1 ? Color.red : Color.cyan;
                Gizmos.DrawSphere(waypoints[i].position, 0.15f);

                UnityEditor.Handles.Label(waypoints[i].position + Vector3.up * 0.25f, $"Punto {i}");

                if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
#endif
    }
}

using UnityEngine;
using UnityEngine.Events;

namespace Minigame
{
    [RequireComponent(typeof(Rigidbody))]
    public class SpikeWaypointMover : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 2f;
        [SerializeField] private float waypointThreshold = 0.1f;
        [SerializeField] private int startWaypointIndex = 0;
        [SerializeField] private bool startOnStart = true;

        [Header("Movimiento Temporal (4 seg por tramo)")]
        [SerializeField] private bool useTimeBased = false;
        [SerializeField] private float segmentDuration = 4f;

        [Header("Colisión con el Globo")]
        [SerializeField] private string balloonTag = "Balloon";
        public UnityEvent onHitBalloon;

        private int currentIndex;
        private bool active;
        private Rigidbody rb;
        private float segmentElapsed = 0f;
        private Vector3 segmentStartPos;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        private void Start()
        {
            if (startOnStart)
                StartMoving();
        }

        private void FixedUpdate()
        {
            if (!active || waypoints == null || waypoints.Length < 2) return;

            Transform target = waypoints[currentIndex];
            if (target == null) return;

            if (useTimeBased)
            {
                segmentElapsed += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(segmentElapsed / segmentDuration);
                rb.MovePosition(Vector3.Lerp(segmentStartPos, target.position, t));

                if (t >= 1f)
                {
                    segmentStartPos = target.position;
                    segmentElapsed = 0f;
                    currentIndex = (currentIndex + 1) % waypoints.Length;
                }
            }
            else
            {
                Vector3 newPos = Vector3.MoveTowards(rb.position, target.position, speed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);

                if (Vector3.Distance(rb.position, target.position) < waypointThreshold)
                    currentIndex = (currentIndex + 1) % waypoints.Length;
            }
        }

        public void StartMoving()
        {
            if (waypoints == null || waypoints.Length < 2)
            {
                Debug.LogWarning("[SpikeWaypointMover] Se necesitan al menos 2 waypoints.");
                return;
            }

            startWaypointIndex = Mathf.Clamp(startWaypointIndex, 0, waypoints.Length - 1);
            rb.position = waypoints[startWaypointIndex].position;
            currentIndex = (startWaypointIndex + 1) % waypoints.Length;
            segmentStartPos = waypoints[startWaypointIndex].position;
            segmentElapsed = 0f;
            active = true;
        }

        public void StopMoving()
        {
            active = false;
        }

        public bool IsActive => active;
        public Transform[] GetWaypoints() => waypoints;

        public Vector3 GetMovementDirection()
        {
            if (!active || waypoints == null || waypoints.Length < 2) return Vector3.zero;

            int nextIndex = currentIndex;
            int prevIndex = (currentIndex - 1 + waypoints.Length) % waypoints.Length;
            if (waypoints[nextIndex] != null && waypoints[prevIndex] != null)
                return (waypoints[nextIndex].position - waypoints[prevIndex].position).normalized;
            return (waypoints[1].position - waypoints[0].position).normalized;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!active) return;
            if (other.CompareTag(balloonTag))
                onHitBalloon?.Invoke();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            Gizmos.color = Color.red;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawWireSphere(waypoints[i].position, 0.15f);
                int next = (i + 1) % waypoints.Length;
                if (waypoints[next] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
            }
        }
#endif
    }
}

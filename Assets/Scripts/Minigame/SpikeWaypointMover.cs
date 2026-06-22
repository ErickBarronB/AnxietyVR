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

        [Header("Colisión con el Globo")]
        [SerializeField] private string balloonTag = "Balloon";
        public UnityEvent onHitBalloon;

        private int currentIndex;
        private bool active;
        private Rigidbody rb;

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

            Vector3 newPos = Vector3.MoveTowards(rb.position, target.position, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Vector3.Distance(rb.position, target.position) < waypointThreshold)
                currentIndex = (currentIndex + 1) % waypoints.Length;
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
            active = true;
        }

        public void StopMoving()
        {
            active = false;
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

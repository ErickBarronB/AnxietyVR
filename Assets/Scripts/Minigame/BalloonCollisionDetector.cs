using UnityEngine;
using System;

namespace Minigame
{
    public class BalloonCollisionDetector : MonoBehaviour
    {
        public Action OnHitSpike;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Spike") || other.GetComponent<SpikeWaypointMover>() != null)
            {
                OnHitSpike?.Invoke();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Spike") || collision.collider.GetComponent<SpikeWaypointMover>() != null)
            {
                OnHitSpike?.Invoke();
            }
        }
    }
}

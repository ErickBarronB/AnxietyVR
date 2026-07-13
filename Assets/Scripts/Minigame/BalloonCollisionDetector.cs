using UnityEngine;
using System;

namespace Minigame
{
    public class BalloonCollisionDetector : MonoBehaviour
    {
        public Action OnHitSpike;
        public Action<Collider> OnReachWaypoint;

        [Header("Sonido de Explosión")]
        [SerializeField] private AudioClip popSound;
        [SerializeField] [Range(0f, 1f)] private float popVolume = 1f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Spike") || other.GetComponent<SpikeWaypointMover>() != null)
            {
                PlayPopSound();
                OnHitSpike?.Invoke();
            }
            else if (other.CompareTag("WayPoint"))
            {
                OnReachWaypoint?.Invoke(other);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Spike") || collision.collider.GetComponent<SpikeWaypointMover>() != null)
            {
                PlayPopSound();
                OnHitSpike?.Invoke();
            }
        }

        private void PlayPopSound()
        {
            if (popSound != null)
                AudioSource.PlayClipAtPoint(popSound, transform.position, popVolume);
        }
    }
}

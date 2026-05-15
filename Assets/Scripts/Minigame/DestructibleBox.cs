using UnityEngine;

namespace Minigame
{
    /// <summary>
    /// Componente que se encarga de destruir la caja cuando entra en contacto con las manos del jugador (Meta XR).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DestructibleBox : MonoBehaviour
    {
        [Header("Configuración del Jugador")]
        [Tooltip("El tag que deben tener los colliders de la mano para destruir esta caja.")]
        public string handTag = "Hand";
        [SerializeField] private float anxietyReduction = 5f;

        [Header("Efectos Visuales")]
        [Tooltip("Prefab de partículas a instanciar cuando la caja es destruida (Opcional).")]
        public GameObject destructionParticlesPrefab;

        private System_PlayerAnxiety anxietySystem;

        private void Start()
        {
            anxietySystem = FindObjectOfType<System_PlayerAnxiety>();
        }

        private void OnTriggerEnter(Collider other)
        {
            // Comprobamos si el objeto que entró en el trigger tiene el tag asignado para las manos.
            if (other.CompareTag(handTag))
            {
                DestroyBox();
            }
        }

        private void DestroyBox()
        {
            if (anxietySystem != null)
            {
                anxietySystem.TriggerCalm(2f);
            }

            // Si hay un sistema de partículas asignado, instanciarlo.
            if (destructionParticlesPrefab != null)
            {
                Instantiate(destructionParticlesPrefab, transform.position, transform.rotation);
            }

            AutoDestroyTarget autoDestroy = GetComponent<AutoDestroyTarget>();
            if (autoDestroy != null)
            {
                autoDestroy.DestroyedByPlayer();
            }

            // Destruir este objeto.
            Destroy(gameObject);
        }
    }
}

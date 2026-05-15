using UnityEngine;

public class AutoDestroyTarget : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;
    //[SerializeField] private AudioClip missedSound;

    private bool wasDestroyedByPlayer = false;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void DestroyedByPlayer()
    {
        wasDestroyedByPlayer = true;
    }

    private void OnDestroy()
    {
        if (!wasDestroyedByPlayer && gameObject.scene.isLoaded)
        {
            //PlayMissedSound();
        }
    }

    //private void PlayMissedSound()
    //{
    //    if (missedSound != null)
    //    {
    //        AudioSource.PlayClipAtPoint(
    //            missedSound,
    //            transform.position
    //        );
    //    }
    //}
}
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour
{
    public AudioClip hoverSound;
    public AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Optional: Configure AudioSource settings
        audioSource.playOnAwake = false; // Don't play automatically
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerExit()
    {
        audioSource.pitch = 0.8f;
        audioSource.PlayOneShot(hoverSound, 1);
    }
}

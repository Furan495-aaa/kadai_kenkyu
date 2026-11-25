using System.Collections;
using UnityEngine;

public class DelayedOneShotSE : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1.5f)] private float volumeScale = 1f;
    [SerializeField, Min(0f)] private float delaySeconds = 0f;

    public void Play()
    {
        if (audioSource == null || clip == null) return;

        if (delaySeconds > 0f)
        {
            StartCoroutine(PlayRoutine());
        }
        else
        {
            audioSource.PlayOneShot(clip, volumeScale);
        }
    }

    private IEnumerator PlayRoutine()
    {
        yield return new WaitForSeconds(delaySeconds);
        audioSource.PlayOneShot(clip, volumeScale);
    }
}
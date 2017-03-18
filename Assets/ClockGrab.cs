using System.Collections;
using UnityEngine;
using VRTK;

public class ClockGrab : VRTK_InteractableObject
{
    // SoundsFX
    public AudioClip returnHomeAudioClip;
    public new AudioSource audio;
    public Transform home;

    Vector3 resetPosition;
    Quaternion resetRotation;
    
    private void Start()
    {
        resetPosition = home.position;
        resetRotation = home.rotation;
    }

    public override void SaveCurrentState()
    {
        // TweenGrabAttach handles kinematics, (lie to the script, we'll handle kinematics)
        interactableRigidbody.isKinematic = false;

        base.SaveCurrentState();
    }

    public void ReturnToHome()
    {
        StartCoroutine(TweenToReset(transform, 0.75f));
        audio.PlayOneShot(returnHomeAudioClip, 0.95f);
    }

    /// <summary>
    /// Tweens position and rotation
    /// </summary>
    /// <param name="thisTransform">Transform to animate</param>
    /// <param name="animationTime">Time animation will take to execute</param>
    public IEnumerator TweenToReset(Transform thisTransform, float animationTime)
    {
        Vector3 startPosition = thisTransform.position;
        Quaternion startRotation = thisTransform.rotation;

        float rate = 1.0f / animationTime;
        float t = 0.0f;
        while (t < 1.0)
        {
            t += Time.deltaTime * rate;
            thisTransform.rotation = Quaternion.Lerp(startRotation, resetRotation, Mathf.SmoothStep(0.0f, 1.0f, t));
            thisTransform.position = Vector3.Lerp(startPosition, resetPosition, Mathf.SmoothStep(0.0f, 1.0f, t));

            yield return null;
        }
    }
}

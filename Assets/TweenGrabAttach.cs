using System;
using System.Collections;
using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;

public class TweenGrabAttach : VRTK_FixedJointGrabAttach
{
    // Fine tuning
    public float percentToShrinkOnGrab = 90;
    public float startGrabAnimationTime = .5f;
    public float stopGrabAnimationTime = 1.5f;
    public float thrownAngularVelocityMultiplier = 0.3f;
    
    // Animation states
    bool beginStartGrabTween = false;
    bool beginStopGrabTween = false;

    Vector3 originalScale;

    // Attach parameters    
    GameObject grabbingObject;
    GameObject givenGrabbedObject;
    Rigidbody givenControllerAttachPoint;
    bool applyGrabbingObjectVelocity;

    // SoundsFX
    public AudioClip grabAudioClip;
    new AudioSource audio;

    public void Start()
    {
        audio = GetComponent<AudioSource>();
        originalScale = gameObject.transform.localScale;
    }

    public void StartGrabAnimationFinished()
    {
        Debug.Log("StartGrab animation finished");
        beginStartGrabTween = false;

        givenGrabbedObject.GetComponent<Rigidbody>().isKinematic = false;

        VRTK_SharedMethods.TriggerHapticPulse(VRTK_DeviceFinder.GetControllerIndex(grabbingObject), 0.75f);

        base.StartGrab(grabbingObject, givenGrabbedObject, givenControllerAttachPoint);
    }

    public void StopGrabAnimationFinished()
    {
        Debug.Log("StopGrab animation finished");
        beginStopGrabTween = false;

        var rb = givenGrabbedObject.GetComponent<Rigidbody>().isKinematic = true;
        base.StopGrab(applyGrabbingObjectVelocity);
    }

    /// <summary>
    /// The StartGrab method sets up the grab attach mechanic as soon as an object is grabbed. It is also responsible for creating the joint on the grabbed object.
    /// </summary>
    /// <param name="grabbingObject">The object that is doing the grabbing.</param>
    /// <param name="givenGrabbedObject">The object that is being grabbed.</param>
    /// <param name="givenControllerAttachPoint">The point on the grabbing object that the grabbed object should be attached to after grab occurs.</param>
    /// <returns>Is true if the grab is successful, false if the grab is unsuccessful.</returns>
    public override bool StartGrab(GameObject grabbingObject, GameObject givenGrabbedObject, Rigidbody givenControllerAttachPoint)
    {
        Debug.Log("StartGrab called");

        Invoke("PlayExpandClip", 0.05f);

        this.grabbingObject = grabbingObject;
        this.givenGrabbedObject = givenGrabbedObject;
        this.givenControllerAttachPoint = givenControllerAttachPoint;
                
        var interactable = givenGrabbedObject.GetComponent<VRTK_InteractableObject>();
        interactable.ZeroVelocity();
        interactable.isKinematic = true;

        beginStartGrabTween = true;
        beginStopGrabTween = false;

        var endScale = ((100f - percentToShrinkOnGrab) / 100f) * originalScale;
        StartCoroutine(TweenStartGrab(transform, endScale, startGrabAnimationTime, StartGrabAnimationFinished));

        return true;
    }

    /// <summary>
    /// The StopGrab method ends the grab of the current object and cleans up the state. It is also responsible for removing the joint from the grabbed object.
    /// </summary>
    /// <param name="applyGrabbingObjectVelocity">If true will apply the current velocity of the grabbing object to the grabbed object on release.</param>
    public override void StopGrab(bool applyGrabbingObjectVelocity)
    {
        Debug.Log("StopGrab called");

        Invoke("PlayExpandClip", 0.35f);
        
        this.applyGrabbingObjectVelocity = applyGrabbingObjectVelocity;
        ReleaseObject(applyGrabbingObjectVelocity);

        // StopGrab() fires during StartGrab animation, grabbedObject may be null
        if (grabbedObject != null)
        {
            // reduce the clocks angular velocity a bit
            var grabbedObjectRigidBody = grabbedObject.GetComponent<Rigidbody>();
            grabbedObjectRigidBody.angularVelocity = grabbedObjectRigidBody.angularVelocity * thrownAngularVelocityMultiplier;
        }

        beginStartGrabTween = false;
        beginStopGrabTween = true;

        StartCoroutine(TweenStopGrab(transform, originalScale, stopGrabAnimationTime, StopGrabAnimationFinished));
    }

    /// <summary>
    /// Tweens position, rotation, and scale of the clock to the controller during Grab
    /// </summary>
    /// <param name="thisTransform">Transform to animate</param>
    /// <param name="endScale">Ending scale of the object</param>
    /// <param name="animationTime">Time animation will take to execute</param>
    /// <param name="callback">Method to execute upon animation completed</param>
    public IEnumerator TweenStartGrab(Transform thisTransform, Vector3 endScale, float animationTime, Action callback)
    {
        Debug.Log("StartGrab animation started");

        Vector3 startPosition = thisTransform.position;
        Quaternion startRotation = thisTransform.rotation;
        Vector3 startScale = thisTransform.localScale;

        float rate = 1.0f / animationTime;
        float t = 0.0f;
        while (t < 1.0)
        {
            if (beginStopGrabTween)
            {
                Debug.Log("Stopping TweenStartGrab coroutine");
                callback();
                yield break;
            }

            t += Time.deltaTime * rate;
            thisTransform.rotation = Quaternion.Lerp(startRotation, givenControllerAttachPoint.transform.rotation * rightSnapHandle.localRotation, Mathf.SmoothStep(0.0f, 1.0f, t));
            thisTransform.position = Vector3.Lerp(startPosition, givenControllerAttachPoint.position - (rightSnapHandle.localPosition * endScale.x), Mathf.SmoothStep(0.0f, 1.0f, t));
            thisTransform.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0.0f, 1.0f, t));

            yield return null;
        }

        callback();
    }

    /// <summary>
    /// Tweens scale of the clock during StopGrab
    /// </summary>
    /// <param name="thisTransform">Transform to animate</param>
    /// <param name="startScale">Starting scale of the object</param>
    /// <param name="endScale">Ending scale of the object</param>
    /// <param name="animationTime">Time animation will take to execute</param>
    /// <param name="callback">Method to execute upon animation completed</param>
    public IEnumerator TweenStopGrab(Transform thisTransform, Vector3 endScale, float animationTime, Action callback)
    {
        Debug.Log("StopGrab animation started");
        Vector3 startScale = thisTransform.localScale;
        float rate = 1.0f / animationTime;
        float t = 0.0f;
        while (t < 1.0)
        {
            if (beginStartGrabTween)
            {
                Debug.Log("Stopping TweenStopGrab coroutine");
                callback();
                yield break;
            }

            t += Time.deltaTime * rate;
            thisTransform.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0.0f, 1.0f, t));

            yield return null;
        }

        callback();
    }

    private void PlayExpandClip()
    {
        audio.PlayOneShot(grabAudioClip, 0.3F);
    }
}
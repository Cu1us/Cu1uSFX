using System;
using UnityEngine;

namespace Cu1uSFX.Internal
{
    /// <summary>
    /// A helper component attached to the audiosources spawned by the SFXPlayer class, that makes sure the objects return to the pool when they have finished playing. 
    /// Also provides functions for manipulation of the audio playback.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SFXHandler : MonoBehaviour
    {
        /// <summary>
        /// The audio source component that this component manages.
        /// </summary>
        [NonSerialized] public AudioSource AudioSourceComponent;
        /// <summary>
        /// The audio source will follow the position of this transform if it isn't null, optionally at the local offset specified by <c>FollowTransformLocalOffset</c>.
        /// </summary>
        [NonSerialized] public Transform FollowTransform;
        /// <summary>
        /// The local offset to place the source at relative to the follow transform.
        /// </summary>
        [NonSerialized] public Vector3 FollowTransformLocalOffset;
        /// <summary>
        /// Called when the audio source has finished playing.
        /// </summary>
        [NonSerialized] public Action OnComplete;
        bool playing;
        float playingTime;
        /// <summary>
        /// As a failsafe, if the audio source is still active even after this many periods of its playtime have elapsed, it will be forcefully marked as completed. 
        /// Note that this means that if you manually pause the source for whatever reason, it will expire eventually due to this.
        /// </summary>
        const float MAX_CLIP_LENGTHS_ELAPSED_BEFORE_FORCE_END = 5f;

        void Awake()
        {
            AudioSourceComponent = GetComponent<AudioSource>();
        }
        void Update()
        {
            if (playing)
            {
                playingTime += Time.deltaTime;
                if (FollowTransform)
                {
                    transform.position = FollowTransform.TransformPoint(FollowTransform.localPosition + FollowTransformLocalOffset);
                }
                if (AudioSourceComponent.time >= AudioSourceComponent.clip.length || playingTime > AudioSourceComponent.clip.length * MAX_CLIP_LENGTHS_ELAPSED_BEFORE_FORCE_END)
                {
                    OnComplete?.Invoke();
                }
            }
        }
        /// <summary>
        /// Stops the audio source and sends the event that it has finished playing.
        /// </summary>
        public void Stop()
        {
            if (playing)
            {
                AudioSourceComponent.Stop();
                playing = false;
                OnComplete?.Invoke();
            }
        }
        public void Play(Vector3? worldPosition = null)
        {
            FollowTransform = null;
            FollowTransformLocalOffset = Vector3.zero;
            transform.position = worldPosition ?? default;
            AudioSourceComponent.spatialize = worldPosition != null;
            AudioSourceComponent.Play();
            playing = true;
        }
        public void Play(Transform followTransform)
        {
            FollowTransform = followTransform;
            FollowTransformLocalOffset = Vector3.zero;
            transform.position = followTransform.position;
            AudioSourceComponent.spatialize = true;
            AudioSourceComponent.Play();
            playing = true;
        }
        public void Play(Transform followTransform, Vector3 localOffset)
        {
            FollowTransform = followTransform;
            FollowTransformLocalOffset = localOffset;
            transform.position = FollowTransform.TransformPoint(FollowTransform.localPosition + FollowTransformLocalOffset);
            AudioSourceComponent.spatialize = true;
            AudioSourceComponent.Play();
            playing = true;
        }
        /// <summary>
        /// Resets the audio source and all its data. Called by the SFXPlayer when returning this object to the object pool.
        /// </summary>
        public void Reset()
        {
            playing = false;
            playingTime = 0;
            AudioSourceComponent.Stop();
            AudioSourceComponent.clip = null;
            AudioSourceComponent.pitch = 1;
            AudioSourceComponent.volume = 1;
            AudioSourceComponent.spatialize = false;
            FollowTransform = null;
            FollowTransformLocalOffset = Vector3.zero;
            OnComplete = null;
        }
    }
}
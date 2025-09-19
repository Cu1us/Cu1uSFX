using System;
using UnityEngine;

namespace Cu1uSFX.Internal
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXHandler : MonoBehaviour
    {
        [NonSerialized] public AudioSource AudioSourceComponent;
        [NonSerialized] public Transform FollowTransform;
        [NonSerialized] public Vector3 FollowTransformLocalOffset;
        [NonSerialized] public Action OnComplete;
        bool playing;
        float playingTime;
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
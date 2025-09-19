using System;
using System.Collections.Generic;
using Cu1uSFX.Internal;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Cu1uSFX
{
    public static class SFXPlayer
    {
        static ObjectPool<AudioSource> SourcePool;
        static readonly List<SFXReference> ActiveReferences = new();

        static SFXReference PrepareAudioSource(SFXDefinition sfx, float volume = 1, float pitch = 1)
        {
            AudioSource source = SourcePool.Get();
            (source.clip, source.volume, source.pitch) = sfx.Sample();

            SFXHandler handler = source.GetComponent<SFXHandler>();

            SFXReference sfxRef = new(source, handler);
            ActiveReferences.Add(sfxRef);

            if (volume != 1)
                sfxRef.Volume = volume;
            if (pitch != 1)
                sfxRef.Pitch = pitch;

            handler.OnComplete += () =>
            {
                ActiveReferences.Remove(sfxRef);
                SourcePool.Release(sfxRef.AudioSource);
                sfxRef.NotifyFinishedAndDispose();
            };

            return sfxRef;
        }

        #region Play functions

        public static SFXReference Play(this SFXDefinition sfx, float volume = 1, float pitch = 1)
        {
            if (sfx == null)
            {
                Debug.LogWarning("[Cu1uSFX] Warning: Tried to play a 'None' sound.");
                return null;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.LogError("[Cu1uSFX] Error: Cannot play sounds outside of play mode!");
                return null;
            }
#endif
            SFXReference sfxRef = PrepareAudioSource(sfx, volume, pitch);
            sfxRef.Handler.Play();
            return sfxRef;
        }
        public static SFXReference Play(this SFXDefinition sfx, Vector3 worldPosition, float volume = 1, float pitch = 1)
        {
            if (sfx == null)
            {
                Debug.LogWarning("[Cu1uSFX] Warning: Tried to play a 'None' sound.");
                return null;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.LogError("[Cu1uSFX] Error: Cannot play sounds outside of play mode!");
                return null;
            }
#endif
            SFXReference sfxRef = PrepareAudioSource(sfx, volume, pitch);
            sfxRef.Handler.Play(worldPosition);
            return sfxRef;
        }
        public static SFXReference Play(this SFXDefinition sfx, Transform transformToFollow, float volume = 1, float pitch = 1)
        {
            if (sfx == null)
            {
                Debug.LogWarning("[Cu1uSFX] Warning: Tried to play a 'None' sound.");
                return null;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.LogError("[Cu1uSFX] Error: Cannot play sounds outside of play mode!");
                return null;
            }
#endif
            SFXReference sfxRef = PrepareAudioSource(sfx, volume, pitch);
            sfxRef.Handler.Play(transformToFollow);
            return sfxRef;
        }
        public static SFXReference Play(this SFXDefinition sfx, Transform transformToFollow, Vector3 followOffset, float volume = 1, float pitch = 1)
        {
            if (sfx == null)
            {
                Debug.LogWarning("[Cu1uSFX] Warning: Tried to play a 'None' sound.");
                return null;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.LogError("[Cu1uSFX] Error: Cannot play sounds outside of play mode!");
                return null;
            }
#endif
            SFXReference sfxRef = PrepareAudioSource(sfx, volume, pitch);
            sfxRef.Handler.Play(transformToFollow, followOffset);
            return sfxRef;
        }

        public static SFXReference Play(this SFX sfx, float volume = 1, float pitch = 1)
        {
            return Play(sfx.GetData(), volume, pitch);
        }
        public static SFXReference Play(this SFX sfx, Vector3 worldPosition, float volume = 1, float pitch = 1)
        {
            return Play(sfx.GetData(), worldPosition, volume, pitch);
        }
        public static SFXReference Play(this SFX sfx, Transform transformToFollow, float volume = 1, float pitch = 1)
        {
            return Play(sfx.GetData(), transformToFollow, volume, pitch);
        }
        public static SFXReference Play(this SFX sfx, Transform transformToFollow, Vector3 followOffset, float volume = 1, float pitch = 1)
        {
            return Play(sfx.GetData(), transformToFollow, followOffset, volume, pitch);
        }

        #endregion


        public static SFXDefinition GetData(this SFX sfx)
        {
            if (sfx == 0) return null;
            if ((int)sfx < 0 || (int)sfx - 1 >= SFXList.Instance.Definitions.Length)
            {
                throw new ArgumentException
                (
                "[Cu1uSFX] The specified SFX has an invalid value and no associated sound."
                + " This can sometimes happen if a sound effect's definition is deleted, but it's still referenced in a serialized object.",
                nameof(sfx)
                );
            }
            return SFXList.Instance.Definitions[(int)sfx - 1];
        }

        public static int StopAll()
        {
            int count = 0;
            foreach (SFXReference sfxRef in ActiveReferences.ToArray())
            {
                sfxRef.Stop();
                count++;
            }
            return count;
        }
        public static void Stop(this SFXReference sfxReference, bool runFinishedCallback = true)
        {
            sfxReference.Stop();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializePool()
        {
            SourcePool = new(
                createFunc: PoolCreate,
                actionOnGet: PoolGet,
                actionOnDestroy: PoolDestroy,
                actionOnRelease: PoolRelease,
                defaultCapacity: 3,
                maxSize: 10
            );
        }
        static AudioSource PoolCreate()
        {
            GameObject go = new("[Cu1uSFX] AudioSource", typeof(AudioSource), typeof(SFXHandler));
            Object.DontDestroyOnLoad(go);
            AudioSource source = go.GetComponent<AudioSource>();
            return source;
        }
        static void PoolGet(AudioSource source)
        {
            source.gameObject.SetActive(true);
        }
        static void PoolRelease(AudioSource source)
        {
            source.GetComponent<SFXHandler>().Reset();
            source.gameObject.SetActive(false);
        }
        static void PoolDestroy(AudioSource source)
        {
            Object.Destroy(source.gameObject);
        }
    }
    public class SFXReference
    {
        readonly public AudioSource AudioSource;
        readonly internal SFXHandler Handler;
        public Action OnFinishedPlaying;
        public bool IsValid { get; private set; } = true;

        public float Volume
        {
            get => IsValid ? AudioSource.volume / InitialVolume : float.NaN;
            set { if (IsValid) { AudioSource.volume = value * InitialVolume; } }
        }
        public float Pitch
        {
            get => IsValid ? AudioSource.pitch / InitialPitch : float.NaN;
            set { if (IsValid) { AudioSource.pitch = value * InitialPitch; } }
        }
        public Vector3? WorldPosition
        {
            get => IsValid && AudioSource.spatialize ? AudioSource.transform.position : null;
            set
            {
                if (IsValid)
                {
                    AudioSource.spatialize = value == null;
                    AudioSource.transform.position = value == null ? default : value.Value;
                }
            }
        }
        public void FollowTransform(Transform transform, Vector3 localOffset = default)
        {
            Handler.FollowTransform = transform;
            Handler.FollowTransformLocalOffset = localOffset;
            AudioSource.spatialize = true;
        }
        internal void NotifyFinishedAndDispose()
        {
            OnFinishedPlaying?.Invoke();
            OnFinishedPlaying = null;
            IsValid = false;
        }
        public void Stop()
        {
            Handler.Stop();
        }

        public readonly float InitialPitch;
        public readonly float InitialVolume;

        internal SFXReference(AudioSource audioSource, SFXHandler handler)
        {
            AudioSource = audioSource;
            Handler = handler;
            InitialPitch = audioSource.pitch;
            InitialVolume = audioSource.volume;
        }
    }
}
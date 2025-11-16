using System;
using System.Collections.Generic;
using Cu1uSFX.Internal;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

// Cu1uSFX Sound Effect Plugin
// Copyright (C) 2025  Måns Fritiofsson

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace Cu1uSFX
{
    /// <summary>
    /// Static class that allows you to play PredefinedSFX and SFXDefinitions. Utilizes object pooling to efficiently supply audio sources for each sound effect.
    /// </summary>
    public static class SFXPlayer
    {
        static ObjectPool<AudioSource> SourcePool;
        static readonly List<SFXReference> ActiveReferences = new();

        static SFXReference PrepareAudioSource(SFXDefinition sfx, float volume = 1, float pitch = 1)
        {
            (AudioClip clip, float sampledVolume, float sampledPitch) = sfx.Sample();

            if (clip == null)
            {
                SFXList.LogWarningIfFlag(SFXLogFlags.SFX_HAD_NULL_CLIP,
                $"[Cu1uSFX] Failed to play SFX '{sfx.Name}': Sampling returned a null AudioClip! Does it not have any clips assigned?");
                return null;
            }

            AudioSource source = SourcePool.Get();
            source.clip = clip;
            source.volume = sampledVolume;
            source.pitch = sampledPitch;

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

        /// <summary>
        /// Play this SFX globally.
        /// </summary>
        /// <param name="sfx">The SFX to play.</param>
        /// <param name="volume">The volume multiplier to use when playing the sound.</param>
        /// <param name="pitch">The pitch multiplier to use when playing the sound.</param>
        /// <returns>A handle</returns>
        public static SFXReference Play(this PredefinedSFX sfx, float volume = 1, float pitch = 1)
        {
            return Play(sfx.Definition, volume, pitch);
        }
        /// <summary>
        /// Play this SFX at a specified world position.
        /// </summary>
        /// <param name="sfx">The SFX to play.</param>
        /// <param name="worldPosition">The world space position to play the sound at.</param>
        /// <param name="volume">The volume multiplier to use when playing the sound.</param>
        /// <param name="pitch">The pitch multiplier to use when playing the sound.</param>
        public static SFXReference Play(this PredefinedSFX sfx, Vector3 worldPosition, float volume = 1, float pitch = 1)
        {
            return Play(sfx.Definition, worldPosition, volume, pitch);
        }
        /// <summary>
        /// Play this SFX, making the audio source follow the specified transform.
        /// </summary>
        /// <param name="sfx">The SFX to play.</param>
        /// <param name="transformToFollow">The transform to follow. (The AudioSource will not be childed to this transform)</param>
        /// <param name="volume">The volume multiplier to use when playing the sound.</param>
        /// <param name="pitch">The pitch multiplier to use when playing the sound.</param>
        public static SFXReference Play(this PredefinedSFX sfx, Transform transformToFollow, float volume = 1, float pitch = 1)
        {
            return Play(sfx.Definition, transformToFollow, volume, pitch);
        }
        /// <summary>
        /// Play this SFX, making the audio source follow the specified transform at a specified offset.
        /// </summary>
        /// <param name="sfx">The SFX to play.</param>
        /// <param name="transformToFollow">The transform to follow. (The AudioSource will not be childed to this transform)</param>
        /// <param name="followOffset">The local-space offset from the transform that the AudioSource should follow at.</param>
        /// <param name="volume">The volume multiplier to use when playing the sound.</param>
        /// <param name="pitch">The pitch multiplier to use when playing the sound.</param>
        public static SFXReference Play(this PredefinedSFX sfx, Transform transformToFollow, Vector3 followOffset, float volume = 1, float pitch = 1)
        {
            return Play(sfx.Definition, transformToFollow, followOffset, volume, pitch);
        }

        /// <summary>
        /// Play this SFX globally.
        /// </summary>
        /// <param name="sfx">The SFX to play.</param>
        /// <param name="volume">The volume multiplier to use when playing the sound.</param>
        /// <param name="pitch">The pitch multiplier to use when playing the sound.</param>
        public static SFXReference Play(this SFXDefinition sfx, float volume = 1, float pitch = 1)
        {
            if (sfx == null)
            {
                SFXList.LogWarningIfFlag(SFXLogFlags.NULL_SFX_PLAYED, "[Cu1uSFX] Warning: Tried to play a 'None' sound.");
                return null;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SFXList.LogWarningIfFlag(SFXLogFlags.PLAYING_IN_EDIT_MODE, "[Cu1uSFX] Warning: Cannot play sounds outside of play mode!");
                return null;
            }
#endif
            SFXReference sfxRef = PrepareAudioSource(sfx, volume, pitch);
            sfxRef?.Handler.Play();
            return sfxRef;
        }
        /// <summary>
        /// Play this SFX at a specified world position.
        /// </summary>
        /// <param name="sfx">The SFX to play.</param>
        /// <param name="worldPosition">The world space position to play the sound at.</param>
        /// <param name="volume">The volume multiplier to use when playing the sound.</param>
        /// <param name="pitch">The pitch multiplier to use when playing the sound.</param>
        public static SFXReference Play(this SFXDefinition sfx, Vector3 worldPosition, float volume = 1, float pitch = 1)
        {
            if (sfx == null)
            {
                SFXList.LogWarningIfFlag(SFXLogFlags.NULL_SFX_PLAYED, "[Cu1uSFX] Warning: Tried to play a 'None' sound.");
                return null;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SFXList.LogWarningIfFlag(SFXLogFlags.PLAYING_IN_EDIT_MODE, "[Cu1uSFX] Warning: Cannot play sounds outside of play mode!");
            }
#endif
            SFXReference sfxRef = PrepareAudioSource(sfx, volume, pitch);
            sfxRef?.Handler.Play(worldPosition);
            return sfxRef;
        }
        /// <summary>
        /// Play this SFX, making the audio source follow the specified transform.
        /// </summary>
        /// <param name="sfx">The SFX to play.</param>
        /// <param name="transformToFollow">The transform to follow. (The AudioSource will not be childed to this transform)</param>
        /// <param name="volume">The volume multiplier to use when playing the sound.</param>
        /// <param name="pitch">The pitch multiplier to use when playing the sound.</param>
        public static SFXReference Play(this SFXDefinition sfx, Transform transformToFollow, float volume = 1, float pitch = 1)
        {
            if (sfx == null)
            {
                SFXList.LogWarningIfFlag(SFXLogFlags.NULL_SFX_PLAYED, "[Cu1uSFX] Warning: Tried to play a 'None' sound.");
                return null;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SFXList.LogWarningIfFlag(SFXLogFlags.PLAYING_IN_EDIT_MODE, "[Cu1uSFX] Warning: Cannot play sounds outside of play mode!");
            }
#endif
            SFXReference sfxRef = PrepareAudioSource(sfx, volume, pitch);
            sfxRef?.Handler.Play(transformToFollow);
            return sfxRef;
        }
        /// <summary>
        /// Play this SFX, making the audio source follow the specified transform at a specified offset.
        /// </summary>
        /// <param name="sfx">The SFX to play.</param>
        /// <param name="transformToFollow">The transform to follow. (The AudioSource will not be childed to this transform)</param>
        /// <param name="followOffset">The local-space offset from the transform that the AudioSource should follow at.</param>
        /// <param name="volume">The volume multiplier to use when playing the sound.</param>
        /// <param name="pitch">The pitch multiplier to use when playing the sound.</param>
        public static SFXReference Play(this SFXDefinition sfx, Transform transformToFollow, Vector3 followOffset, float volume = 1, float pitch = 1)
        {
            if (sfx == null)
            {
                SFXList.LogWarningIfFlag(SFXLogFlags.NULL_SFX_PLAYED, "[Cu1uSFX] Warning: Tried to play a 'None' sound.");
                return null;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SFXList.LogWarningIfFlag(SFXLogFlags.PLAYING_IN_EDIT_MODE, "[Cu1uSFX] Warning: Cannot play sounds outside of play mode!");
            }
#endif
            SFXReference sfxRef = PrepareAudioSource(sfx, volume, pitch);
            sfxRef?.Handler.Play(transformToFollow, followOffset);
            return sfxRef;
        }

        /// <summary>
        /// Play this AudioClip globally.
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        /// <param name="volume">The volume multiplier to use when playing the sound.</param>
        /// <param name="pitch">The pitch multiplier to use when playing the sound.</param>
        public static SFXReference Play(this AudioClip clip, float volume = 1, float pitch = 1)
        {
            SFXDefinition definition = new(clip);
            return Play(definition, volume, pitch);
        }
        /// <summary>
        /// Play this AudioClip at a specified world position.
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        /// <param name="worldPosition">The world space position to play the sound at.</param>
        /// <param name="volume">The volume multiplier to use when playing the sound.</param>
        /// <param name="pitch">The pitch multiplier to use when playing the sound.</param>
        public static SFXReference Play(this AudioClip clip, Vector3 worldPosition, float volume = 1, float pitch = 1)
        {
            SFXDefinition definition = new(clip);
            return Play(definition, worldPosition, volume, pitch);
        }
        /// <summary>
        /// Play this AudioClip, making the audio source follow the specified transform.
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        /// <param name="transformToFollow">The transform to follow. (The AudioSource will not be childed to this transform)</param>
        /// <param name="volume">The volume multiplier to use when playing the sound.</param>
        /// <param name="pitch">The pitch multiplier to use when playing the sound.</param>
        public static SFXReference Play(this AudioClip clip, Transform transformToFollow, float volume = 1, float pitch = 1)
        {
            SFXDefinition definition = new(clip);
            return Play(definition, transformToFollow, volume, pitch);
        }
        /// <summary>
        /// Play this AudioClip, making the audio source follow the specified transform at a specified offset.
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        /// <param name="transformToFollow">The transform to follow. (The AudioSource will not be childed to this transform)</param>
        /// <param name="followOffset">The local-space offset from the transform that the AudioSource should follow at.</param>
        /// <param name="volume">The volume multiplier to use when playing the sound.</param>
        /// <param name="pitch">The pitch multiplier to use when playing the sound.</param>
        /// <returns></returns>
        public static SFXReference Play(this AudioClip clip, Transform transformToFollow, Vector3 followOffset, float volume = 1, float pitch = 1)
        {
            SFXDefinition definition = new(clip);
            return Play(definition, transformToFollow, followOffset, volume, pitch);
        }

        #endregion

        /// <summary>
        /// Stops all currently playing sound effects.
        /// </summary>
        /// <returns>The amount of sound effects that were stopped.</returns>
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
        /// <summary>
        /// Stops the specified sound effect.
        /// </summary>
        /// <param name="sfxReference">The reference to the sound effect to stop.</param>
        /// <param name="runFinishedCallback"></param>
        // public static void Stop(this SFXReference sfxReference, bool runFinishedCallback = true)
        // {
        //     sfxReference.Stop(runFinishedCallback); // Circular method call - prioritizes this extension above the Stop() defined in SFXReference
        // }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializePool()
        {
            SourcePool = new(
                createFunc: PoolCreate,
                actionOnGet: PoolGet,
                actionOnDestroy: PoolDestroy,
                actionOnRelease: PoolRelease,
                defaultCapacity: SFXList.Instance.AudioSourcePoolDefault,
                maxSize: SFXList.Instance.AudioSourcePoolMax
            );
        }
        static AudioSource PoolCreate()
        {
            GameObject go = new("[Cu1uSFX] AudioSource", typeof(AudioSource), typeof(SFXHandler))
            {
                hideFlags = HideFlags.NotEditable
            };
            Object.DontDestroyOnLoad(go);
            AudioSource source = go.GetComponent<AudioSource>();
            source.playOnAwake = false;
            SFXList.LogIfFlag(SFXLogFlags.NOTIF_VERBOSE, "[Cu1uSFX] Instantiated new AudioSource", go);
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
            SFXList.LogIfFlag(SFXLogFlags.NOTIF_VERBOSE, "[Cu1uSFX] Destroyed overflow AudioSource", source.gameObject);
            Object.Destroy(source.gameObject);
        }
    }
    public class SFXReference
    {
        AudioSource _audioSource;
        /// <summary>
        /// The audio source component that this reference refers to.
        /// </summary>
        /// <remarks>
        /// Note: If you make changes to this, you must revert them before the source is put back into the object pool! This can be bound to <c>OnFinishedPlaying</c>.
        /// </remarks>
        public AudioSource AudioSource => _audioSource;
        /// <summary>
        /// The audio source handler component that this reference refers to.
        /// </summary>
        /// <remarks>
        /// Note: If you make changes to this, you must revert them before the source is put back into the object pool! This can be bound to <c>OnFinishedPlaying</c>.
        /// </remarks>
        readonly internal SFXHandler Handler;
        /// <summary>
        /// Callback that is executed when the source has finished playing, just before this reference becomes invalid.
        /// </summary>
        public Action OnFinishedPlaying;
        /// <summary>
        /// Is this reference valid?
        /// </summary>
        /// <remarks>
        /// A SFXReference becomes invalid when its referenced sound finishes playing, or is stopped.
        /// </remarks>
        public bool IsValid { get; private set; } = true;

        /// <summary>
        /// The volume multiplier for this sound effect.
        /// </summary>
        /// <remarks>
        /// This modifier always starts out at 1, and acts multiplicatively along with any random volume defined in the Sound Effects list.
        /// </remarks>
        public float Volume
        {
            get => IsValid ? AudioSource.volume / InitialVolume : float.NaN;
            set
            {
                if (IsValid) { AudioSource.volume = value * InitialVolume; }
                else
                {
                    SFXList.LogWarningIfFlag(SFXLogFlags.SFXREF_EXPIRED, $"[Cu1uSFX] Script attempted to set {nameof(Volume)} on an expired SFXReference.");
                }
            }
        }
        /// <summary>
        /// The pitch multiplier for this sound effect.
        /// </summary>
        /// <remarks>
        /// This modifier always starts out at 1, and acts multiplicatively along with any random pitch defined in the Sound Effects list.
        /// </remarks>
        public float Pitch
        {
            get => IsValid ? AudioSource.pitch / InitialPitch : float.NaN;
            set
            {
                if (IsValid) { AudioSource.pitch = value * InitialPitch; }
                else
                {
                    SFXList.LogWarningIfFlag(SFXLogFlags.SFXREF_EXPIRED, $"[Cu1uSFX] Script attempted to set {nameof(Pitch)} on an expired SFXReference.");
                }
            }
        }
        /// <summary>
        /// The world-space position that this sound is being played at.
        /// </summary>
        /// <remarks>
        /// Set to null to make the sound play globally. Changing this value while the sound is following a transform will cause it to stop following it.
        /// </remarks>
        public Vector3? WorldPosition
        {
            get => IsValid && AudioSource.spatialize ? AudioSource.transform.position : null;
            set
            {
                if (IsValid)
                {
                    Handler.FollowTransform = null;
                    AudioSource.spatialize = value == null;
                    AudioSource.transform.position = value == null ? default : value.Value;
                }
                else
                {
                    SFXList.LogWarningIfFlag(SFXLogFlags.SFXREF_EXPIRED, $"[Cu1uSFX] Script attempted to set {nameof(WorldPosition)} on an expired SFXReference.");
                }
            }
        }
        /// <summary>
        /// Makes the sound follow a specific transform, optionally with an offset.
        /// </summary>
        /// <param name="transform">The transform to follow.</param>
        /// <param name="localOffset">The local-space offset relative to the transform to position the sound at.</param>
        public void FollowTransform(Transform transform, Vector3 localOffset = default)
        {
            if (!IsValid || !Handler)
            {
                SFXList.LogWarningIfFlag(SFXLogFlags.SFXREF_EXPIRED, $"[Cu1uSFX] Script attempted to invoke {nameof(FollowTransform)} on an expired SFXReference.");
                return;
            }
            Handler.FollowTransform = transform;
            Handler.FollowTransformLocalOffset = localOffset;
            AudioSource.spatialize = true;
        }
        internal void NotifyFinishedAndDispose()
        {
            OnFinishedPlaying?.Invoke();
            OnFinishedPlaying = null;
            _audioSource = null;
            IsValid = false;
        }
        /// <summary>
        /// Stops the sound, and disposes this reference.
        /// </summary>
        public void Stop()
        {
            if (IsValid && Handler)
                Handler.Stop();
            else
                SFXList.LogWarningIfFlag(SFXLogFlags.SFXREF_EXPIRED, $"[Cu1uSFX] Script attempted to stop an expired SFXReference.");
        }

        /// <summary>
        /// The pitch that this sound was originally played at, as defined in the Sound Effects list.
        /// </summary>
        public readonly float InitialPitch;
        /// <summary>
        /// The volume that this sound was originally played at, as defined in the Sound Effects list.
        /// </summary>
        public readonly float InitialVolume;

        internal SFXReference(AudioSource audioSource, SFXHandler handler)
        {
            _audioSource = audioSource;
            Handler = handler;
            InitialPitch = audioSource.pitch;
            InitialVolume = audioSource.volume;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
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

namespace Cu1uSFX.Internal
{
    /// <summary>
    /// The singleton ScriptableObject that stores all sound effects that you define in the Sound Effects window.
    /// </summary>
    public class SFXList : ScriptableObject
    {
        public const string SINGLETON_ASSET_NAME = "SFX List";

        static SFXList _instance;
        /// <summary>
        /// The instance of this singleton. Guaranteed to always exist: if it isn't loaded, it loads it with Resources.Load() and initializes it. 
        /// If it doesn't exist, it creates it.
        /// </summary>
        public static SFXList Instance
        {
            get
            {
                if (_instance == null)
                {
                    LoadSingleton();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Easily accessed list of the currently generated SFX enums.
        /// </summary>
        public List<string> EnumNames = new();
        /// <summary>
        /// The array of defined sound effects, containing the definitions as seen in the Sound Effects window.
        /// </summary>
        public SFXDefinition[] Definitions;
#if UNITY_EDITOR
        /// <summary>
        /// The path of the script to fill when generating the SFX enum names. Note that changing this to another script or file will permanently overwrite its contents!
        /// </summary>
        public string SFXEnumScriptPath = "Assets/Scripts/SFXEnum.cs";
#endif

        public SFXLogFlags LogFlags = SFXLogFlags.DEFAULT;
        [Min(1)] public int AudioSourcePoolMax = 10;
        [Min(0)] public int AudioSourcePoolDefault = 3;

        /// <summary>
        /// Makes sure the asset exists and is assigned as the singleton instance, and loads/creates/assigns it if not.
        /// </summary>
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        static void LoadSingleton()
        {
            _instance = Resources.Load<SFXList>($"{SINGLETON_ASSET_NAME}");
            if (_instance == null)
            {
                _instance = CreateInstance<SFXList>();
#if UNITY_EDITOR
                if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                }
                UnityEditor.AssetDatabase.CreateAsset(_instance, $"Assets/Resources/{SINGLETON_ASSET_NAME}.asset");
                LogIfFlag(SFXLogFlags.NOTIF_INFO, $"[Cu1uSFX] Created a new SFX List asset at '/Assets/Resources/{SINGLETON_ASSET_NAME}'!", _instance);
#endif
            }
#if UNITY_EDITOR
            MakeSureSFXEnumScriptPathIsValid();
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Makes sure the script defined at the <c>SFXEnumScriptPath</c> exists, and if not, reset the path to default, and create the script if necessary.
        /// </summary>
        /// <returns>True if the path already existed.</returns>
        public static bool MakeSureSFXEnumScriptPathIsValid()
        {
            ref string path = ref Instance.SFXEnumScriptPath;
            if (!UnityEditor.AssetDatabase.AssetPathExists(path))
            {
                path = string.Empty;
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                path = "Assets/Scripts/SFXEnum.cs";
                if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Scripts"))
                {
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "Scripts");
                }

                using System.IO.StreamWriter writer = new(path);
                writer.WriteLine("using UnityEngine;");
                writer.WriteLine("");
                writer.WriteLine("// DO NOT EDIT - This script will be automatically filled with enums that represent sound effecs that you create! Add sound effects in Window/SFX Editor");
                writer.WriteLine("// DO NOT MOVE THIS SCRIPT - Assign a new script to fill with enums instead in the 'Advanced' tab in the SFX List at /Assets/Resources/SFX List");
                writer.Close();
                UnityEditor.AssetDatabase.ImportAsset(path);

                return false;
            }
            return true;
        }
#endif

        /// <summary>
        /// Given a PredefinedSFX, returns its name.
        /// </summary>
        /// <param name="sfx">The PredefinedSFX to get the name of.</param>
        /// <returns>The name of the PredefinedSFX, or null if the SFX is invalid.</returns>
        public static string GetNameOfSFX(PredefinedSFX sfx)
        {
            if (!IsValidSFX(sfx))
                return null;
            return _instance.Definitions[sfx.IndexInSFXList.Value].Name;
        }
        /// <summary>
        /// Given an index, returns the name of the SFX at that position in the SFX list.
        /// </summary>
        /// <param name="sfx">The index of the SFX to get the name of.</param>
        /// <returns>The name of the found sfx, or null if the index is out of bounds.</returns>
        public static string GetNameOfSFX(ushort sfx)
        {
            if (sfx >= Instance.Definitions.Length)
                return null;
            return _instance.Definitions[sfx].Name;
        }
        /// <summary>
        /// Loops through all sound effects to find the SFX with the specified name, and returns a PredefinedSFX pointing to it.
        /// </summary>
        /// <param name="name">The SFX name to search for.</param>
        /// <returns>A PredefinedSFX instance pointing to the SFX, or a null value if it isn't found.</returns>
        public static PredefinedSFX GetSFXByName(string name)
        {
            for (ushort i = 0; i < Instance.Definitions.Length; i++)
            {
                if (_instance.Definitions[i].Name == name)
                    return new(i);
            }
            return PredefinedSFX.NullValue;
        }
        /// <summary>
        /// Loops through all sound effects to check if one of them has the specified name.
        /// </summary>
        /// <param name="name">The name to look for.</param>
        /// <returns>True if a SFX with that name exists in the SFX list.</returns>
        public static bool HasSFXWithName(string name)
        {
            for (ushort i = 0; i < Instance.Definitions.Length; i++)
            {
                if (_instance.Definitions[i].Name == name)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Given a name, finds the index of the SFX with that name in the SFX list.
        /// </summary>
        /// <param name="name">The name of the SFX to look for.</param>
        /// <returns>The index of the found SFX, or null if none was found.</returns>
        internal static ushort? GetSFXIDByName(string name)
        {
            for (ushort i = 0; i < Instance.Definitions.Length; i++)
            {
                if (_instance.Definitions[i].Name == name)
                    return i;
            }
            return null;
        }
        /// <summary>
        /// Checks if a specified PredefinedSFX points to a valid SFX in the SFX list.
        /// </summary>
        /// <param name="sfx">The PredefinedSFX to verify.</param>
        /// <returns>True if it's valid and safe to use, or false otherwise.</returns>
        public static bool IsValidSFX(PredefinedSFX sfx) => sfx.IndexInSFXList != null && sfx.IndexInSFXList.Value < Instance.Definitions.Length;
        /// <summary>
        /// Checks if a specified ushort points to a valid index in the SFX list.
        /// </summary>
        /// <param name="sfx">The index to verify the validity of.</param>
        /// <returns>True if the index is valid, false if it's out of bounds.</returns>
        public static bool IsValidSFX(ushort sfx) => sfx != ushort.MaxValue && sfx < Instance.Definitions.Length;
        /// <summary>
        /// Safely gets the SFXDefinition that a specified PredefinedSFX points to.
        /// </summary>
        /// <param name="sfx">The PredefinedSFX to get the definition of.</param>
        /// <returns>The definition of the specified SFX, or null if it's invalid.</returns>
        public static SFXDefinition GetDefinitionOfSFX(PredefinedSFX sfx) => IsValidSFX(sfx) ? Instance.Definitions[sfx.IndexInSFXList.Value] : null;
        /// <summary>
        /// Safely try to get the definition of the SFX at the specified index in the SFX list.
        /// </summary>
        /// <param name="sfx">The index of the SFX to get the definition of.</param>
        /// <returns>The SFXDefinition of the SFX at the specified index, or null if the index is out of bounds.</returns>
        public static SFXDefinition GetDefinitionOfSFX(ushort sfx) => IsValidSFX(sfx) ? Instance.Definitions[sfx] : null;

        public static void LogIfFlag(SFXLogFlags flag, string message)
        {
            if ((Instance.LogFlags & flag) == flag)
                Debug.Log(message);
        }
        public static void LogIfFlag(SFXLogFlags flag, string message, Object context)
        {
            if ((Instance.LogFlags & flag) == flag)
                Debug.Log(message, context);
        }
        public static void LogWarningIfFlag(SFXLogFlags flag, string message)
        {
            if ((Instance.LogFlags & flag) == flag)
                Debug.LogWarning(message);
        }
        public static void LogWarningIfFlag(SFXLogFlags flag, string message, Object context)
        {
            if ((Instance.LogFlags & flag) == flag)
                Debug.LogWarning(message, context);
        }
        public static void LogErrorIfFlag(SFXLogFlags flag, string message)
        {
            if ((Instance.LogFlags & flag) == flag)
                Debug.LogError(message);
        }
        public static void LogErrorIfFlag(SFXLogFlags flag, string message, Object context)
        {
            if ((Instance.LogFlags & flag) == flag)
                Debug.LogError(message, context);
        }
    }

    /// <summary>
    /// Enum for determining the types of logs that the Cu1uSFX plugin should send.
    /// </summary>
    [Flags]
    public enum SFXLogFlags : byte
    {
        [InspectorName("No logs")] NONE = 0,
        [InspectorName("Errors/Critical")] INTERNAL_ERROR_CRITICAL = 1,
        [InspectorName("Errors/Non-critical")] INTERNAL_ERROR_NONCRITICAL = 2,
        [InspectorName("Info/Basic")] NOTIF_INFO = 4,
        [InspectorName("Info/Verbose")] NOTIF_VERBOSE = 8,
        [InspectorName("Warnings/Playing 'NONE' sounds")] NULL_SFX_PLAYED = 16,
        [InspectorName("Warnings/Playing SFX with no clips")] SFX_HAD_NULL_CLIP = 32,
        [InspectorName("Warnings/Playing SFX in edit mode")] PLAYING_IN_EDIT_MODE = 64,
        [InspectorName("Warnings/Using expired SFXReference")] SFXREF_EXPIRED = 128,
        [InspectorName("Default settings")] DEFAULT = INTERNAL_ERROR_CRITICAL | INTERNAL_ERROR_NONCRITICAL | NOTIF_INFO | SFX_HAD_NULL_CLIP | PLAYING_IN_EDIT_MODE
    }

    /// <summary>
    /// The definition of a sound effect, containing audio clips, pitch and volume to use.
    /// </summary>
    /// <remarks>
    /// Can be played with <c>SFXPlayer.Play()</c>.
    /// </remarks>
    [Serializable]
    public record SFXDefinition
    {
#pragma warning disable IDE0044 // Suppress IDE044: Add readonly modifier
        [SerializeField] string _name;
        [SerializeField] string _category;
        [SerializeField] AudioClip[] _clips;
        [SerializeField] bool _randomizeVolume;
        [SerializeField] float _volumeMin = 1;
        [SerializeField] float _volumeMax = 1;
        [SerializeField] bool _randomizePitch;
        [SerializeField] float _pitchMin = 1;
        [SerializeField] float _pitchMax = 1;
#pragma warning restore IDE0044 // Suppress IDE0044: Add readonly modifier

        /// <summary>
        /// The name of the sound effect, as defined in the Sound Effects list.
        /// </summary>
        public string Name => _name;
        /// <summary>
        /// The category of this sound effect. Only used for display purposes.
        /// </summary>
        public string Category => _category;
        /// <summary>
        /// The selection of AudioClips to pick from when playing this sound effect.
        /// </summary>
        public AudioClip[] Clips => _clips;
        /// <summary>
        /// Should this sound's volume be randomized?
        /// </summary>
        /// <remarks>
        /// If true, sampled volume will be between <c>VolumeMin</c> and <c>VolumeMax</c>.
        /// If false, volume will always be equal to <c>VolumeMin</c>.
        /// </remarks>
        public bool RandomizeVolume => _randomizeVolume;
        /// <summary>
        /// The minimum volume to pick when playing this sound effect. If <c>RandomizeVolume</c> is false, the volume will always be set to this.
        /// </summary>
        /// <seealso cref="VolumeMax"/>
        /// <seealso cref="RandomizeVolume"/>
        public float VolumeMin => _volumeMin;
        /// <summary>
        /// The maximum volume to pick when playing this sound effect. If <c>RandomizeVolume</c> is set to true, volume will always be set to <c>VolumeMin</c> instead.
        /// </summary>
        /// <seealso cref="VolumeMin"/>
        /// <seealso cref="RandomizeVolume"/>
        public float VolumeMax => _volumeMax;
        /// <summary>
        /// Should this sound's pitch be randomized?
        /// </summary>
        /// <remarks>
        /// If true, sampled pitch will be between <c>PitchMin</c> and <c>PitchMax</c>.
        /// If false, pitch will always be equal to <c>PitchMin</c>.
        /// </remarks>
        public bool RandomizePitch => _randomizePitch;
        /// <summary>
        /// The minimum pitch to pick when playing this sound effect. If <c>RandomizePitch</c> is false, the pitch will always be set to this.
        /// </summary>
        /// <seealso cref="PitchMax"/>
        /// <seealso cref="RandomizePitch"/>
        public float PitchMin => _pitchMin;
        /// <summary>
        /// The maximum pitch to pick when playing this sound effect. If <c>RandomizePitch</c> is set to true, volume will always be set to <c>PitchMin</c> instead.
        /// </summary>
        /// <seealso cref="PitchMin"/>
        /// <seealso cref="RandomizePitch"/>
        public float PitchMax => _pitchMax;

        /// <summary>
        /// Samples data from this definition, based on its settings, to play from an AudioSource.
        /// </summary>
        /// <returns>A tuple containing a selected audio clip, volume, and pitch.</returns>
        public (AudioClip clip, float volume, float pitch) Sample()
        {
            AudioClip clip = _clips.Length == 0 ? null : _clips[Random.Range(0, _clips.Length)];
            float pitch = _randomizePitch ? Random.Range(_pitchMin, _pitchMax) : _pitchMin;
            float volume = _randomizeVolume ? Random.Range(_volumeMin, _volumeMax) : _volumeMin;
            AudioClip theclip = null;
            SFXDefinition a = new(theclip);
            return (clip, volume, pitch);
        }

        /// <summary>
        /// Creates an empty SFXDefinition.
        /// </summary>
        public SFXDefinition() { }
        /// <summary>
        /// Creates an empty SFXDefinition with the specified name and category.
        /// </summary>
        public SFXDefinition(string name, string category)
        {
            _name = name;
            _category = category;
        }
        /// <summary>
        /// Creates a SFXDefinition from the specified clip(s)
        /// </summary>
        /// <param name="clips">The clip(s) to pick from when playing this sound effect.</param>
        public SFXDefinition(params AudioClip[] clips)
        {
            _clips = clips;
            _randomizePitch = false;
            _randomizeVolume = false;
        }
        /// <summary>
        /// Creates a SFXDefinition with the specified clip and data.
        /// </summary>
        /// <param name="clip">The audio clip to use for this sound effect.</param>
        /// <param name="volume">The volume to use for this sound effect.</param>
        /// <param name="pitch">The pitch to use for this sound effect.</param>
        /// <param name="name">The name of this sound effect.</param>
        public SFXDefinition(AudioClip clip, float volume = 1, float pitch = 1, string name = null)
        {
            _clips = new AudioClip[] { clip };
            _name = name;
            _volumeMax = volume;
            _volumeMin = volume;
            _pitchMax = pitch;
            _pitchMin = pitch;
            _randomizePitch = false;
            _randomizeVolume = false;
        }
        /// <summary>
        /// Creates a SFXDefinition with the specified clips and data.
        /// </summary>
        /// <param name="clips">The audio clips to pick from when playing this sound effect.</param>
        /// <param name="volume">The volume to use for this sound effect.</param>
        /// <param name="pitch">The pitch to use for this sound effect.</param>
        /// <param name="name">The name of this sound effect.</param>
        public SFXDefinition(ICollection<AudioClip> clips, float volume = 1, float pitch = 1, string name = null)
        {
            _clips = clips.ToArray();
            _name = name;
            _volumeMax = volume;
            _volumeMin = volume;
            _pitchMax = pitch;
            _pitchMin = pitch;
            _randomizePitch = false;
            _randomizeVolume = false;
        }
        /// <summary>
        /// Creates a SFXDefinition with the specified clips and randomized data.
        /// </summary>
        /// <param name="clips">The audio clips to pick from when playing this sound effect.</param>
        /// <param name="volumeMin">The minimum volume that can be picked for this sound effect.</param>
        /// <param name="volumeMax">The maximum volume that can be picked for this sound effect.</param>
        /// <param name="pitchMin">The minimum pitch that can be picked for this sound effect.</param>
        /// <param name="pitchMax">The maximum pitch that can be picked for this sound effect.</param>
        /// <param name="name">The name of this sound effect.</param>
        public SFXDefinition(ICollection<AudioClip> clips, float volumeMin, float volumeMax, float pitchMin, float pitchMax, string category = null, string name = null)
        {
            _clips = clips.ToArray();
            _name = name;
            _category = category;
            _volumeMax = volumeMax;
            _volumeMin = volumeMin;
            _pitchMax = pitchMax;
            _pitchMin = pitchMin;
            _randomizePitch = true;
            _randomizeVolume = true;
        }
    }
}
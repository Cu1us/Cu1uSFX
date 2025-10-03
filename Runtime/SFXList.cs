using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cu1uSFX.Internal
{
    public class SFXList : ScriptableObject
    {
        public const string SINGLETON_ASSET_NAME = "SFX List";

        static SFXList _instance;
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
                Debug.Log($"[Cu1uSFX] Created a new SFX List asset at '/Assets/Resources/{SINGLETON_ASSET_NAME}'!", _instance);
#endif
            }
#if UNITY_EDITOR
            MakeSureSFXEnumScriptPathIsValid();
#endif
        }

#if UNITY_EDITOR
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

                // TextAsset a = new("hello");
                // UnityEditor.AssetDatabase.CreateAsset(new UnityEditor.MonoScript(), $"Assets/Scripts/SFXEnum.cs");
                return false;
            }
            return true;
        }
#endif

        public static string GetNameOfSFX(PredefinedSFX sfx)
        {
            if (!IsValidSFX(sfx))
                return null;
            return _instance.Definitions[sfx.IndexInSFXList.Value].Name;
        }
        public static string GetNameOfSFX(ushort sfx)
        {
            if (sfx >= Instance.Definitions.Length)
                return null;
            return _instance.Definitions[sfx].Name;
        }
        public static PredefinedSFX GetSFXByName(string name)
        {
            for (ushort i = 0; i < Instance.Definitions.Length; i++)
            {
                if (_instance.Definitions[i].Name == name)
                    return new(i);
            }
            return PredefinedSFX.NullValue;
        }
        public static bool HasSFXWithName(string name)
        {
            for (ushort i = 0; i < Instance.Definitions.Length; i++)
            {
                if (_instance.Definitions[i].Name == name)
                    return true;
            }
            return false;
        }
        internal static ushort? GetSFXIDByName(string name)
        {
            for (ushort i = 0; i < Instance.Definitions.Length; i++)
            {
                if (_instance.Definitions[i].Name == name)
                    return i;
            }
            return null;
        }
        public static bool IsValidSFX(PredefinedSFX sfx) => sfx.IndexInSFXList != null && sfx.IndexInSFXList.Value < Instance.Definitions.Length;
        public static bool IsValidSFX(ushort sfx) => sfx != ushort.MaxValue && sfx < Instance.Definitions.Length;
        public static SFXDefinition GetDefinitionOfSFX(PredefinedSFX sfx)
        {
            return IsValidSFX(sfx) ? Instance.Definitions[sfx.IndexInSFXList.Value] : null;
        }
        public static SFXDefinition GetDefinitionOfSFX(ushort sfx)
        {
            return IsValidSFX(sfx) ? Instance.Definitions[sfx] : null;
        }

        public List<string> EnumNames = new();
        public SFXDefinition[] Definitions;
#if UNITY_EDITOR
        public string SFXEnumScriptPath = "Assets/Scripts/SFXEnum.cs";
#endif
    }

    [Serializable]
    public record SFXDefinition
    {
        [SerializeField] string _name;
        [SerializeField] string _category;
        [SerializeField] AudioClip[] _clips;
        [SerializeField] bool _randomizeVolume;
        [SerializeField] float _volumeMin = 1;
        [SerializeField] float _volumeMax = 1;
        [SerializeField] bool _randomizePitch;
        [SerializeField] float _pitchMin = 1;
        [SerializeField] float _pitchMax = 1;

        public string Name => _name;
        public string Category => _category;
        public AudioClip[] Clips => _clips;
        public bool RandomizeVolume => _randomizeVolume;
        public float VolumeMin => _volumeMin;
        public float VolumeMax => _volumeMax;
        public bool RandomizePitch => _randomizePitch;
        public float PitchMin => _pitchMin;
        public float PitchMax => _pitchMax;

        public (AudioClip clip, float volume, float pitch) Sample()
        {
            AudioClip clip = _clips.Length == 0 ? null : _clips[Random.Range(0, _clips.Length)];
            float pitch = _randomizePitch ? Random.Range(_pitchMin, _pitchMax) : _pitchMin;
            float volume = _randomizeVolume ? Random.Range(_volumeMin, _volumeMax) : _volumeMin;
            return (clip, volume, pitch);
        }

        public SFXDefinition() { }
        public SFXDefinition(string name, string category)
        {
            _name = name;
            _category = category;
        }
        public SFXDefinition(string name = null, float volume = 1, float pitch = 1, params AudioClip[] clips)
        {
            _clips = clips;
            _name = name;
            _volumeMax = volume;
            _volumeMin = volume;
            _pitchMax = pitch;
            _pitchMin = pitch;
            _randomizePitch = false;
            _randomizeVolume = false;
        }
        public SFXDefinition(string name = null, float volumeMin = 1, float volumeMax = 1, float pitchMin = 1, float pitchMax = 1, params AudioClip[] clips)
        {
            _clips = clips;
            _name = name;
            _volumeMax = volumeMax;
            _volumeMin = volumeMin;
            _pitchMax = pitchMax;
            _pitchMin = pitchMin;
            _randomizePitch = true;
            _randomizeVolume = true;
        }
    }
}
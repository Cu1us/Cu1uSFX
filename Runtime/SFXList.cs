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
                Debug.Log($"Cu1uSFX: Created a new SFX List asset at /Assets/Resources/{SINGLETON_ASSET_NAME}");
#endif
            }
        }

        public List<string> EnumNames = new();
        public SFXDefinition[] Definitions;
    }

    [Serializable]
    public record SFXDefinition
    {
        public string Name;
        public string Category;
        public AudioClip[] Clips;
        public bool RandomizeVolume;
        public float VolumeMin = 1;
        public float VolumeMax = 1;
        public bool RandomizePitch;
        public float PitchMin = 1;
        public float PitchMax = 1;

        public (AudioClip clip, float volume, float pitch) Sample()
        {
            AudioClip clip = Clips.Length == 0 ? null : Clips[Random.Range(0, Clips.Length)];
            float volume = RandomizePitch ? Random.Range(PitchMin, PitchMax) : PitchMin;
            float pitch = RandomizeVolume ? Random.Range(VolumeMin, VolumeMax) : VolumeMin;
            return (clip, volume, pitch);
        }
    }
}
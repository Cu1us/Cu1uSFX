using System;
using Cu1uSFX.Internal;
using UnityEngine;

namespace Cu1uSFX
{
    [Serializable]
    public struct PredefinedSFX : ISerializationCallbackReceiver
    {
        [SerializeField] ushort _indexInSFXList;
        internal readonly ushort? IndexInSFXList => _indexInSFXList == ushort.MaxValue ? null : _indexInSFXList;
        [SerializeField, HideInInspector] string SerializedName;

        public readonly static PredefinedSFX NullValue = new(null);

        public readonly SFXDefinition Definition => SFXList.GetDefinitionOfSFX(this);
        public readonly string Name => Definition?.Name;

        public PredefinedSFX(ushort indexInSFXList)
        {
            _indexInSFXList = indexInSFXList;
            SerializedName = SFXList.GetNameOfSFX(indexInSFXList);
        }
        private PredefinedSFX(string serializedName = null)
        {
            _indexInSFXList = ushort.MaxValue;
            SerializedName = serializedName;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() => VerifyAndRepairValidity();
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(SerializedName))
            {
                _indexInSFXList = ushort.MaxValue;
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += HandleAfterDeserialize;
#endif
        }

#if UNITY_EDITOR
        void HandleAfterDeserialize()
        {
            UnityEditor.EditorApplication.update -= HandleAfterDeserialize;
            VerifyAndRepairValidity();
        }
#endif

        void VerifyAndRepairValidity()
        {
            if (SFXList.IsValidSFX(_indexInSFXList))
            {
                // Index is valid
                if (string.IsNullOrEmpty(SerializedName))
                {
                    // Name is null
                    // Fetch name from index
                    SerializedName = SFXList.GetDefinitionOfSFX(_indexInSFXList).Name;
                }
                else if (!SFXList.HasSFXWithName(SerializedName))
                {
                    // Name exists but is invalid
                    // Null both, sfx was probably deleted
                    SerializedName = null;
                    _indexInSFXList = ushort.MaxValue;
                }
                else if (SerializedName != SFXList.GetDefinitionOfSFX(_indexInSFXList).Name)
                {
                    // Name exists but doesn't match up with index
                    // Fetch index from name, sfx was probably reordered
                    _indexInSFXList = SFXList.GetSFXIDByName(SerializedName) ?? ushort.MaxValue;
                    if (_indexInSFXList == ushort.MaxValue)
                    {
                        // Could not fetch index from name - this should never happen as HasSFXWithName returned true earlier
                        // Null both as a failsafe
                        _indexInSFXList = ushort.MaxValue;
                        SerializedName = null;
                    }
                }
                // else, we are good to go (both index and name are non-null and valid and they match up)
            }
            else
            {
                // Index is invalid
                if (!string.IsNullOrEmpty(SerializedName))
                {
                    // Name is non-null
                    // Attempt to get index from name
                    _indexInSFXList = SFXList.GetSFXIDByName(SerializedName) ?? ushort.MaxValue;
                    if (_indexInSFXList == ushort.MaxValue)
                    {
                        // Name was invalid - sfx was probably deleted
                        // Null both
                        SerializedName = null;
                    }
                }
                // else, we are good to go (both are null - this is an empty sfx)
            }
        }

        public readonly override string ToString()
        {
            return $"[SFX '{Name}']";
        }

        public static explicit operator ushort?(PredefinedSFX sfx) => sfx.IndexInSFXList;
    }
}
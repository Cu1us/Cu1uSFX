using System;
using Cu1uSFX.Internal;
using UnityEngine;

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
    /// A sound effect that you have defined in the SFX List in the editor. Can be easily edited in the inspector, if serialized.
    /// </summary>
    [Serializable]
    public struct PredefinedSFX : ISerializationCallbackReceiver
    {
        [SerializeField] ushort _indexInSFXList;
        internal readonly ushort? IndexInSFXList => _indexInSFXList == ushort.MaxValue ? null : _indexInSFXList;
        [SerializeField, HideInInspector] string SerializedName;

        /// <summary>
        /// An empty sound effect.
        /// </summary>
        public readonly static PredefinedSFX NullValue = new(null);

        /// <summary>
        /// Contains the data of this sound effect; audio clips, pitch, volume, and randomization options.
        /// </summary>
        public readonly SFXDefinition Definition => SFXList.GetDefinitionOfSFX(this);
        /// <summary>
        /// The name of this sound effect, as defined in the Sound Effects menu.
        /// </summary>
        public readonly string Name => Definition?.Name;

        /// <summary>
        /// Creates a new PredefinedSFX object with a reference to the sound effect defined at a specific index in the Sound Effects menu.
        /// </summary>
        /// <remarks>
        /// Should only be used if you for whatever reason need a reference to the sound effect at a specific index in the SFX list. Even then,
        /// you should probably be using <c>SFXList.Instance.Definitions[i]</c> instead (unless you need to serialize this reference, in which case, do use this constructor).
        /// </remarks>
        /// <param name="indexInSFXList">The index of the sound effect to reference. Make sure it is not out of bounds.</param>
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
            // Cannot perform the necessary Load calls to access the SFXList instance during serialization - defer it to the next editor tick.
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

        /// <summary>
        /// Called on serialize/deserialize to make sure the stored index and name are valid and match up. If not, try to recover it. 
        /// If the SFX List is reordered or modified, this will use the stored SFX name to try to find the new index of the stored sound to preserve validity.
        /// </summary>
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

        public readonly override string ToString() => $"[SFX '{Name}']";
        public static explicit operator ushort?(PredefinedSFX sfx) => sfx.IndexInSFXList;
    }
}
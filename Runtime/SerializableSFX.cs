using System;
using UnityEngine;

namespace Cu1uSFX
{
    [Serializable]
    public class SerializableSFX : ISerializationCallbackReceiver
    {
        [SerializeField] string SFXName;

        [NonSerialized] private SFX value;
        [field: NonSerialized] public bool IsValid { get; private set; }

        public SFX Value
        {
            get => value;
            set
            {
                string newSFXName = Enum.GetName(typeof(SFX), value);
                if (string.IsNullOrEmpty(SFXName))
                {
                    throw new ArgumentException("[Cu1uSFX] Invalid SFX value passed when setting SerializableSFX value.");
                }
                else
                {
                    this.value = value;
                    SFXName = newSFXName;
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            IsValid = Enum.TryParse(SFXName, out value);
            if (!IsValid)
            {
                value = (SFX)(-1);
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            SFXName = Enum.GetName(typeof(SFX), value);
        }
        public SFX ToSFX()
        {
            if (!IsValid)
            {
                throw new ArgumentException($"[Cu1uSFX] The SerializedSFX references a SFX '{SFXName}' that no longer exists.");
            }
            return value;
        }

        #region Play functions

        public SFXReference Play(float volume = 1, float pitch = 1)
        {
            return value.Play(volume, pitch);
        }
        public SFXReference Play(Vector3 worldPosition, float volume = 1, float pitch = 1)
        {
            return value.Play(worldPosition, volume, pitch);
        }
        public SFXReference Play(Transform followTransform, float volume = 1, float pitch = 1)
        {
            return value.Play(followTransform, volume, pitch);
        }
        public SFXReference Play(Transform followTransform, Vector3 followOffset, float volume = 1, float pitch = 1)
        {
            return value.Play(followTransform, followOffset, volume, pitch);
        }

        #endregion


        public SerializableSFX(SFX sfx)
        {
            value = sfx;
            SFXName = Enum.GetName(typeof(SFX), sfx);
            IsValid = !string.IsNullOrEmpty(SFXName);
        }

        public static implicit operator SFX(SerializableSFX ssfx) => ssfx.ToSFX();
        public static explicit operator SerializableSFX(SFX sfx) => new(sfx);
    }
}
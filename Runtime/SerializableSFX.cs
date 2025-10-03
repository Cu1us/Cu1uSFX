using System;
using UnityEngine;
using Cu1uSFX.Internal;

// namespace Cu1uSFX
// {
//     [Serializable]
//     public class SerializableSFX : ISerializationCallbackReceiver
//     {
//         [SerializeField] string SFXName;
//         [NonSerialized] private PredefinedSFX value;

//         public PredefinedSFX Value
//         {
//             get => value;
//             set
//             {
//                 if (SFXList.IsValidSFX(value))
//                 {
//                     this.value = value;
//                 }
//                 else
//                 {
//                     throw new ArgumentException("[Cu1uSFX] Invalid SFX value passed when setting SerializableSFX value.");
//                 }
//             }
//         }

//         void ISerializationCallbackReceiver.OnAfterDeserialize()
//         {
//             value = SFXList.GetSFXByName(SFXName) ?? PredefinedSFX.NullValue;
//         }
//         void ISerializationCallbackReceiver.OnBeforeSerialize()
//         {
//             SFXName = SFXList.GetNameOfSFX(value);
//         }

//         #region Play functions

//         public SFXReference Play(float volume = 1, float pitch = 1)
//         {
//             return SFXPlayer.Play(value, volume, pitch);
//         }
//         public SFXReference Play(Vector3 worldPosition, float volume = 1, float pitch = 1)
//         {
//             return SFXPlayer.Play(value, worldPosition, volume, pitch);
//         }
//         public SFXReference Play(Transform followTransform, float volume = 1, float pitch = 1)
//         {
//             return SFXPlayer.Play(value, followTransform, volume, pitch);
//         }
//         public SFXReference Play(Transform followTransform, Vector3 followOffset, float volume = 1, float pitch = 1)
//         {
//             return SFXPlayer.Play(value, followTransform, followOffset, volume, pitch);
//         }

//         #endregion


//         public SerializableSFX(PredefinedSFX sfx)
//         {
//             value = SFXList.IsValidSFX(sfx) ? sfx : PredefinedSFX.NullValue;
//             SFXName = SFXList.GetNameOfSFX(sfx);
//         }

//         public static implicit operator PredefinedSFX(SerializableSFX ssfx) => ssfx.Value;
//         public static explicit operator SerializableSFX(PredefinedSFX sfx) => new(sfx);
//     }
// }
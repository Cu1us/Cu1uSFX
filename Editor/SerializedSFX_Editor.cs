using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Cu1uSFX.Internal
{
    [CustomPropertyDrawer(typeof(SerializableSFX))]
    public class SerializedSFX_Editor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializableSFX sfx = (SerializableSFX)property.boxedValue;

            string sfxName = property.FindPropertyRelative("SFXName").stringValue;
            
            if (!Enum.TryParse(sfxName, out SFX defaultValue))
            {
                defaultValue = 0;
            }

            EnumField enumField = new(property.displayName, defaultValue);
            enumField.RegisterValueChangedCallback((changeEvent) =>
            {
                string sfxName = Enum.GetName(typeof(SFX), changeEvent.newValue);
                SerializedProperty sfxNameProp = property.FindPropertyRelative("SFXName");
                sfxNameProp.stringValue = sfxName;
                property.serializedObject.ApplyModifiedProperties();
            });
            return enumField;
        }
    }
}
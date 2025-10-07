using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cu1uSFX.Internal
{
    /// <summary>
    /// Custom property drawer for making serialized instances of PredefinedSFX be editable through the inspector, via a dropdown menu that lets you 
    /// pick one of the defined sound effects in the project, ordered by category.
    /// </summary>
    [CustomPropertyDrawer(typeof(PredefinedSFX))]
    public class PredefinedSFX_Editor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            string selectedSFXName = property.FindPropertyRelative("SerializedName").stringValue;

            int optionCount = SFXList.Instance.Definitions.Length;
            List<string> choices = new(optionCount + 1)
            {
                "None"
            };

            int selectedChoice = 0;

            for (int i = 0; i < optionCount; i++)
            {
                ref SFXDefinition definition = ref SFXList.Instance.Definitions[i];

                string category = definition.Category;
                string name = definition.Name;

                if (name == selectedSFXName)
                    selectedChoice = i + 1;

                if (string.IsNullOrWhiteSpace(category))
                    choices.Add(name);
                else
                    choices.Add($"{category}/{name}");
            }

            DropdownField dropdownField = new(property.displayName, choices, selectedChoice);

            dropdownField.RegisterValueChangedCallback((changeEvent) =>
            {
                string sfxName;
                int sfxIndex;
                if (dropdownField.index == 0)
                {
                    sfxName = null;
                    sfxIndex = ushort.MaxValue;
                }
                else
                {
                    sfxIndex = dropdownField.index - 1;
                    sfxName = SFXList.Instance.Definitions[sfxIndex].Name;
                }
                SerializedProperty sfxNameProp = property.FindPropertyRelative("SerializedName");
                SerializedProperty sfxIndexProp = property.FindPropertyRelative("_indexInSFXList");
                sfxNameProp.stringValue = sfxName;
                sfxIndexProp.intValue = sfxIndex;
                property.serializedObject.ApplyModifiedProperties();
            });

            return dropdownField;
        }
    }
}
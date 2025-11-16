using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

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
    [CustomPropertyDrawer(typeof(SFXDefinition))]
    public class SFXDefinition_Editor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty clipsProp = property.FindPropertyRelative(nameof(SFXDefinition.Clips));

            VisualElement container = new()
            {
                style = { alignItems = Align.FlexStart, flexDirection = FlexDirection.Row }
            };

            Label label = new()
            {
                text = property.displayName,
                style = { marginLeft = 3, paddingTop = 3, width = Length.Percent(67) }
            };

            Button editButton = new(() =>
            {
                SFX_EditSFXWindow_Editor.Spawn(property, property.displayName, true, property.serializedObject.targetObject.name);
            })
            {
                text = "Edit",
                style = { width = Length.Percent(33) }
            };

            container.Add(label);
            container.Add(editButton);

            return container;
        }
    }
}
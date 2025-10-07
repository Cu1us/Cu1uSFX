using UnityEngine;
using UnityEditor;

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
    /// The inspector for the SFX List ScriptableObject.
    /// </summary>
    [CustomEditor(typeof(SFXList))]
    public class SFXList_Editor : Editor
    {
        bool showAdvanced = false;
        MonoScript selectedEnumScript;
        void OnEnable()
        {
            if (AssetDatabase.AssetPathExists(SFXList.Instance.SFXEnumScriptPath))
            {
                selectedEnumScript = AssetDatabase.LoadAssetAtPath(SFXList.Instance.SFXEnumScriptPath, typeof(MonoScript)) as MonoScript;
            }
        }
        public override void OnInspectorGUI()
        {
            SFXList sfxList = (SFXList)serializedObject.targetObject;
            if (sfxList != SFXList.Instance)
            {
                EditorGUILayout.LabelField("Invalid SFX List object.");
                EditorGUILayout.LabelField($"To load correctly, a SFXList asset named '{SFXList.SINGLETON_ASSET_NAME}' should exist inside the Resources folder.");
                return;
            }

            // Open editor button
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Open SFX Editor"))
            {
                SFX_Window_Editor.Spawn();
            }
            GUIStyle smallTextStyle = new(GUI.skin.label);
            smallTextStyle.fontSize -= 2;
            EditorGUILayout.LabelField("The SFX editor can also be opened using the Window/SFX Editor tab.", smallTextStyle);
            EditorGUILayout.Space(20);

            // Log settings
            EditorGUILayout.LabelField(new GUIContent("Log settings", "When should the Cu1uSFX plugin send log messages?"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SFXList.LogFlags)), GUIContent.none);
            EditorGUILayout.Space(10);

            // Audio Source object pool settings
            EditorGUILayout.LabelField(new GUIContent(
                "Audio Source object pool", 
                "AudioSources are pooled to optimize performance: instead of destroying sources when they're finished playing, they're disabled and reused later." +
                "\n\nIf you plan on playing a lot of concurrent sounds, make sure these values are appropriate."
            ));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(SFXList.AudioSourcePoolDefault)),
                new GUIContent("Default", "Starting size of the pool.\n\nWhen the game starts, this many AudioSources will be prepared in advance.\n\n[Default = 3]")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(SFXList.AudioSourcePoolMax)), 
                new GUIContent("Max", "Maximum size of the pool.\n\nIf the pool is full, sources above the max count will be destroyed instead of put back in the pool.\n\n[Default = 10]")
            );

            // Advanced tab
            EditorGUILayout.Space(20);
            showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced");
            if (showAdvanced)
            {
                GUIStyle style = new(GUI.skin.label);
                style.fontSize -= 2;
                style.normal.textColor = style.normal.textColor * new Color(1f, 1f, 0.7f);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Change the script that the SFX enum is generated in here.", style);
                selectedEnumScript = EditorGUILayout.ObjectField(new GUIContent("Enum generation target"), selectedEnumScript, typeof(MonoScript), false) as MonoScript;
                EditorGUILayout.LabelField("WARNING: The contents of the selected script will be permanently overwritten!", style);
                EditorGUILayout.LabelField("NOTE: The previous selected script must be deleted if this", style);
                EditorGUILayout.LabelField("is changed, to prevent duplicate definitions of the SFX enum.", style);
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("To change this safely:", style);
                EditorGUILayout.LabelField("- Create a new empty script at your desired location", style);
                EditorGUILayout.LabelField("- Assign that script to the field above", style);
                EditorGUILayout.LabelField("- Delete the script that was previously assigned (default is Assets/Scripts/SFXEnum.cs)", style);
                EditorGUILayout.LabelField("Done! Now all SFX enums will be generated in the new script instead.", style);
                if (EditorGUI.EndChangeCheck() && selectedEnumScript != null)
                {
                    string newPath = AssetDatabase.GetAssetPath(selectedEnumScript);
                    sfxList.SFXEnumScriptPath = newPath;
                    if (SFXList.MakeSureSFXEnumScriptPathIsValid())
                    {
                        SFXList.LogIfFlag(SFXLogFlags.NOTIF_INFO, $"[Cu1uSFX] Changed enum generation target to '{newPath}'.");
                        SFXEnumGenerator.GenerateEnumScript();
                        SFXEnumGenerator.RecompileScripts();
                    }
                }
            }
        }
    }
}
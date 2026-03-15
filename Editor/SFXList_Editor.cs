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
        bool showGenOptions = false;
        bool showDebug = false;
        bool enableCodeGen;
        bool enableCategoryCodeGen;
        MonoScript selectedEnumScript;
        MonoScript storedEnumScript;
        void OnEnable()
        {
            if (AssetDatabase.AssetPathExists(SFXList.Instance.SFXEnumScriptPath))
            {
                storedEnumScript = AssetDatabase.LoadAssetAtPath(SFXList.Instance.SFXEnumScriptPath, typeof(MonoScript)) as MonoScript;
                selectedEnumScript = storedEnumScript;
            }
            enableCodeGen = SFXList.Instance.EnableCodeGeneration;
            enableCategoryCodeGen = SFXList.Instance.CategorizeSFXEnum;
        }
        public override void OnInspectorGUI()
        {
            SFXList sfxList = (SFXList)serializedObject.targetObject;
            if (sfxList != SFXList.Instance)
            {
                EditorGUILayout.HelpBox($"Invalid SFX List object.\nFor the plugin to function, a SFXList asset named '{SFXList.SINGLETON_ASSET_NAME}' must exist under a Resources folder.", MessageType.Error);
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Sound effects: {sfxList.Definitions.Length}", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, fontSize = 14 });

            // Open editor button
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Open SFX Editor", new GUIStyle(GUI.skin.button) { margin = new RectOffset(60, 60, 0, 0), fixedHeight = 30 }))
            {
                SFX_Window_Editor.Spawn();
            }
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("The SFX editor can also be opened using the Window/SFX Editor tab.", MessageType.Info);
            EditorGUILayout.Space(20);

            GUIStyle headerStyle = new(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold
            };


            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.LabelField(new GUIContent("General settings"), headerStyle);

                // Highlight unsaved SFX setting
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(nameof(SFXList.HighlightUnsavedSFXsInList)),
                    new GUIContent("Highlight unsaved SFX in list", "Should unsaved sound effects in the SFX list be marked with a '*'?\n\n" +
                    "Disabling this saves performance if there's a large numbers of sound effects." +
                    "\n\n(This setting has no effect if code generation is disabled)\n\n[Default: enabled]")
                );
                // Log messages setting
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SFXList.LogFlags)), new GUIContent("Log messages", "When should the Cu1uSFX plugin send log messages?"));
                EditorGUILayout.Space(10);

                // Audio Source object pool settings
                EditorGUILayout.LabelField(new GUIContent(
                    "Audio Source pooling",
                    "AudioSources are pooled to optimize performance: instead of destroying sources when they're finished playing, they're disabled and reused later." +
                    "\n\nIf you plan on playing a lot of concurrent sounds, make sure these values are appropriate."
                ), headerStyle);
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(nameof(SFXList.AudioSourcePoolDefault)),
                    new GUIContent("Prewarm", "Starting size of the pool.\n\nWhen the game starts, this many AudioSources will be prepared in advance.\n\n[Default = 0]")
                );
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(nameof(SFXList.AudioSourcePoolMax)),
                    new GUIContent("Max", "Maximum size of the pool.\n\nIf the pool is full, sources above the max count will be destroyed instead of put back in the pool.\n\n[Default = 10]")
                );
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.Space(10);

            showGenOptions = EditorGUILayout.Foldout(showGenOptions, "Code generation");
            if (showGenOptions)
            {
                EditorGUI.indentLevel++;
                enableCodeGen = EditorGUILayout.Toggle(
                    new GUIContent(
                        "Enable code generation",
                        "If enabled, the plugin will generate a script containing a static SFX enum class, that lets you access your project-wide sound effects"
                        + " directly using 'SFX.YourSound' anywhere in the code. Disabling this will delete that script, if it exists.\n\n[Default: enabled]"),
                    enableCodeGen
                );
                EditorGUI.BeginDisabledGroup(!enableCodeGen);
                enableCategoryCodeGen = EditorGUILayout.Toggle(
                    new GUIContent(
                        "Categorize SFX enum",
                        "If enabled, the static SFX enum will sort sound effects by their category, meaning you must access sound effects that have a defined "
                        + "category using 'SFX.YourCategory.YourSound' instead of just 'SFX.YourSound'.\n\n[Default: disabled]"
                ),
                enableCategoryCodeGen
                );
                EditorGUI.EndDisabledGroup();

                // Advanced tab
                showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced");
                if (showAdvanced)
                {
                    EditorGUI.indentLevel++;
                    GUIStyle style = new(GUI.skin.label);
                    style.fontSize -= 2;
                    style.normal.textColor = style.normal.textColor * new Color(1f, 1f, 0.7f);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.HelpBox("Changing the target script for enum generation will delete the previous script asset, and overwrite the contents of the new script you select. This cannot be undone!", MessageType.Warning);
                    EditorGUI.BeginDisabledGroup(!enableCodeGen);
                    selectedEnumScript = EditorGUILayout.ObjectField(new GUIContent("Enum generation target"), selectedEnumScript, typeof(MonoScript), false) as MonoScript;
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space(5);
                    showDebug = EditorGUILayout.Foldout(showDebug, "Debug");
                    if (showDebug)
                    {
                        EditorGUI.indentLevel++;
                        GUIStyle buttonStyle = new(GUI.skin.button) { margin = new RectOffset(60, 30, 0, 0) };
                        EditorGUILayout.Space(10);

                        if (GUILayout.Button("Regenerate SFX enum script immediately", buttonStyle))
                        {
                            SFXList.LogIfFlag(SFXLogFlags.NOTIF_INFO, "[Cu1uSFX] Regenerating SFX enum without recompiling...");
                            SFXEnumGenerator.GenerateEnumScript(enableCategoryCodeGen);
                        }
                        EditorGUILayout.Space(5);
                        if (GUILayout.Button("Delete SFX enum script", buttonStyle))
                        {
                            SFXList.LogIfFlag(SFXLogFlags.NOTIF_INFO, "[Cu1uSFX] Deleting SFX enum script...");
                            SFXEnumGenerator.DeleteEnumScript();
                        }
                        EditorGUILayout.Space(5);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space(5);



                bool changedGenerationTarget = selectedEnumScript != null && selectedEnumScript != storedEnumScript;

                bool changed = enableCodeGen != sfxList.EnableCodeGeneration || enableCategoryCodeGen != sfxList.CategorizeSFXEnum || changedGenerationTarget;

                EditorGUI.BeginDisabledGroup(!changed);
                if (GUILayout.Button("Save and Recompile", new GUIStyle(GUI.skin.button) { margin = new RectOffset(30, 0, 0, 0) }))
                {
                    sfxList.CategorizeSFXEnum = enableCategoryCodeGen;
                    if (enableCodeGen)
                    {
                        sfxList.EnableCodeGeneration = true;
                        if (changedGenerationTarget)
                        {
                            SFXEnumGenerator.DeleteEnumScript();
                            string newPath = AssetDatabase.GetAssetPath(selectedEnumScript);
                            sfxList.SFXEnumScriptPath = newPath;
                            storedEnumScript = selectedEnumScript;
                            if (SFXList.MakeSureSFXEnumScriptPathIsValid())
                            {
                                SFXList.LogIfFlag(SFXLogFlags.NOTIF_INFO, $"[Cu1uSFX] Changed enum generation target to '{newPath}'.");
                            }
                        }
                        SFXEnumGenerator.GenerateEnumScript(enableCategoryCodeGen);
                        // SFXEnumGenerator.RecompileScripts();
                    }
                    else if (sfxList.EnableCodeGeneration) // Code generation was set from true to false
                    {
                        sfxList.EnableCodeGeneration = false;
                        SFXEnumGenerator.DeleteEnumScript();
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
        }

        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();
            Rect position = new(0, 0, EditorGUIUtility.currentViewWidth, 25);
            EditorGUI.LabelField(position, new GUIContent($"[ Cu1uSFX v.{SFXList.PLUGIN_VERSION} ]"), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 13 });
        }
    }
}
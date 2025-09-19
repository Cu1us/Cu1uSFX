using UnityEngine;
using UnityEditor;

namespace Cu1uSFX.Internal
{
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
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Open SFX Editor"))
            {
                SFX_Window_Editor.Spawn();
            }
            EditorGUILayout.LabelField("The SFX editor can also be opened using the Window/SFX Editor tab.");
            EditorGUILayout.Space(20);
            showAdvanced = EditorGUILayout.BeginFoldoutHeaderGroup(showAdvanced, "Advanced");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Change the script that the SFX enum is generated in here.");
            selectedEnumScript = EditorGUILayout.ObjectField("Enum generation target", selectedEnumScript, typeof(MonoScript), false) as MonoScript;
            EditorGUILayout.LabelField("WARNING: The contents of the selected script will be permanently overwritten!");
            EditorGUILayout.LabelField("If you change this, you must also delete the previous script to avoid naming conflicts.");
            if (EditorGUI.EndChangeCheck() && selectedEnumScript != null)
            {
                string newPath = AssetDatabase.GetAssetPath(selectedEnumScript);
                sfxList.SFXEnumScriptPath = newPath;
                if (SFXList.MakeSureSFXEnumScriptPathIsValid())
                {
                    Debug.Log($"[Cu1uSFX] Changed enum generation target to '{newPath}'.");
                    SFXEnumGenerator.GenerateEnumScript();
                    SFXEnumGenerator.RecompileScripts();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
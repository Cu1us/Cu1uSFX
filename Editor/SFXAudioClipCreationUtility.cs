using UnityEngine;
using UnityEditor;
using Cu1uSFX.Internal;

public static class SFXAudioClipCreationUtility
{
    [MenuItem("Assets/Create/SFX from selected clips %#a")]
    public static void CreateSFXFromClips()
    {
        AudioClip[] clips = Selection.GetFiltered<AudioClip>(SelectionMode.Assets);
        if (clips.Length == 0) return;
        SerializedObject sfxList = new(SFXList.Instance);
        SerializedProperty definitionsProp = sfxList.FindProperty(nameof(SFXList.Definitions));
        SFX_NewSFXWindow_Editor.Spawn(definitionsProp, "", clips, showListWhenCompleted: true);
    }

    [MenuItem("Assets/Create/SFX from selected clips %#a", true)]
    public static bool ValidateCreateSFXFromClips()
    {
        return Selection.count > 0 && Selection.GetFiltered<AudioClip>(SelectionMode.Assets).Length > 0;
    }
}

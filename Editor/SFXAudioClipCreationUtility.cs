using UnityEngine;
using UnityEditor;
using Cu1uSFX.Internal;

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

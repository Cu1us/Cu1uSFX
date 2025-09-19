using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cu1uSFX.Internal
{
    public class SFX_Window_Editor : EditorWindow
    {
        TabView CategoryTabView;
        Button SaveChangesButton;
        int SelectedTabIndex = 0;
        readonly Dictionary<string, ScrollView> CategoryTabMap = new();

        VisualElement GenerateRootContent()
        {
            CategoryTabMap.Clear();
            VisualElement content = new();

            SerializedObject sfxList = new(SFXList.Instance);

            SerializedProperty definitionsProp = sfxList.FindProperty(nameof(SFXList.Definitions));

            if (definitionsProp == null)
            {
                Debug.LogError("[Cu1uSFX] Error: Sounds array is null");
                return content;
            }

            CategoryTabView = new()
            {
                name = "Categories",
            };

            Tab allTab = new("All");
            CategoryTabView.Add(allTab);


            if (definitionsProp.arraySize == 0)
            {
                Label emptyCategoryLabel = new("No sound effects have been added yet.")
                {
                    style = { alignSelf = Align.Center, marginTop = 20, marginLeft = 10, marginRight = 10 }
                };
                allTab.Add(emptyCategoryLabel);
            }
            else
            {
                ScrollView allTabScrollView = new(ScrollViewMode.Vertical);
                allTab.Add(allTabScrollView);
                for (int i = 0; i < definitionsProp.arraySize; i++)
                {
                    SerializedProperty sfx = definitionsProp.GetArrayElementAtIndex(i);
                    string sfxCategory = sfx.FindPropertyRelative(nameof(SFXDefinition.Category)).stringValue;

                    if (!string.IsNullOrWhiteSpace(sfxCategory))
                    {
                        if (!CategoryTabMap.TryGetValue(sfxCategory, out ScrollView tabView))
                        {
                            Tab tab = new(sfxCategory);
                            tabView = new(ScrollViewMode.Vertical);
                            tab.Add(tabView);
                            CategoryTabMap[sfxCategory] = tabView;
                            CategoryTabView.Add(tab);
                        }
                        tabView.Add(DisplaySFX(sfx, tabView.childCount));
                    }
                    allTabScrollView.Add(DisplaySFX(sfx, i));
                }
            }

            if (SelectedTabIndex < CategoryTabView.childCount)
                CategoryTabView.selectedTabIndex = SelectedTabIndex;

            content.Add(CategoryTabView);

            Button addNewSFXButton = new(OnAddNewButtonClicked)
            {
                text = "Add new",
                tooltip = "Adds a new sound effect to the current category.",
                style = { marginTop = 20, marginLeft = 20, marginRight = 20, marginBottom = 5, alignSelf = Align.Center }
            };

            SaveChangesButton = new(SaveChanges)
            {
                text = "Save",
                tooltip = "Saves any added/removed sound effects, and regenerates the SFX enum.\n\nChanges to individual sound settings are saved automatically.",
                style = { marginTop = 5, marginLeft = 20, marginRight = 20, marginBottom = 10, alignSelf = Align.Center },
                enabledSelf = hasUnsavedChanges
            };
            content.Add(addNewSFXButton);
            content.Add(SaveChangesButton);

            return content;
        }

        void CreateGUI()
        {
            Regenerate();
        }

        void OnAddNewButtonClicked()
        {
            SerializedObject sfxList = new(SFXList.Instance);
            SerializedProperty definitionsProp = sfxList.FindProperty(nameof(SFXList.Definitions));
            string category = CategoryTabView.activeTab.label == "All" ? "" : CategoryTabView.activeTab.label;
            SFX_NewSFXWindow_Editor window = SFX_NewSFXWindow_Editor.Spawn(definitionsProp, category);
            window.OnObjectUpdated -= MarkUnsavedChangesAndRegenerate;
            window.OnObjectUpdated += MarkUnsavedChangesAndRegenerate;
        }

        VisualElement DisplaySFX(SerializedProperty sfxProp, int boxIndex)
        {
            VisualElement box = new Box()
            {
                style = { alignItems = Align.FlexEnd, flexDirection = FlexDirection.Row, marginTop = 2 }
            };

            string soundName = sfxProp.FindPropertyRelative(nameof(SFXDefinition.Name)).stringValue;
            Label label = new(soundName)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginLeft = 10, marginRight = 10, marginTop = 3, alignSelf = Align.FlexStart }
            };

            Button editButton = new(() =>
            {
                SFX_EditSFXWindow_Editor window = SFX_EditSFXWindow_Editor.Spawn(sfxProp, soundName);
                window.OnObjectUpdated -= Regenerate;
                window.OnObjectUpdated += Regenerate;
            })
            {
                text = "Edit",
                style = { width = 60 }
            };

            Button deleteButton = new(() =>
            {
                DeleteSfxByName(soundName);
                MarkUnsavedChangesAndRegenerate();
            })
            {
                text = "-",
                tooltip = $"Delete {soundName}",
                style = { width = 10, color = new(new Color(1f, 0.5f, 0.5f)), unityFontStyleAndWeight = FontStyle.Bold }
            };

            box.Add(deleteButton);
            box.Add(editButton);
            box.Add(label);

            return box;
        }

        bool DeleteSfxByName(string name)
        {
            SerializedObject sfxList = new(SFXList.Instance);

            SerializedProperty definitionsProp = sfxList.FindProperty(nameof(SFXList.Definitions));

            for (int i = 0; i < definitionsProp.arraySize; i++)
            {
                string propName = definitionsProp.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(SFXDefinition.Name)).stringValue;
                if (propName == name)
                {
                    definitionsProp.DeleteArrayElementAtIndex(i);
                    sfxList.ApplyModifiedProperties();

                    // Closes any open windows that are currently editing the SFX that we just deleted
                    if (HasOpenInstances<SFX_EditSFXWindow_Editor>())
                    {
                        SFX_EditSFXWindow_Editor windowToClose = GetWindow<SFX_EditSFXWindow_Editor>();
                        if (windowToClose != null && windowToClose.SoundName == name)
                            windowToClose.Close();
                    }
                    return true;
                }
            }
            return false;
        }

        void Regenerate()
        {
            if (CategoryTabView != null)
                SelectedTabIndex = CategoryTabView.selectedTabIndex;
            rootVisualElement.Clear();
            rootVisualElement.Add(GenerateRootContent());
        }
        void MarkUnsavedChangesAndRegenerate()
        {
            hasUnsavedChanges = HasUnsavedChanges();
            SaveChangesButton.enabledSelf = hasUnsavedChanges;
            Regenerate();
        }
        bool HasUnsavedChanges()
        {
            // If the SFX SFXDefinitions list mismatches with the EnumNames list then there is unsaved data
            SerializedObject sfxList = new(SFXList.Instance);
            SerializedProperty definitionsProp = sfxList.FindProperty(nameof(SFXList.Definitions));
            List<string> desiredEnumNames = new();
            for (int i = 0; i < definitionsProp.arraySize; i++)
            {
                string name = definitionsProp.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(SFXDefinition.Name)).stringValue;
                desiredEnumNames.Add(name);
            }
            if (desiredEnumNames.Count != SFXList.Instance.EnumNames.Count)
                return true;
            for (int i = 0; i < desiredEnumNames.Count; i++)
            {
                if (desiredEnumNames[i] != SFXList.Instance.EnumNames[i])
                    return true;
            }
            return false;
        }

        public override void SaveChanges()
        {
            SaveChangesButton.enabledSelf = false;
            SFXEnumGenerator.GenerateEnumScript();
            base.SaveChanges();
            SFXEnumGenerator.RecompileScripts();
        }
        public override void DiscardChanges()
        {
            base.DiscardChanges();
        }

        [MenuItem("Window/SFX Editor")]
        public static void Spawn()
        {
            SFX_Window_Editor window = GetWindow<SFX_Window_Editor>();
            window.titleContent.text = "Sound Effects";
            window.minSize = new(100, 100);
            window.Regenerate();
        }
    }


    public class SFX_EditSFXWindow_Editor : EditorWindow
    {
        SerializedProperty SfxProperty;
        public string SoundName;
        public Action OnObjectUpdated;

        Button PreviewButton;
        Label PreviewButtonErrorLabel;

        AudioSource PreviewSoundSource;

        const float STOPPABLE_PREVIEW_MIN_LENGTH = 1f;

        public static SFX_EditSFXWindow_Editor Spawn(SerializedProperty sfxProp, string name)
        {
            SFX_EditSFXWindow_Editor window = GetWindow<SFX_EditSFXWindow_Editor>(true, $"Edit SFX", true);
            window.ShowUtility();
            window.Initialize(sfxProp, name);
            return window;
        }
        public void Initialize(SerializedProperty sfxProp, string name)
        {
            titleContent.text = $"Editing {name}";
            titleContent.tooltip = "SFX Editor";
            minSize = new(300, 300);
            rootVisualElement.Clear();
            SfxProperty = sfxProp;
            SoundName = name;
            rootVisualElement.Add(GenerateRootContent());
        }
        void OnGUI()
        {
            if (SfxProperty == null && this != null)
                Close();
        }
        void TickPreviewFinished()
        {
            if (PreviewSoundSource == null || PreviewButton == null)
            {
                EditorApplication.update -= TickPreviewFinished;
                return;
            }
            else if (PreviewSoundSource.isPlaying && PreviewSoundSource.clip.length / PreviewSoundSource.pitch >= STOPPABLE_PREVIEW_MIN_LENGTH)
            {
                PreviewButton.text = "Stop";
            }
            else
            {
                PreviewButton.text = "Preview";
                EditorApplication.update -= TickPreviewFinished;
            }
        }
        VisualElement GenerateRootContent()
        {
            ScrollView content = new(ScrollViewMode.Vertical);

            string sfxName = SfxProperty.FindPropertyRelative(nameof(SFXDefinition.Name)).stringValue;
            string categoryName = SfxProperty.FindPropertyRelative(nameof(SFXDefinition.Category)).stringValue;
            Label editingLabel = new($"{sfxName}")
            {
                style = { alignSelf = Align.Center, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 8, marginBottom = 4, fontSize = 18 }
            };
            content.Add(editingLabel);

            //
            // Audio clips settings
            //

            UnityEngine.UIElements.PopupWindow clipsSettings = new()
            {
                text = "Audio clips",
                style = { marginTop = 10, marginLeft = 10, marginRight = 10 }
            };

            SerializedProperty clipsProp = SfxProperty.FindPropertyRelative(nameof(SFXDefinition.Clips));
            if (clipsProp.arraySize == 0)
            {
                clipsProp.InsertArrayElementAtIndex(0);
                clipsProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            SerializedProperty singleClipProp = clipsProp.GetArrayElementAtIndex(0);

            PropertyField singleClipField = new(singleClipProp, "");
            singleClipField.BindProperty(singleClipProp);

            PropertyField clipsField = new(clipsProp, "Clips")
            {
                tooltip = "A random one of these will be selected each time the SFX is played.",
            };
            clipsField.RegisterValueChangeCallback((changeEvent) =>
            {
                if (clipsProp.arraySize == 0)
                {
                    clipsProp.InsertArrayElementAtIndex(0);
                    clipsProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    singleClipField.Unbind();
                    singleClipField.BindProperty(clipsProp.GetArrayElementAtIndex(0));
                }
            });

            bool randomize = clipsProp.arraySize > 1;
            clipsSettings.text = randomize ? "Audio clips" : "Audio clip";

            Toggle randomizeClipsToggle = new()
            {
                value = randomize,
                style = { position = Position.Absolute, top = 3, right = 8, alignSelf = Align.FlexEnd, flexDirection = FlexDirection.RowReverse }
            };
            Label randomizeClipsToggleLabel = new("Randomize?")
            {
                style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Italic, marginRight = 2 }
            };
            randomizeClipsToggle.Add(randomizeClipsToggleLabel);
            randomizeClipsToggle.RegisterCallback<ChangeEvent<bool>>((callback) =>
            {
                if (callback.newValue == callback.previousValue) return;
                if (callback.newValue)
                {
                    clipsSettings.Add(clipsField);
                    clipsSettings.Remove(singleClipField);
                }
                else
                {
                    clipsSettings.Remove(clipsField);
                    clipsSettings.Add(singleClipField);
                    clipsProp.arraySize = 1;
                    clipsProp.serializedObject.ApplyModifiedProperties();
                }
                clipsSettings.text = callback.newValue ? "Audio clips" : "Audio clip";
            });

            clipsSettings.Add(randomizeClipsToggle);
            if (randomize)
                clipsSettings.Add(clipsField);
            else
                clipsSettings.Add(singleClipField);

            //
            // Pitch settings
            //

            UnityEngine.UIElements.PopupWindow pitchSettings = new()
            {
                text = "Pitch",
                style = { marginTop = 10, marginLeft = 10, marginRight = 10 }
            };

            SerializedProperty minPitchProp = SfxProperty.FindPropertyRelative(nameof(SFXDefinition.PitchMin));
            SerializedProperty maxPitchProp = SfxProperty.FindPropertyRelative(nameof(SFXDefinition.PitchMax));
            SerializedProperty randomizePitchProp = SfxProperty.FindPropertyRelative(nameof(SFXDefinition.RandomizePitch));
            PropertyField minPitchField = new(minPitchProp, "Min");
            PropertyField maxPitchField = new(maxPitchProp, "Max");

            randomize = randomizePitchProp.boolValue;
            minPitchField.label = randomize ? "Min" : "";

            Toggle randomizePitchToggle = new()
            {
                value = randomize,
                style = { position = Position.Absolute, top = 3, right = 8, alignSelf = Align.FlexEnd, flexDirection = FlexDirection.RowReverse }
            };
            Label randomizePitchToggleLabel = new("Randomize?")
            {
                style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Italic, marginRight = 2 }
            };
            randomizePitchToggle.Add(randomizePitchToggleLabel);
            randomizePitchToggle.RegisterCallback<ChangeEvent<bool>>((callback) =>
            {
                if (callback.newValue == callback.previousValue) return;
                if (callback.newValue)
                {
                    pitchSettings.Add(maxPitchField);
                }
                else
                {
                    pitchSettings.Remove(maxPitchField);
                }
                minPitchField.label = callback.newValue ? "Min" : "";
            });
            randomizePitchToggle.BindProperty(randomizePitchProp);

            pitchSettings.Add(randomizePitchToggle);
            pitchSettings.Add(minPitchField);
            if (randomize) pitchSettings.Add(maxPitchField);

            //
            // Volume settings
            //

            UnityEngine.UIElements.PopupWindow volumeSettings = new()
            {
                text = "Volume",
                style = { marginTop = 10, marginLeft = 10, marginRight = 10 }
            };

            SerializedProperty minVolumeProp = SfxProperty.FindPropertyRelative(nameof(SFXDefinition.VolumeMin));
            SerializedProperty maxVolumeProp = SfxProperty.FindPropertyRelative(nameof(SFXDefinition.VolumeMax));
            SerializedProperty randomizeVolumeProp = SfxProperty.FindPropertyRelative(nameof(SFXDefinition.RandomizeVolume));
            PropertyField minVolumeField = new(minVolumeProp, "Min");
            PropertyField maxVolumeField = new(maxVolumeProp, "Max");

            randomize = randomizeVolumeProp.boolValue;
            minVolumeField.label = randomize ? "Min" : "";

            Toggle randomizeVolumeToggle = new()
            {
                value = randomize,
                style = { position = Position.Absolute, top = 3, right = 8, alignSelf = Align.FlexEnd, flexDirection = FlexDirection.RowReverse }
            };
            Label randomizeVolumeToggleLabel = new("Randomize?")
            {
                style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Italic, marginRight = 2 }
            };
            randomizeVolumeToggle.Add(randomizeVolumeToggleLabel);
            randomizeVolumeToggle.RegisterCallback<ChangeEvent<bool>>((callback) =>
            {
                if (callback.newValue == callback.previousValue) return;
                if (callback.newValue)
                {
                    volumeSettings.Add(maxVolumeField);
                }
                else
                {
                    volumeSettings.Remove(maxVolumeField);
                }
                minVolumeField.label = callback.newValue ? "Min" : "";
            });
            randomizeVolumeToggle.BindProperty(randomizeVolumeProp);

            volumeSettings.Add(randomizeVolumeToggle);
            volumeSettings.Add(minVolumeField);
            if (randomize) volumeSettings.Add(maxVolumeField);

            //
            // Category field
            //

            PropertyField categoryField = new(SfxProperty.FindPropertyRelative(nameof(SFXDefinition.Category)), "Category")
            {
                tooltip = "Sound effects with the same category will be grouped together for easy organizing.",
                style = { marginLeft = Length.Percent(15), marginRight = Length.Percent(15), marginTop = 20 }
            };

            //
            // Preview button
            //

            PreviewButton = new(PreviewSound)
            {
                text = "Preview",
                style = { marginLeft = Length.Percent(20), marginRight = Length.Percent(20), marginTop = 20, marginBottom = 10 }
            };
            PreviewButtonErrorLabel = new Label()
            {
                visible = false,
                style = { color = new(new Color(0.8f, 0.4f, 0.2f)), position = Position.Absolute, bottom = 30, alignSelf = Align.Center }
            };

            //
            // Bind fields
            //

            categoryField.Bind(SfxProperty.serializedObject);
            minPitchField.Bind(SfxProperty.serializedObject);
            maxPitchField.Bind(SfxProperty.serializedObject);
            minVolumeField.Bind(SfxProperty.serializedObject);
            maxVolumeField.Bind(SfxProperty.serializedObject);
            clipsField.Bind(SfxProperty.serializedObject);

            categoryField.RegisterValueChangeCallback(OnCategoryChanged);

            content.Add(clipsSettings);
            content.Add(pitchSettings);
            content.Add(volumeSettings);
            content.Add(categoryField);
            content.Add(PreviewButton);
            content.Add(PreviewButtonErrorLabel);

            return content;
        }
        void OnCategoryChanged(SerializedPropertyChangeEvent changeEvent)
        {
            OnObjectUpdated?.Invoke();
        }

        void PreviewSound()
        {
            if (EditorUtility.audioMasterMute)
            {
                PreviewButtonErrorLabel.visible = true;
                PreviewButtonErrorLabel.text = "Scene view is muted.";
                return;
            }
            SFXDefinition sfx = (SFXDefinition)SfxProperty.boxedValue;
            if (sfx.Clips.Length == 0 || (sfx.Clips.Length == 1 && sfx.Clips[0] == null))
            {
                PreviewButtonErrorLabel.visible = true;
                PreviewButtonErrorLabel.text = "You must specify an audio clip.";
                return;
            }
            float pitch = sfx.RandomizePitch ? UnityEngine.Random.Range(sfx.PitchMin, sfx.PitchMax) : sfx.PitchMin;
            if (pitch == 0)
            {
                PreviewButtonErrorLabel.visible = true;
                PreviewButtonErrorLabel.text = "Tried to play with zero pitch.";
                return;
            }
            float volume = sfx.RandomizeVolume ? UnityEngine.Random.Range(sfx.VolumeMin, sfx.VolumeMax) : sfx.VolumeMin;
            if (volume <= 0)
            {
                PreviewButtonErrorLabel.visible = true;
                PreviewButtonErrorLabel.text = "Tried to play with zero volume.";
                return;
            }
            if (PreviewSoundSource == null)
            {
                GameObject previewSoundObject = new("[Cu1uSFX] SFXEditorPreviewSoundSource", typeof(AudioSource))
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                PreviewSoundSource = previewSoundObject.GetComponent<AudioSource>();
            }
            bool wasPlaying = PreviewSoundSource.isPlaying;
            bool stoppable = PreviewSoundSource.clip != null && PreviewSoundSource.clip.length / PreviewSoundSource.pitch >= STOPPABLE_PREVIEW_MIN_LENGTH;
            if (wasPlaying)
                PreviewSoundSource.Stop();
            if (!wasPlaying || (wasPlaying && !stoppable))
            {
                AudioClip clip = sfx.Clips[UnityEngine.Random.Range(0, sfx.Clips.Length)];
                if (clip == null)
                {
                    PreviewButtonErrorLabel.visible = true;
                    PreviewButtonErrorLabel.text = "An empty clip was selected.";
                    return;
                }
                PreviewSoundSource.clip = clip;
                PreviewSoundSource.pitch = pitch;
                PreviewSoundSource.volume = volume;
                stoppable = clip.length / pitch >= STOPPABLE_PREVIEW_MIN_LENGTH;
                PreviewSoundSource.Play();
                if (stoppable)
                {
                    PreviewButton.text = "Stop";
                    EditorApplication.update += TickPreviewFinished;
                }
            }
            PreviewButtonErrorLabel.visible = false;
        }

        void OnDisable()
        {
            if (PreviewSoundSource != null)
            {
                PreviewSoundSource.Stop();
                DestroyImmediate(PreviewSoundSource.gameObject);
            }
        }
    }

    public class SFX_NewSFXWindow_Editor : EditorWindow
    {
        SerializedProperty DefinitionsProperty;
        TextField NameField;
        Label ErrorLabel;
        string categoryToAddTo;
        public Action OnObjectUpdated;
        public static SFX_NewSFXWindow_Editor Spawn(SerializedProperty definitionsListProp, string category)
        {
            if (HasOpenInstances<SFX_NewSFXWindow_Editor>())
            {
                FocusWindowIfItsOpen<SFX_NewSFXWindow_Editor>();
                return GetWindow<SFX_NewSFXWindow_Editor>();
            }
            else
            {
                SFX_NewSFXWindow_Editor window = GetWindow<SFX_NewSFXWindow_Editor>(true, "Create new sound effect", true);
                window.ShowUtility();
                window.minSize = new(200, 120);
                window.Initialize(definitionsListProp, category);
                return window;
            }
        }
        void OnGUI()
        {
            if (DefinitionsProperty == null && this != null)
                Close();
        }
        void Initialize(SerializedProperty definitionsListProp, string category)
        {
            DefinitionsProperty = definitionsListProp;
            NameField = new()
            {
                style = { marginTop = 20, marginBottom = 20 }
            };
            ErrorLabel = new("errors go here")
            {
                visible = false,
                style = { color = Color.red }
            };
            categoryToAddTo = category;
            rootVisualElement.Add(GenerateRootContent());
            NameField.Focus();
        }
        VisualElement GenerateRootContent()
        {
            VisualElement container = new();

            Label label = new("Name:")
            {
                style = { alignSelf = Align.Center }
            };

            container.Add(label);

            container.Add(NameField);

            Button addNewButton = new(OnClickAddNew)
            {
                text = "Add"
            };
            container.Add(addNewButton);
            container.Add(ErrorLabel);

            return container;
        }

        void OnClickAddNew()
        {
            if (string.IsNullOrWhiteSpace(NameField.value))
            {
                ErrorLabel.visible = true;
                ErrorLabel.text = "Name cannot be empty.";
                return;
            }
            string formattedValue = SFXEnumGenerator.FormatEnumName(NameField.value);
            if (NameField.value != formattedValue)
            {
                ErrorLabel.visible = true;
                if (char.IsDigit(NameField.value[0]))
                {
                    ErrorLabel.text = "Name cannot start with a number.";
                }
                else
                {
                    ErrorLabel.text = "Name must be alphanumeric.";
                }
                NameField.value = formattedValue;
                return;
            }

            // Check if it already exists
            for (int i = 0; i < DefinitionsProperty.arraySize; i++)
            {
                SerializedProperty definitionProp = DefinitionsProperty.GetArrayElementAtIndex(i);
                string sfxName = definitionProp.FindPropertyRelative(nameof(SFXDefinition.Name)).stringValue;
                if (sfxName == formattedValue)
                {
                    ErrorLabel.visible = true;
                    ErrorLabel.text = "That name already exists.";
                    return;
                }
            }

            // Add property
            int newIndex = DefinitionsProperty.arraySize;
            DefinitionsProperty.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newSfx = DefinitionsProperty.GetArrayElementAtIndex(newIndex);
            newSfx.boxedValue = new SFXDefinition()
            {
                Name = formattedValue,
                Category = categoryToAddTo
            };
            newSfx.serializedObject.ApplyModifiedProperties();
            OnObjectUpdated?.Invoke();
            Close();
        }
    }
}
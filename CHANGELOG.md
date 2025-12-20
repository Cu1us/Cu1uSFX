# Changelog:

## 1.0.0

- Initial release

## 1.1.0

Features:
- Audio clips can now be played directly from any script using the Play() extension method.

Fixes:
- The SFXDefinition constructors are now more separated and won't cause ambiguity issues.

## 1.2.0

Features:
- The SFXDefinition class can now be exposed in the inspector and edited through the Edit SFX window.

Fixes:
- Changing the selected SFX in the Edit SFX window will now stop any ongoing sound previews.
- Added the "sfx" package keyword.

## 1.3.0

Features:
- A new setting on the SFX List can enable the generated SFX enums to be sorted by category, requiring you to refer to them using `SFX.YourCategory.YourSound` for sounds with specified categories, instead of `SFX.YourSound`.
- Code generation can now be completely disabled, as a setting on the SFX List asset.
  - This removes the Save button in the SFX list as it's redundant with code generation disabled.
- Changing SFX enum script generation target will now delete the old script, to prevent duplicate definitions.
  - This deletion will only happen if the old target script was automatically generated, and will not delete user scripts.
- Changing settings on the SFX List related to code generation now requires you to click a "Save and Recompile" button to apply changes.

Fixes:
- Changes to the Log Settings or Audio Source object pool settings on the SFX List asset are now properly saved.
- Recompilation of scripts after generating the SFX enum now use AssetDatabase.ImportAsset(), which should cause less issues and random errors.

## 1.4.0

Features:
- You can now quickly define a new SFX by selecting a number of audio clips in the project view, then selecting Create > SFX from selected clips. (Shortcut: Shift+Ctrl+A)
- The SFX list can now mark newly added but unsaved SFX with an asterisk, to mark that they are unsaved.
  - This is enabled by default, but can be disabled using a setting on the SFX List asset, to save performance for projects with a large number of sound effects.

Fixes:
- The SFX list window will now re-check for unsaved enums when created, to solve issues where the window is still marked as unsaved if it's open when scripts are recompiled from other sources.
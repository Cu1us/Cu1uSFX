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
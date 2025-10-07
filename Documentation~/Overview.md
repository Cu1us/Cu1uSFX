# Thanks for checking out Cu1uSFX!

*Cu1uSFX* is an easy-to-use plugin for creating sound effects in the editor, and then playing them from anywhere in your code!

Check out *Quickstart.md* to get a quick explanation on how to use the plugin.


## What features does this have?
Check out *Tips and Tricks.md* for a list of features and tips and tricks!


## How does it work?
When you install the plugin, it will try to create a SFXList ScriptableObject in *Assets/Resources/SFX List*. This asset will store all your sound effects and settings, so don't delete it!

Head over to that asset and click "Open SFX Editor", or click Window/SFX Editor in the toolbar, and you'll see the Sound Effects list. This window can be docked anywhere in the editor. Here you can create sound effect "definitions", that are stored in a big list in the SFXList singleton asset.

When you add or remove a sound effect, you need to click Save on the Sound Effects window. This will generate a script asset in your project, by default in *Assets/Scripts/SFXEnum.cs*, but this can be changed in the settings on the SFX List asset.

This script will then be filled with a static ``SFX`` class with one readonly static member for each sound effect. These are all assigned a lightweight ``PredefinedSFX`` struct that points to a specific sound effect in the SFX List asset. These are static and can be accessed from anywhere, and then played using ``SFXPlayer.Play(SFX.YourSoundEffect)`` (or ``SFX.YourSoundEffect.Play()``). This will access the ``SFXDefinition`` that the ``PredefinedSFX`` points to, and play it.

The PredefinedSFX class can also be serialized and displayed in the inspector so you can pick a sound effect in the editor. This has a handy dropdown menu that organized by category, if you have specified categories for your sound effects.

(You can also use SFXDefinition's constructor to create a temporary sound effect definition that you can play like any other.)

Playing a sound effect will spawn a special AudioSource with an attached SFXHandler component from an object pool, which then samples the SFXDefinition and plays it. After the clip has finished playing, the SFXHandler calls an event that puts it back into the object pool for future use. The SFXHandler also makes sure the AudioSource can do things such as follow a specific Transform at a specific offset, depending on which overload for Play() you used. Note that the AudioSources will be placed in DoNotDestroyOnLoad - if you want them to stop when switching scenes, use SFXPlayer.StopAll().

The Play()-functions return a ``SFXReference`` handle that you can use to change the volume, pitch, or position of the sound effect while it's playing, or stop it early. You can also make it follow a specific transform, optionally at a local-space offset, using ``FollowTransform()``. By default, however, sound effects are not spatialized and play globally, unless a position is specified in Play() or afterwards using its SFXReference.

Once the AudioSource has finished playing, the SFXReference's ``IsValid`` will become false, after which you should not use it, since the AudioSource it references will have been put back in the object pool. The class has checks to prevent editing it if it's invalid, but might fail if you cache the reference to the now-disabled AudioSource in your own scripts.


If you have any issues with the package or need help, feel free to contact me on discord @ cu1us.



Copyright © 2025 Måns Fritiofsson

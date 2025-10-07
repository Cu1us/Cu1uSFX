## QUICKSTART

1. Open the Sound Effects window in the Window/SFX Editor tab
  - You can also access this, and some settings, through the SFX List asset, located by default in Assets/Resources/SFX List.asset. This is created when you install the plugin.

2. Click **Add new**
  - Tip: The Sound Effects window can be docked like any other window

3. Type in a name for your sound effect. It must be alphanumeric, and cannot begin with a number.

4. Click **Edit** on your new sound effect.

5. Specify the Audio Clip(s) to play
  - If you want to play a random clip, or randomize volume/pitch, click the *Randomize?* toggle!
  - Note that this plugin is not intended for music or looping sound effects!

6. Click **Save** in the Sound Effects window when you're done.
  - You only need to click Save if you've added or removed a sound effect to the list. Edits to individual sound effects will be saved automatically.

7. Unity should now start recompiling C# scripts.
  - This is because the plugin generates a script in the background, located by default in *Assets/Scripts/SFXEnum.cs*, with static members that refer to each sound effect, for ease of use.

**You now have your first sound effect!**
Now, to play it:

8. Go to any script in your game

9. To play your new sound effect, simply call ``SFXPlayer.Play(SFX.YourSFXName);``
  - You can also use ``SFX.YourSFXName.Play()`` to do the same thing.

10. Done! Your sound effect should now play ingame when the code is run.

Check out *Tips and Tricks.md* for more advanced use cases!



Copyright © 2025 Måns Fritiofsson

# Tips & tricks

### Parameters of the Play() method
The `Play()` method has a lot of options to choose from - you can add a pitch multiplier, a volume multiplier, a world-space position to play at, or a transform to follow (optionally with an offset). The `Play()` method also returns a `SFXReference` handle that can be used to stop the sound later, or modify its settings.

Note:
Specifying a pitch or volume in the `Play()` function acts as a *multiplier* on top of the pitch or volume you've defined in the SFX List, if any. For example, if you've created a sound effect with a random volume between 1 and 2, and you specify a volume of 0.5f when you call Play(), the resulting sound will be played with a random volume between 0.5 and 1.

Example:
```cs
SFX.Example.Play(); // Play the sound globally
SFX.Example.Play(0.5f, 0.9f); // Volume multiplier of 0.5, pitch multiplier of 0.9
SFX.Example.Play(volume: 0.5f); // Volume multiplier of 0.5, pitch multiplier of 1 (default)
SFX.Example.Play(new Vector3(0, 0, 0), pitch: 0.9f); // Plays the sound at [0, 0, 0] in world space
SFX.Example.Play(transform); // Plays the sound at this object's position, following it when it moves
SFX.Example.Play(transform, new Vector3(0, 0, 1)); // Plays the sound one unit in front of this object, following it at that local offset when it moves or rotates

SFXReference sound = SFX.Example.Play(); // Save the handle of the sound
sound.Pitch = 1.5f; // Change pitch multiplier to 1.5
sound.WorldPosition = new Vector3(10, 10, 10); // Move the sound in world space. Also stops following any transform, if it was doing that before.
sound.WorldPosition = null; // Make the sound play globally (everywhere) again.
sound.Stop(); // Stops the sound.
// Note: If time has passed, you might want to check sound.IsValid to make sure the sound is still accessible.
```

### Playing AudioClips directly
As long as your script has `using Cu1uSFX;`, you can play any audio clip directly using `Play()` - either using `audioClip.Play()` or `SFXPlayer.Play(audioClip)`. As all other play functions, you can input a specific volume, pitch, position, or a transform to follow in the function, and stop it later using the SFXReference that is returned.

Example:
```cs
using Cu1uSFX;

public class Example : MonoBehaviour
{
    [SerializeField] AudioClip clip;

    void PlayClip()
    {
        clip.Play(); // Plays the clip
    }
}
```

### Scene changes
The AudioSources that this plugin creates to play all sounds are placed in DoNotDestroyOnLoad, meaning they will keep playing even if you unload the scene they were played from. If you do not want this behavior, you can use `SFXPlayer.StopAll()` to stop all sounds.


### Sound effects in the Inspector
The PredefinedSFX struct and the SFXDefinition class can both be serialized and editable through the inspector. Use these to easily configure sound effects on your objects!
Editing a PredefinedSFX in the inspector will expose an enum dropdown selection of all the sound effects you've defined in the SFX List; sorted by category.
A SFXDefinition, however, will allow you to configure a new, local SFX that only exists on that object, editable through its own window.

Example:
```cs
using Cu1uSFX;

public class Example : MonoBehaviour
{
    [SerializeField] PredefinedSFX A; // Enum dropdown of your created SFXs
    [SerializeField] SFXDefinition B; // New SFX that you can edit through a window

    void PlaySounds()
    {
        A.Play();
        B.Play();
    }
}
```


### Defining sound effects in runtime
You can create a new SFXDefinition class instance at runtime too, then play it freely as if it was any other SFX. Use any of the constructors - for example `new SFXDefinition(clipA, clipB);` or `new SFXDefinition(new AudioClip[] {clipA, clipB}, volume, pitch);`. You can then play it using `sfxDefinition.Play();` or `SFXPlayer.Play(sfxDefinition);`


### Moving the SFX List asset
The SFX List asset can be moved into any Resources folder. This means any folder in your Assets folder that is named Resources, or subfolders of such a folder. This restriction is because Resources.Load() must be able to access the asset at any time.


### Moving the SFXEnum.cs script
You cannot just drag the SFXEnum.cs script to some other folder - the SFX List references it by its path, and doing this would make the plugin think it was deleted, and generate a new one. (This might change to tracking by its GUID in a future update, which would solve this issue). But you can still move the script by following the instructions below.


### Changing which script file the SFX enum is generated in
Go to the SFX List asset (located by default in *Assets/Resources/SFX List*) and open the "Advanced" dropdown. Follow the instructions there to change the generation target. Be careful, the selected script will be overwritten and this cannot be undone! Only select an empty script as the enum generation target.

After changing the generation target, delete the previous script that had the SFX enums, then regenerate them in the new script by making a change in the Sound Effects list and clicking Save.

If this script is later deleted, it will revert to the default in *Assets/Resources/SFX List*, and create it if it does not exist.



Copyright © 2025 Måns Fritiofsson

# Cu1uSFX
Easy-to-use Unity package that streamlines playing and creating sound effects.

## 📯 Features
- Convenient editor window where you can define any number of named sound effects, and group them by categories.
- Define sound effects with randomized volume, pitch, and any number of audio clips.
- Preview sound effects in the editor while creating them!
- Play these sound effects anywhere in your game with a single line of code.

### Extras
- Play AudioClips from anywhere with the .Play() extension method.
- Sounds can be played globally, at a specific position, following a specific transform, and have their pitch/volume/position changed mid-playback.
- Define sound effects locally on objects in the inspector, or select from the global list using a dropdown.

## ⚙️ Installation
1. In the Unity Editor, open the Package Manager (Window -> Package Manager)
2. Click the large plus sign at the top
3. "Install package from git URL..."
4. Type in `https://github.com/Cu1us/Cu1uSFX.git`
5. Done!

## 🔨 Usage
When the package is installed, open the SFX List through the Window/SFX List tab, and start creating sound effects!
These can then be played from anywhere like this:
```cs
using Cu1uSFX;
public class Example : MonoBehaviour
{
    public void PlaySounds()
    {
        SFX.YourSound.Play();
        // OR
        SFXPlayer.Play(SFX.YourSound);
    }
}
```

You can also select sound effects through the inspector like this:
```cs
using Cu1uSFX;
public class SimpleSfxPlayer : MonoBehaviour
{
    [SerializeField] PredefinedSFX predefinedSfx; // Choose one of your defined sound effects from a dropdown in the inspector

    [SerializeField] SFXDefinition locallyDefinedSfx; // Define a new sound effect in the inspector for this object only, with its own audio clips, pitch, etc.

    public void PlaySounds()
    {
        predefinedSfx.Play();
        locallyDefinedSfx.Play();
    }
}
```
This will result in the inspector view below:  
![Inspector view of the SimpleSfxPlayer component](/Documentation~/Images/InspectorExample.png)

Need more info? Check out the quickstart guide: 📑 [Quickstart](/Documentation~/Quickstart.md)  

Overview, and how the package works internally: 📑 [Overview](/Documentation~/Overview.md)  

List of more advanced features: 📑 [Tips and Tricks](/Documentation~/Tips%20and%20Tricks.md)  

## 📷 Screenshots
The SFX List window: (opened through Window -> SFX List, or by clicking the button on the SFX List asset)  
![The SFX List window](/Documentation~/Images/SFXList.png)

SFX editing window:  
![The SFX Editor window](/Documentation~/Images/EditSFX.png)


## 📜 License
This project is licenced under the GNU General Public License v3. See the [LICENSE](/LICENSE.md) file for details.  

Copyright © 2025 Måns Fritiofsson

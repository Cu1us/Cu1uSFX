# Tips & tricks

### Sound effects in the Inspector
The PredefinedSFX struct can be serialized and displayed in the inspector, allowing you to pick a sound effect with a handy dropdown menu, organized by your categories.

You can also serialize the SFXDefinition class to define specific sound effects on your objects in the inspector. These can also be created in code using its constructor, then played like any other sound effect.


### Moving the SFX List asset
The SFX List asset can be moved into any Resources folder. This means any folder in your Assets folder that is named Resources, or subfolders of such a folder. This is because Resources.Load() must be able to access the asset at any time.


### Moving the SFXEnum.cs script
You cannot just drag the SFXEnum.cs script to some other folder - the SFX List references it by its path. This might change to its GUID in a future update. But you can still accomplish the same thing - follow the instructions below instead.


### Changing which script file the SFX enum is generated in
Go to the SFX List asset (located by default in *Assets/Resources/SFX List*) and open the "Advanced" dropdown. Follow the instructions there to change the generation target. Be careful, the selected script will be overwritten and this cannot be done! Only select an empty script as the enum generation target.

After changing the generation target, delete the previous script that had the SFX enums, then regenerate them in the new script by making a change in the Sound Effects list and clicking Save. 

If this script is later deleted, it will revert to the default in *Assets/Resources/SFX List*, and create it if it does not exist.



Copyright © 2025 Måns Fritiofsson

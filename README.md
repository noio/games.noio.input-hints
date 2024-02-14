# Input Hints

![Preview](Docs~/preview.gif)

Input hints are little prompts in game that display the button, key or action that the player needs to perform to
execute a certain action. 

> Press [A] to Jump

Unity's new Input and Localization systems are perfectly set create live dynamic Input Hints, based on
the actual bindings set up in the Input Actions. TextMesh Pro is used for displaying sprites inline in text strings.
All that it needs is a little bit of plumbing, and this package is that plumbing!

## How to use

You will need actual icons to display in the text. I can recommend the excellent 
[PC & Consoles Controller Buttons Icons Pack](https://assetstore.unity.com/packages/2d/gui/icons/pc-consoles-controller-buttons-icons-pack-85215).
This package is based around the way they set up sprites.

1. Install the package from the git URL
2. In Unity, go to `Assets/Create/Noio/Input Hints Config`
3. Follow instructions on the Input Hints Config asset.
4. Check out `DeviceDetectorSample.cs`
                                                                   

## Sample Setup

See below for a sample setup that uses the actions defined in Unity's Default Input Action Map (Look, Move, Fire), and 
the controller icons linked above.

![Sample Setup](Docs~/example_setup.png)
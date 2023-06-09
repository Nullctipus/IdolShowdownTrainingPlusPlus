# Deprecation Announcement
As I have been added to the Idol Showdown Team, I will be deprecating this mod when feature parity is achieved.

# Training++ For Idol Showdown

## Features
- Display frame times in command list
- Display what a move can cancel into in command list
- Display hit and hurt boxes in training mode
- Render character animations with hit and hurt boxes to pngs 
- Step Frames
- Replay Editor (very basic atm)

## Installation
Install [Bepinex5](https://github.com/BepInEx/BepInEx)

Download IdolShowdownTrainingPlusPlus.dll from [Releases](https://github.com/Nullctipus/IdolShowdownTrainingPlusPlus/releases/latest)

Install IdolShowdownTrainingPlusPlus.dll to Idol Showdown/Bepinex/plugins

## Usage
F8: Toggle Menu

F9: Toggle Frame Walk

f10: Step One Frame

[Replay Editor](https://github.com/Nullctipus/IdolShowdownTrainingPlusPlus/blob/bepinex5/ReplayEditor.md)

## Issues
- Projectiles do not render

## But I want to use MelonLoader mods

See [MelonLoader for Bepinex](https://github.com/BepInEx/BepInEx.MelonLoader.Loader)

## Linux
If you know how to install bepinex for linux you dont need this


Follow the Normal Installation and come back

Install [Proton Tricks](https://github.com/Matoking/protontricks)

run `protontricks --gui`

click `Idol Showdown: 1742020` from the list then click OK

Click OK for any error popups

Select `Select the default wineprefix` and click OK

Select `Run winecfg` and click OK

Click the `Libraries` tab

Under the label `New override for library` select winhttp and click add

Exit out of protontricks and you can start the game

## Why Not MelonLoader

Melonloader is harder to develop for as it expects Visual Studio and/or .Net Framework to be used. This makes development on Linux hell.

MelonLoader is more prone to breakage as BepInEx uses code analysers and imports Unity Modules from NuGet.

MelonLoader does not like when people protect their source code. [Code](https://github.com/LavaGang/MelonLoader/blob/41071711aa1b20d340196000b51f862118c736be/MelonLoader/Utils/AssemblyVerifier.cs#LL80C35-L80C35)

By extension machine generated code also will not load if MelonLoader detects it.
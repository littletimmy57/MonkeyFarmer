# Monkey Farmer

Monkey Farmer is a custom Primary tower for Bloons TD6. It fights at close range with a pitchfork and can place crop stands on the track that generate cash as Bloons pass them.

## Upgrade Paths

- Top path: throws increasingly powerful explosive seed packets, ending with the global-range Seed Storm.
- Middle path: improves the pitchfork's speed, damage, pierce, and fire damage, ending with Pitchfork Inferno.
- Bottom path: adds camo detection and improves the crop stand economy, ending with World Crop Center.

## Requirements

- Bloons TD 6 on Windows
- MelonLoader
- BTD Mod Helper

Use mods only in modded or single-player games. Do not use mods to gain an advantage in competitive or online play.

## Installation

1. Install MelonLoader and BTD Mod Helper.
2. Download `Gardener.dll` from the latest GitHub release.
3. Place `Gardener.dll` in the Bloons TD 6 `Mods` folder.
4. Start the game and confirm that Monkey Farmer appears in the tower menu.

## Building From Source

The project currently expects Bloons TD 6 at `X:\Steam\steamapps\common\BloonsTD6`. Change `BloonsTD6Path` in `Gardener.csproj` if your installation is elsewhere.

Build in Visual Studio, or run:

```powershell
dotnet build .\Gardener.csproj -c Release -p:Platform=x64 -p:CopyToModsFolder=false
```

The release DLL will be written under `bin\x64\Release\net6.0`.

## Credits

Concept, tower design, upgrade ideas, descriptions, and balancing by `contentpaper3833`. Published and maintained with parental supervision.

Built with BTD Mod Helper. Bloons TD 6 and its characters are property of Ninja Kiwi. This is an unofficial fan-made mod and is not affiliated with or endorsed by Ninja Kiwi.

## License

The original source code in this repository is available under the [MIT License](LICENSE). This license does not grant rights to Bloons TD 6, Ninja Kiwi assets, or third-party dependencies.

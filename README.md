# MapUpconverter
Features:
- Converts 3.3.5 map-related file formats to modern 7.3.5, 8.3.x, 9.2.x+ file formats (WDT, WDL, ADT).
- Adds height texturing information (MTXP).
- Filename => FileDataID conversions.
- Convert-on-save mode to convert ADTs as they are saved by e.g. Noggit while mapping [allowing for quick in-game previews](https://marlam.in/u/Wow_y5z4Dte6uZ.mp4).
- Optionally generates/updates an Epsilon patch for the map (and any other files in the output directory).

## Status
This tool is still actively in development and might not work for all usecases. For the currently supported usecases and how to set these up, see [this site](https://marlamin.github.io/modern-map-making/).

## Support
Please join our [Discord](https://discord.gg/q4tRTwwDEQ) for support, do not DM people but post in the #troubleshooting channel instead.

## Downloading
You can get the latest release from [the releases page](https://github.com/ModernWoWTools/MapUpconverter/releases).

## Usage
On Windows, open MapUpconverterGUI.exe and fill in the settings, download the required files and hit start.
Alternatively or on other platforms, create and fill in settings.json correctly and run MapUpconverter.exe.
For more information, see [this site](https://marlamin.github.io/modern-map-making/).

## Settings
You can use the included MapUpConverterGUI.exe tool (Windows only!) to change these settings. If you want to edit the settings.json/want more information on what things do, read on.

### Example settings.json file without Epsilon integration
```json
{
  "inputDir": "C:\\NoggitProject",
  "outputDir": "C:\\NoggitMapButModern",
  "mapName": "yourmapnamegoeshere",
  "epsilonDir": "",
  "epsilonPatchName": "",
  "arctiumDir": "",
  "arctiumPatchName": "",
  "generateWDTWDL": true,
  "rootWDTFileDataID": 0,
  "exportTarget": "Generic",
  "convertOnSave": false,
  "clientRefresh": false,
  "casRefresh": false,
  "mapID": -1,
  "targetVersion": 927,
  "lightBaseIntensity": 1.25,
  "lightBaseAttenuationEnd": 7.5,
  "useAdvancedLightConfig": false
}
```
### Example settings.json file with Epsilon integration
```json
{
  "inputDir": "C:\\NoggitProject",
  "outputDir": "C:\\NoggitMapButModern",
  "mapName": "yourmapnamegoeshere",
  "epsilonDir": "C:\\Epsilon",
  "epsilonPatchName": "YourPatchNameGoesHere",
  "arctiumDir": "",
  "arctiumPatchName": "",
  "generateWDTWDL": true,
  "rootWDTFileDataID": 1498241,
  "exportTarget": "Epsilon",
  "convertOnSave": true,
  "clientRefresh": true,
  "casRefresh": false,
  "mapID": -1,
  "targetVersion": 927,
  "lightBaseIntensity": 1.25,
  "lightBaseAttenuationEnd": 7.5,
  "useAdvancedLightConfig": false
}
```

All settings are required to be present in the file, but may be left as default depending on which, these settings are tagged as optional below.

### inputDir
Directory (e.g. inside Noggit project directory) with ADTs to monitor for changes.

### outputDir
Output directory to put converted files in.

### mapName
The name of your map. This is used to name the WDT file amongst other files.

### epsilonDir (optional)
Path to Epsilon launcher directory.

### epsilonPatchName (optional)
Name of Epsilon patch to keep create/keep updated.

### arctiumDir (optional)
Path to the Arctium Launcher directory.

### arctiumPatchName (optional)
Name for the Arctium patch names (used as filename for listfile mapping and patch dir in files folder).

### generateWDTWDL (optional)
Whether or not to generate/update WDT/WDL files. Defaults to `true` as this tool is primarily meant for fully custom maps, but can be disabled when editing a (partial) official map and you don't want to override official WDT/WDLs. If one wants to generate an updated WDT/WDL due to having added new ADTs or wanting to update the distant-mountain heightmap, make sure to extract all ADTs of the existing map into the input directory before doing so or it will only generate a WDT/WDL for the ADTs present in the input directory instead of all of them (and break the map).

### rootWDTFileDataID (optional)
When overwriting an existing WDT ID or if you already know your WDTs ID, you can set it here. If you use Epsilon integration, you will want to fill this in.

### convertOnSave (optional)
If set to false, generates the map once and then exits. This is required the first time you generate a map.
If set to true, the program will wait for updates in the input directory and convert them as soon as they are saved. This is useful for Noggit users who want to see their changes in-game quickly after they save them.

### clientRefresh (optional)
Whether or not to enable client refreshing (experimental), needs external DLL. Check [this](https://marlamin.github.io/modern-map-making/hot-reloading) for more information.

### casRefresh (optional)
Whether or not to enable the client refreshing the CAS filesystem, needs external DLL. Check [this](https://marlamin.github.io/modern-map-making/hot-reloading) for more information.

### mapID (optional)
Used with client refresh mode, defaults to -1.

### targetVersion (optional)
Currently supported target versions: 735, 830, 927. More might be added in the future to unlock version-specific features.

### lightBaseIntensity (optional)
Base intensity of lights to scale with their size.

### lightBaseAttenuationEnd (optional)
Base attenuation end of lights to scale with their size.

### useAdvancedLightConfig (optional)
Currently unused setting used in the future for more fine-grained light configuration.

## License
Unlike my other projects, this is licensed under GPLv3 instead of MIT to keep compatibility with 3rd-party licenses.

## Credits
- implave for constant project discussion/testing throughout
- [Warcraft.NET](https://github.com/ModernWoWTools/Warcraft.NET)
- [wowdev.wiki](https://wowdev.wiki/)

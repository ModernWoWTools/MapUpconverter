# MapUpconverter
Tool to automatically go from Noggit Red output (with modern features enabled) to modern ADTs readable by 8.2+ WoW clients. 

## Downloading
You can get the latest release from [the releases page](https://github.com/Marlamin/MapUpconverter).

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
    "rootWDTFileDataID": 0,
    "convertOnSave": false
}
```
### Example settings.json file with Epsilon integration
```json
{
    "inputDir": "C:\\NoggitProject",
    "outputDir": "C:\\Epsilon\\_retail_\\Patches\\YourPatchNameGoesHere",
    "mapName": "yourmapnamegoeshere",
    "epsilonDir": "C:\\Epsilon",
    "epsilonPatchName": "YourPatchNameGoesHere",
    "rootWDTFileDataID": 1498241,
    "convertOnSave": false
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

### rootWDTFileDataID (optional)
When overwriting an existing WDT ID or if you already know your WDTs ID, you can set it here. If you use Epsilon integration, you will want to fill this in.

### convertOnSave (optional)
If set to false, generates the map once and then exits. This is required the first time you generate a map.
If set to true, the program will wait for updates in the input directory and convert them as soon as they are saved. This is useful for Noggit users who want to see their changes in-game quickly after they save them.

## License
Licensed under GPLv3 instead of MIT (unlike my usual projects) to keep in line with 3rd-party licenses. 

## Credits
- implave for constant project discussion/testing throughout
- [Luzifix's Warcraft.NET](https://github.com/Luzifix/Warcraft.NET)
- [wowdev.wiki](https://wowdev.wiki/)
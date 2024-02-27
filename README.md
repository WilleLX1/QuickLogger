# QuickLogger
QuickLogger is a C# application designed to hooks clipboard, keys pressed, capture screenshots and more!

## Features

- Keylogging: ...
- Screenshots: ...
- Process monitoring: ...
- Clipboard monitoring: ...

## Usage

1. Open the project in Visual Studio.
2. Set settings in Form1.cs
3. Build and run the project.
4. OPTIONAL: Build as one-file to only have one file instead of entire project (with DLL files)

## All Settings
* **DebugMode** - If false, the window will be hidden and the program will run in the background.
* **Webhook** - Set your desired discord webhook address, it will send all data to the webhook.
* **logPath** - Path to log file. (E.g: C:\users\public\log.txt)
* **Persistence** - If true, will install persistence to start on every startup. Else if false, will only make a run-once that sends the data.
* **RegistryKeyName** - Name of the registry key and persistence EXE name.
* **persistencePath** - Path to save persistence file (E.g: C:\users\public\persistence)
* **useScreenshot** - OBS! ONLY POSSIBLE IF PERSISTENCE IS TRUE! If true, the program will take screenshots and send them zipped when sending log.
* **screenshotInterval** - Interval between screenshots in milliseconds (E.g: 10000)
* **screenshotPath** - Path to save screenshots (E.g: C:\\users\publi\Screenshots)
* **flagPath** - Path to save timestamp file for first infection (E.g: C:\users\public\flag.txt)

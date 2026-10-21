# Easy-Video-Converter
An easy point-and-click video converter that converts video into a DirectPlay format for Plex/Jellyfin servers.

## Overview ##
The goal of this project is to take any source video and convert it to a format where a streaming server (Plex, Jellyfin, etc...) can DirectPlay without any transcoding.

## Contributing ##
[Read Contributing](https://github.com/Dynamensions/Easy-Video-Converter/blob/main/CONTRIBUTING.md)

## Technical Details ##
- Architecture
  - MVVM
  - Core
    - Engine
  - Front Ends
    - EasyVideoConverter (Current main entry point)
      - WIN UI 3
      - Main Entry Point
    - EasyVideoConverter.RichClient (Future Windows Presentation Rich Client)
    - Console (future console application)
   
### Thoughts ###
- May break out services into its own layer to provide several service layers for several CODEC engines

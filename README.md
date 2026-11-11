# Easy-Video-Converter
An easy point-and-click video converter that converts video into a DirectPlay format for Plex/Jellyfin servers. DirectPlay allows the device to download and decode the file making keeping your media server from transcoding. This will allow you to have a lightweight media server.

## Overview ##
The goal of this project is to take any source video and convert it to a format where a streaming server (Plex, Jellyfin, etc...) can DirectPlay without any transcoding.

### Subtitles ###
There are subtitle options available and will only work if the video contains subtitles; otherwise, these options are ignored. There are dedicated subtitle managers. 
If you would like to download subtitles for the encoded videos:
- [Sonarr](https://github.com/Sonarr/Sonarr) - TV show subtitle manager 
- [Radarr](https://github.com/Radarr/Radarr) - Movie subtitle manager
- [Bazarr](https://github.com/morpheus65535/bazarr) - Manager for Sonarr and Radarr (recommended)

These managers will automatically download or update subtitles for files added to watched folders. If the subtitle is not available, it will retry. It is recommended to install these on your streaming server, but you can also install them on the system hosting this application and have it watch the output folder.

NOTE: Adding subtitles to this project will make it more of a media manager than an encoder. Possible future work, but not a priority at this time.

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

### Devices used to test ###
- [GMKtec NucBox 5](https://www.amazon.com/s?k=nucbox+5&crid=35VQJZGY1UBQP&sprefix=nucbox+5) - Retail price 150 USD.
- [CWWK Fanless Mini PC Intel N Series-N100](https://www.amazon.com/s?k=CWWK+PC+n100&i=electronics&crid=3QVO7R0A29D34&sprefix=cwwk+pc+n100) - Retail price 350-500 (Recommended)

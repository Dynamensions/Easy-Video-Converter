# Easy-Video-Converter
An easy point-and-click video converter that converts video into a DirectPlay format for Plex/Jellyfin servers.

## Overview ##
The goal of this project is to take any source video and convert it to a format where a streaming server (Plex, Jellyfin, etc...) can DirectPlay without any transcoding.

## Contributing ##
If you would like to contribute, you can clone the document. All dependencies are downloaded including FFMPEG binaries. 

Code should be in a branching structure (I know at the moment it's a mess; I'm in the process of cleaning it once I get it stable). A branch structure is where units of code exist at the bottom of the code page, while the trunk that calls them is at the top. The trunk will call the branches in the order of execution, and the branches will also be in that order. Example:

```cs
public void ProcessVideo()
{
  AnalyzeVideo();
  EncodeVideo();
}

private void AnalyzeVideo()
{
  // do work
}

private void EncodeVideo()
{
  // do work
}
```

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

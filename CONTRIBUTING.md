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

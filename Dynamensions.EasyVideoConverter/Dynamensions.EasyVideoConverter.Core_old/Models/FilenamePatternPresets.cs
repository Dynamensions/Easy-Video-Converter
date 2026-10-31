namespace Dynamensions.EasyVideoConverter.Core.Models;

public static class FilenamePatternPresets
{
    public static readonly Dictionary<string, string> Presets =
        new()
        {
            ["Simple"] = "{name}",
            ["Properties"] = "{name}.{vcodec}.{res}.{hdr}",
            ["Properties (Extended)"] = "{name}.{vcodec}.{res}.{hdr}.{acodec}.{achannels}",
            ["Client Optimized"] = "{name}.{client}.{vcodec}.{res}.{acodec}.{achannels}",
            ["Technical"] = "{name}.{vcodec}.{profile}.{level}.{bitdepth}.{hdr}.{res}.{fps}.{acodec}.{achannels}",
            ["Full Nerd Mode"] = "{name}.{vcodec}.{profile}.{level}.{bitdepth}.{hdr}.{acodec}.{abitrate}.{achannels}.{fps}.{encoder}.{sub}.{perf}",
            //["Custom"] = "" // user will fill this manually
        };
}

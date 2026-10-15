using Dynamensions.EasyVideoConverter.Core.Models;

namespace Dynamensions.EasyVideoConverter.Helpers;

public static class SubtitleSelector
{
    public static SubtitleSelectionResult Select(MediaInfo info, IEnumerable<string> preferredLanguages, string? preferredBurninLanguage)
    {
        var result = new SubtitleSelectionResult();
        SubtitleTrack? burnInCandidate = null;

        if (!info.HasSubtitles)
            return result;

        // Normalize preferred languages
        var prefs = preferredLanguages
            .Select(l => l.ToLowerInvariant())
            .ToList();

        // Normalize subtitle languages (fix "und")
        foreach (var s in info.Subtitles)
        {
            if (s.Language == "und" || string.IsNullOrWhiteSpace(s.Language))
                s.Language = GuessLanguageFromTitle(s.Title);
        }


        // ---------------------------------------------------------
        // 1. If user wants burn-in Try to find the burn-in language
        // ---------------------------------------------------------
        if (preferredBurninLanguage is not null)
        {
            burnInCandidate = info.Subtitles
                .Where(s => s.Language.Equals(preferredBurninLanguage, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => SubtitlePriority(s.Codec))
                .FirstOrDefault();
        }
        // If found → burn-in it
        if (burnInCandidate != null)
        {
            result.BurnIn = burnInCandidate;

            // Everything else becomes external
            result.External.AddRange(info.Subtitles.Where(s => s != burnInCandidate));
            return result;
        }

        // ---------------------------------------------------------
        // 2. If user requested a language that does not exist
        //    → do NOT burn-in anything
        // ---------------------------------------------------------

        result.BurnIn = burnInCandidate;

        // ---------------------------------------------------------
        // 3. External subtitles = all preferred languages except burn-in
        // ---------------------------------------------------------
        foreach (var pref in prefs)
        {
            var matches = info.Subtitles
                .Where(s => s.Language == pref)
                .Where(s => s != burnInCandidate);

            result.External.AddRange(matches);
        }

        result.External = [.. FilterTracks(result.External)];

        return result;
    }

    private static string GuessLanguageFromTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "und";

        title = title.ToLowerInvariant();

        if (title.Contains("eng")) return "eng";
        if (title.Contains("english")) return "eng";
        if (title.Contains("sdh")) return "eng";
        if (title.Contains("hi")) return "eng-hi";

        if (title.Contains("arabic") || title.Contains("ara")) return "ara";
        if (title.Contains("spanish") || title.Contains("spa")) return "spa";
        if (title.Contains("french") || title.Contains("fre")) return "fre";
        if (title.Contains("german") || title.Contains("ger")) return "ger";
        if (title.Contains("japanese") || title.Contains("jpn")) return "jpn";
        if (title.Contains("chinese") || title.Contains("chi")) return "chi";

        return "und";
    }

    private static int SubtitlePriority(string codec)
    {
        return codec.ToLowerInvariant() switch
        {
            "subrip" => 0,
            "srt" => 0,
            "ass" => 1,
            "mov_text" => 2,
            "hdmv_pgs_subtitle" => 3,
            _ => 4
        };
    }

    private static IEnumerable<SubtitleTrack> FilterTracks(IEnumerable<SubtitleTrack> tracks)
    {
        return tracks
            // Remove commentary / weird tracks
            .Where(t => t.Title is not null && !t.Title.Contains("notes", StringComparison.OrdinalIgnoreCase))
            .Where(t => t.Title is not null && !t.Title.Contains("commentary", StringComparison.OrdinalIgnoreCase))
            .Where(t => t.Title is not null && !t.Title.Contains("commentary", StringComparison.OrdinalIgnoreCase))
            .Where(t => t.Title is not null && !t.Title.Contains("nothing", StringComparison.OrdinalIgnoreCase))
            .Where(t => t.Title is not null && !t.Title.Contains("nothing", StringComparison.OrdinalIgnoreCase))
            .Where(t => t.Title is not null && !t.Title.Contains("lyrics", StringComparison.OrdinalIgnoreCase))
            .Where(t => t.Title is not null && !t.Title.Contains("lyrics", StringComparison.OrdinalIgnoreCase))

            // Remove SDH unless user explicitly wants SDH
            .Where(t => t.Title is not null && !t.Title.Contains("sdh", StringComparison.OrdinalIgnoreCase))

            // Sort by codec priority
            .OrderBy(t => SubtitlePriority(t.Codec))

            // Pick best per language
            .GroupBy(t => t.Language)
            .Select(g => g.First());
    }

}


public class SubtitleSelectionResult
{
    public SubtitleTrack? BurnIn { get; set; }
    public List<SubtitleTrack> External { get; set; } = [];
}


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Search;

/// <summary>
/// Metadata associated with a YouTube video returned by a search query.
/// </summary>
public class VideoSearchResult : Video, ISearchResult
{
    public VideoSearchResult(
        VideoId id,
        string title,
        string description,
        Author author,
        TimeSpan? duration,
        IReadOnlyList<Thumbnail> thumbnails
    )
        : base(id, title, author, description, duration, thumbnails) { }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Video ({Title})";
}

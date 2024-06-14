using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Playlists;

/// <summary>
/// Metadata associated with a YouTube video included in a playlist.
/// </summary>
public class PlaylistVideo : Video, IBatchItem
{
    public PlaylistVideo(
        PlaylistId playlistId,
        VideoId id,
        string title,
        Author author,
        TimeSpan? duration,
        IReadOnlyList<Thumbnail> thumbnails
    )
        : base(id, title, author, default, duration, thumbnails)
    {
        PlaylistId = playlistId;
    }

    /// <summary>
    /// ID of the playlist that contains this video.
    /// </summary>
    public PlaylistId PlaylistId { get; }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Video ({Title})";
}

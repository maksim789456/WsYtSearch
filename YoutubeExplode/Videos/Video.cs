using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;

namespace YoutubeExplode.Videos;

/// <summary>
/// Metadata associated with a YouTube video.
/// </summary>
public class Video(
    VideoId id,
    string title,
    Author author,
    string description,
    TimeSpan? duration,
    IReadOnlyList<Thumbnail> thumbnails
) : IVideo
{
    /// <inheritdoc />
    public VideoId Id { get; } = id;

    /// <inheritdoc />
    public string Url => $"https://www.youtube.com/watch?v={Id}";

    /// <inheritdoc />
    public string Title { get; } = title;

    /// <inheritdoc />
    public Author Author { get; } = author;

    /// <inheritdoc />
    public string Description { get; } = description;

    /// <inheritdoc />
    public TimeSpan? Duration { get; } = duration;

    /// <inheritdoc />
    public IReadOnlyList<Thumbnail> Thumbnails { get; } = thumbnails;

    public DateTimeOffset PublishTime
    {
        get
        {
            if (!_publishTime.Equals(DateTimeOffset.MinValue))
                return _publishTime;

            var uploadTime = YoutubeClient
                .Instance.Videos.GetVideoUploadDate(Id)
                .GetAwaiter()
                .GetResult();
            _publishTime = uploadTime;
            return _publishTime;
        }
        set => _publishTime = value;
    }

    private DateTimeOffset _publishTime = DateTimeOffset.MinValue;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Video ({Title})";
}

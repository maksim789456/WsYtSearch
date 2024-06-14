using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using WsYtSearch.Models;
using WsYtSearch.Utils;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace WsYtSearch.Controllers;

public class WsYtController : ControllerBase
{
    [HttpGet("/ws")]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
        await WsYt(ws);
    }

    private static readonly FetchContext Context = new();
    private static readonly YoutubeClient YoutubeClient = new();

    private static async Task WsYt(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            var receivedStr = Encoding.UTF8.GetString(buffer).Trim('\0');
            Console.WriteLine(receivedStr);
            var tokens = receivedStr.Split(";");
            switch (tokens[0])
            {
                case "fetch":
                    await Fetch(webSocket, tokens);
                    break;
                case "get":
                    await GetData(webSocket, tokens);
                    break;
            }

            Array.Clear(buffer);
            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

    private static async Task GetData(WebSocket webSocket, string[] tokens)
    {
        if (tokens.Length < 3)
        {
            Console.WriteLine("Not enough args");
            return;
        }

        var id = Int32.Parse(tokens[1]);
        if (id < 0 || id > Context.Results.Count - 1)
            return;
        var result = Context.Results[id];

        string? data = tokens[2] switch
        {
            "source" => result.Url,
            "videoId" => result.Id,
            "title" => result.Title,
            "description" => result.Description,
            "channelTitle" => result.Author.ChannelTitle,
            "channelId" => result.Author.ChannelId,
            "channelLink" => result.Author.ChannelUrl,
            "thumbnail" => result.Thumbnails[0].Url,
            "publishTime" => result.PublishTime.ToString("s", CultureInfo.InvariantCulture),
            _ => null
        };
        if (data != null)
            await webSocket.SendString(data);
    }

    private static async Task Fetch(WebSocket webSocket, string[] tokens)
    {
        if (tokens.Length < 3)
        {
            Console.WriteLine("Not enough args");
            return;
        }

        Context.SearchText = tokens[2];
        Context.EntriesCount = Int32.Parse(tokens[1]);
        Context.Results = [];
        Context.Type = FetchType.Search;
        Context.VideoId = Context.PlaylistId = null;

        TryToParse();

        switch (Context.Type)
        {
            case FetchType.Search:
                var videoSearchResults =
                    await YoutubeClient.Search.GetVideosAsync(Context.SearchText, Context.EntriesCount);
                Context.Results = videoSearchResults.Cast<Video>().ToList();
                break;
            case FetchType.Video:
                if (Context.EntriesCount <= 0)
                    return;
                var videoResult = await YoutubeClient.Videos.GetAsync(Context.VideoId);
                Context.Results = [videoResult];
                break;
            case FetchType.Playlist:
                var playlist = await YoutubeClient.Playlists.GetVideosAsync(Context.PlaylistId);
                Context.Results = playlist.Cast<Video>().ToList();
                break;
        }

        var entriesCount = Context.Type == FetchType.Video ? Context.EntriesCount : 1;
        await webSocket.SendString($"fetchOk;{Context.Type};{entriesCount};{Context.Results.Count}");
    }

    private static void TryToParse()
    {
        if (!Uri.TryCreate(Context.SearchText, UriKind.Absolute, out var uri)) return;
        switch (uri.Host)
        {
            case "youtu.be":
                Context.VideoId = uri.AbsolutePath.Replace("/", "");
                Context.Type = FetchType.Video;
                break;
            case "youtube.com" or "www.youtube.com":
            {
                var query = HttpUtility.ParseQueryString(uri.Query);
                switch (uri.AbsolutePath)
                {
                    case "/watch":
                    {
                        var playlistId = query["list"];
                        if (playlistId != null)
                        {
                            Context.PlaylistId = playlistId;
                            Context.Type = FetchType.Playlist;
                            break;
                        }

                        var videoId = query["v"];
                        if (videoId == null) return;
                        Context.VideoId = videoId;
                        Context.Type = FetchType.Video;
                        break;
                    }
                    case "/playlist":
                    {
                        var playlistId = query["list"];
                        if (playlistId == null) return;
                        Context.PlaylistId = playlistId;
                        Context.Type = FetchType.Playlist;
                        break;
                    }
                }

                break;
            }
        }
    }
}
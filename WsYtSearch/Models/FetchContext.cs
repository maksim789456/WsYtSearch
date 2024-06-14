using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace WsYtSearch.Models;

public class FetchContext
{
    public string SearchText { get; set; }
    public string? VideoId { get; set; }
    public string? PlaylistId { get; set; }
    public FetchType Type { get; set; }
    public int EntriesCount { get; set; }
    public List<Video> Results { get; set; }
}
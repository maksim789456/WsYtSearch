using System.Net.WebSockets;
using System.Text;

namespace WsYtSearch.Utils;

public static class WebSocketUtils
{
    public static async Task SendString(this WebSocket ws, string text) => await ws.SendAsync(
        Encoding.UTF8.GetBytes(text),
        WebSocketMessageType.Text, true, CancellationToken.None);
}
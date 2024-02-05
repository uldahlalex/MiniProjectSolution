using System.Net.WebSockets;
using System.Text.Json;
using Fleck;
using lib;

namespace Api.Helpers.cs;

public static class ExtensionMethods
{
    public static void SendDto(this IWebSocketConnection ws, BaseDto dto) 
        => ws.Send(JsonSerializer.Serialize(dto) ?? throw new ArgumentException("Failed to serialize dto"));
}
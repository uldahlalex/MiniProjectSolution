using System.Text.Json;
using Fleck;
using lib;

namespace Api.Helpers.cs;

public static class ExtensionMethods
{
    public static void SendDto<T>(this IWebSocketConnection ws, T dto) where T : BaseDto
    {
        ws.Send(JsonSerializer.Serialize(dto) ?? throw new ArgumentException("Failed to serialize dto"));
    }
}
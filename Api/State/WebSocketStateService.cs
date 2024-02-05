using System.Text.Json;
using Api.Helpers.cs;
using Api.Models.QueryModels;
using Fleck;
using lib;
using Serilog;

namespace Api.State;

public class WsWithMetadata(IWebSocketConnection connection)
{
    public IWebSocketConnection Connection { get; set; } = connection;
    public bool IsAuthenticated { get; set; } = false;
    public EndUser? User { get; set; }
}

public class WebSocketStateService
{
    private readonly Dictionary<Guid, WsWithMetadata> _clients = new();
    private readonly Dictionary<Guid, HashSet<string>> _clientToRooms = new();
    private readonly Dictionary<string, HashSet<Guid>> _roomsToClients = new();

    public WsWithMetadata GetClient(Guid clientId)
    {
        Log.Information(JsonSerializer.Serialize(_clients.Keys.ToList()), "Connected clients:");
        return _clients[clientId];
    }

    public void AddClient(Guid clientId, IWebSocketConnection connection)
    {
        _clients.TryAdd(clientId, new WsWithMetadata(connection));
        //log all connections
        Log.Information(JsonSerializer.Serialize(_clients.Keys.ToList()), "Connected clients:");
    }

    public void RemoveClient(Guid clientId)
    {
        if (_clientToRooms.TryGetValue(clientId, out var rooms))
        {
            foreach (var room in rooms)
            {
                _roomsToClients[room].Remove(clientId);
                if (_roomsToClients[room].Count == 0) _roomsToClients.Remove(room);
            }

            _clientToRooms.Remove(clientId);
        }

        _clients.Remove(clientId);
    }

    public void JoinRoom(Guid clientId, string roomId)
    {
        if (!_roomsToClients.ContainsKey(roomId)) _roomsToClients[roomId] = new HashSet<Guid>();

        _roomsToClients[roomId].Add(clientId);

        if (!_clientToRooms.ContainsKey(clientId)) _clientToRooms[clientId] = new HashSet<string>();

        _clientToRooms[clientId].Add(roomId);
    }

   

    public void BroadcastMessage(string roomId, BaseDto dto)
    {
        if (_roomsToClients.TryGetValue(roomId, out var clients))
            foreach (var clientId in clients)
                if (_clients.TryGetValue(clientId, out var connection))
                    connection.Connection.SendDto(dto);
    }

    public List<IWebSocketConnection> GetClientsInRoom(string room)
    {
        return _roomsToClients.TryGetValue(room, out var clients)
            ? clients.Select(clientId => _clients[clientId].Connection).ToList()
            : new List<IWebSocketConnection>();
    }

    public List<string> GetRoomsForClient(Guid clientId)
    {
        return _clientToRooms.TryGetValue(clientId, out var rooms)
            ? rooms.ToList()
            : new List<string>();
    }
}
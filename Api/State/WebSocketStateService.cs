
using Fleck;

namespace api.State;

public class WebSocketStateService
{
    private readonly Dictionary<Guid, IWebSocketConnection> _clients = new();
    private readonly Dictionary<string, HashSet<Guid>> _roomsToClients = new();
    private readonly Dictionary<Guid, HashSet<string>> _clientToRooms = new();

    public void AddClient(Guid clientId, IWebSocketConnection connection)
    {
        _clients[clientId] = connection;
    }

    public void RemoveClient(Guid clientId)
    {
        if (_clientToRooms.TryGetValue(clientId, out var rooms))
        {
            foreach (var room in rooms)
            {
                _roomsToClients[room].Remove(clientId);
                if (_roomsToClients[room].Count == 0)
                {
                    _roomsToClients.Remove(room);
                }
            }

            _clientToRooms.Remove(clientId);
        }

        _clients.Remove(clientId);
    }

    public void JoinRoom(Guid clientId, string roomId)
    {
        if (!_roomsToClients.ContainsKey(roomId))
        {
            _roomsToClients[roomId] = new HashSet<Guid>();
        }

        _roomsToClients[roomId].Add(clientId);

        if (!_clientToRooms.ContainsKey(clientId))
        {
            _clientToRooms[clientId] = new HashSet<string>();
        }

        _clientToRooms[clientId].Add(roomId);
    }

    public void LeaveRoom(Guid clientId, string roomId)
    {
        if (_roomsToClients.ContainsKey(roomId) && _roomsToClients[roomId].Remove(clientId))
        {
            if (_roomsToClients[roomId].Count == 0)
            {
                _roomsToClients.Remove(roomId);
            }
        }

        if (_clientToRooms.ContainsKey(clientId))
        {
            _clientToRooms[clientId].Remove(roomId);
            if (_clientToRooms[clientId].Count == 0)
            {
                _clientToRooms.Remove(clientId);
            }
        }
    }

    public void BroadcastMessage(string roomId, string message)
    {
        if (_roomsToClients.TryGetValue(roomId, out var clients))
        {
            foreach (var clientId in clients)
            {
                if (_clients.TryGetValue(clientId, out var connection))
                {
                    connection.Send(message);
                }
            }
        }
    }
}
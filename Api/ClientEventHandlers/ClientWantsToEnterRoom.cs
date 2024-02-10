using System.ComponentModel.DataAnnotations;
using Api.ClientEventFilters;
using Api.Helpers.cs;
using Api.Models.ParameterModels;
using Api.Models.ServerEvents;
using Api.Repositories;
using Api.State;
using Fleck;
using lib;

namespace Api.ClientEventHandlers;

public class ClientWantsToEnterRoomDto : BaseDto
{
    [Required] [Range(1, int.MaxValue)] public int roomId { get; set; }
}

[RequireAuthentication]
[ValidateDataAnnotations]
public class ClientWantsToEnterRoom(
    ChatRepository chatRepository) : BaseEventHandler<ClientWantsToEnterRoomDto>
{
    public override Task Handle(ClientWantsToEnterRoomDto dto, IWebSocketConnection socket)
    {
        WebSocketStateService.JoinRoom(socket.ConnectionInfo.Id, dto.roomId);
        socket.SendDto(new ServerAddsClientToRoom
        {
            messages = chatRepository.GetPastMessages(new GetPastMessagesParams(dto.roomId)),
            liveConnections = WebSocketStateService.GetClientsInRoom(dto.roomId).Count,
            roomId = dto.roomId
        });
        WebSocketStateService.BroadcastMessage(dto.roomId, new ServerNotifiesClientsInRoomSomeoneHasJoinedRoom
        {
            message = "Client joined the room!",
            userEmail = WebSocketStateService.GetClient(socket.ConnectionInfo.Id).User.email,
            roomId = dto.roomId
        });

        return Task.CompletedTask;
    }
}
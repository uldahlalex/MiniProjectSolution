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
    ChatRepository chatRepository,
    WebSocketStateService stateService) : BaseEventHandler<ClientWantsToEnterRoomDto>
{
    public override Task Handle(ClientWantsToEnterRoomDto dto, IWebSocketConnection socket)
    {
        stateService.JoinRoom(socket.ConnectionInfo.Id, dto.roomId.ToString());
        socket.SendDto(new ServerAddsClientToRoom
        {
            messages = chatRepository.GetPastMessages(new GetPastMessagesParams(dto.roomId)),
            liveConnections = stateService.GetClientsInRoom(dto.roomId + ToString()).Count,
            roomId = dto.roomId
        });
        stateService.BroadcastMessage(dto.roomId.ToString(), new ServerNotifiesClientsInRoomSomeoneHasJoinedRoom
        {
            message = "Client joined the room!",
            user = stateService.GetClient(socket.ConnectionInfo.Id).User,
            roomId = dto.roomId
        });

        return Task.CompletedTask;
    }
}
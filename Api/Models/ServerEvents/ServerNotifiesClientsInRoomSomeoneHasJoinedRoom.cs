using Api.Models.QueryModels;
using lib;

namespace Api.Models.ServerEvents;

public class ServerNotifiesClientsInRoomSomeoneHasJoinedRoom : BaseDto
{
    public int roomId { get; set; }

    public string? message { get; set; }
    public EndUser user { get; set; }
}
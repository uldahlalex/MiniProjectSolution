using Api.Models.QueryModels;
using lib;

namespace Api.Models.ServerEvents;

public class ServerBroadcastsMessageToClientsInRoom : BaseDto
{
    public MessageWithSenderEmail? message { get; set; }
    public int roomId { get; set; }
}
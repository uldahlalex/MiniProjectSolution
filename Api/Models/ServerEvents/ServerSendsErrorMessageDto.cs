using lib;

namespace Api.ServerEvents;

public class ServerSendsErrorMessageDto : BaseDto
{
    public string message { get; set; }
}
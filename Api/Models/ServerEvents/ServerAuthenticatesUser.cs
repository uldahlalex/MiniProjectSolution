using lib;

namespace Api.Models.ServerEvents;

public class ServerAuthenticatesUser : BaseDto
{
    public string? jwt { get; set; }
}
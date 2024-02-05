using System.ComponentModel.DataAnnotations;
using Externalities.QueryModels;
using Fleck;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using lib;
using Serilog;

namespace Api.ClientEventHandlers;

public class ClientWantsToSignInDto : BaseDto
{
    public string email { get; set; }
    
    public string password { get; set; }
}

public class ClientWantsToAuthenticate : BaseEventHandler<ClientWantsToSignInDto>
{
    public override Task Handle(ClientWantsToSignInDto dto, IWebSocketConnection socket)
    {
        
    }
    
  
}
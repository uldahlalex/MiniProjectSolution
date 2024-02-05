using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using Api.ClientEventFilters;
using Api.Helpers.cs;
using Api.Models.ParameterModels;
using Api.Models.ServerEvents;
using Api.Repositories;
using Api.Security;
using Api.State;
using Fleck;
using lib;

namespace Api.ClientEventHandlers;

public class ClientWantsToAuthenticateWithJwtDto : BaseDto
{
    [Required] public string? jwt { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsToAuthenticateWithJwt(
    ChatRepository chatRepository,
    TokenService tokenService)
    : BaseEventHandler<ClientWantsToAuthenticateWithJwtDto>
{
    public override async Task Handle(ClientWantsToAuthenticateWithJwtDto dto, IWebSocketConnection socket)
    {
        var claims = tokenService.ValidateJwtAndReturnClaims(dto.jwt!);
        var user = chatRepository.GetUser(new FindByEmailParams(claims["email"]));
        if (user.isbanned)
            throw new AuthenticationException("User is banned");
        WebSocketStateService.GetClient(socket.ConnectionInfo.Id).User = user;
        WebSocketStateService.GetClient(socket.ConnectionInfo.Id).IsAuthenticated = true;
        socket.SendDto(new ServerAuthenticatesUserFromJwt());
    }
}
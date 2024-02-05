using System.Security.Authentication;
using Api.Helpers.cs;
using Api.Models.ParameterModels;
using Api.Models.ServerEvents;
using Api.Repositories;
using Api.Security;
using Api.State;
using Fleck;
using lib;
using Serilog;

namespace Api.ClientEventHandlers;

public class ClientWantsToSignInDto : BaseDto
{
    public string email { get; set; }

    public string password { get; set; }
}

public class ClientWantsToAuthenticate(
    ChatRepository chatRepository,
    TokenService tokenService,
    CredentialService credentialService)
    : BaseEventHandler<ClientWantsToSignInDto>
{
    public override Task Handle(ClientWantsToSignInDto request, IWebSocketConnection socket)
    {

        var user = chatRepository.GetUser(new FindByEmailParams(request.email!));
        if (user.isbanned) throw new AuthenticationException("User is banned");
        var expectedHash = credentialService.Hash(request.password!, user.salt!);
        if (!expectedHash.Equals(user.hash)) throw new AuthenticationException("Wrong credentials!");
        WebSocketStateService.GetClient(socket.ConnectionInfo.Id).IsAuthenticated = true;
        WebSocketStateService.GetClient(socket.ConnectionInfo.Id).User = user;
        socket.SendDto(new ServerAuthenticatesUser { jwt = tokenService.IssueJwt(user) });
        return Task.CompletedTask;
    }
}
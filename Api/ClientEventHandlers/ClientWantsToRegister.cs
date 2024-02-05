using System.ComponentModel.DataAnnotations;
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

public class ClientWantsToRegisterDto : BaseDto
{
    [EmailAddress] public string email { get; set; }

    [MinLength(6)] public string password { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsToRegister(
    ChatRepository chatRepository,
    CredentialService credentialService,
    WebSocketStateService stateService,
    TokenService tokenService
) : BaseEventHandler<ClientWantsToRegisterDto>
{
    public override Task Handle(ClientWantsToRegisterDto dto, IWebSocketConnection socket)
    {
        if (chatRepository.DoesUserAlreadyExist(new FindByEmailParams(dto.email)))
            throw new ValidationException("User with this email already exists");
        var salt = credentialService.GenerateSalt();
        var hash = credentialService.Hash(dto.password, salt);
        var user = chatRepository.InsertUser(new InsertUserParams(dto.email, hash, salt));
        var token = tokenService.IssueJwt(user);
        stateService.GetClient(socket.ConnectionInfo.Id).IsAuthenticated = true;
        socket.SendDto(new ServerAuthenticatesUser { jwt = token });
        return Task.CompletedTask;
    }
}
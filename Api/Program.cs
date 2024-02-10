using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Authentication;
using Api.Helpers.cs;
using Api.Models.ServerEvents;
using Api.Repositories;
using Api.Security;
using Api.State;
using Fleck;
using lib;
using Serilog;

namespace Api;

public static class Startup
{
    public static void Main(string[] args)
    {
        var webApp = Start(args);
        webApp.Run();
    }
    
    public static WebApplication Start(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(
                outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton<CredentialService>();
        builder.Services.AddSingleton<TokenService>();

        builder.Services.AddNpgsqlDataSource(ChatRepository.ProperlyFormattedConnectionString,
            sourceBuilder => { sourceBuilder.EnableParameterLogging(); });
        builder.Services.AddSingleton<ChatRepository>();
        var services = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());


        var app = builder.Build();
        app.Services.GetService<ChatRepository>()!.ExecuteRebuildFromSqlScript();
        var server = new WebSocketServer("ws://0.0.0.0:8181");
        server.RestartAfterListenError = true;
        server.Start(socket =>
        {
            socket.OnOpen = () => WebSocketStateService.AddClient(socket.ConnectionInfo.Id, socket);
            socket.OnClose = () => WebSocketStateService.RemoveClient(socket.ConnectionInfo.Id);
            socket.OnMessage = async message =>
            {
                try
                {
                    await app.InvokeClientEventHandler(services, socket, message);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Global exception handler");
                    if (app.Environment.IsProduction() && (e is ValidationException || e is AuthenticationException))
                    {
                        socket.SendDto(new ServerSendsErrorMessageToClient()
                        {
                            errorMessage = "Something went wrong",
                            receivedMessage = message
                        });
                    }
                    else
                    {
                        socket.SendDto(new ServerSendsErrorMessageToClient
                            { errorMessage = e.Message, receivedMessage = message });
                    }
                }
            };
        });
        return app;
    }
}
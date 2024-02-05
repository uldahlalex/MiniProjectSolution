using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Authentication;
using Api;
using Api.Helpers.cs;
using Api.ServerEvents;
using api.State;
using Fleck;
using lib;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<CredentialService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<WebSocketStateService>();

builder.Services.AddNpgsqlDataSource(ChatRepository.ProperlyFormattedConnectionString, sourceBuilder =>
{
    sourceBuilder.EnableParameterLogging();
});
builder.Services.AddSingleton<ChatRepository>();
var services = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());

var app = builder.Build();
var state = app.Services.GetService<WebSocketStateService>()!;
var server = new WebSocketServer("ws://0.0.0.0:8181");
server.RestartAfterListenError = true;
server.Start(socket =>
{
    socket.OnOpen = () => state.AddClient(socket.ConnectionInfo.Id, socket);
    socket.OnClose = () => state.RemoveClient(socket.ConnectionInfo.Id);
    socket.OnMessage = async message =>
    {
        try
        {
            await app.InvokeClientEventHandler(services, socket, message);
        }
        catch (Exception e)
        {
            if (app.Environment.IsProduction() && e is not ValidationException or AuthenticationException)
            {
                Log.Error(e, "Global exception handler");
                socket.SendDto(new ServerSendsErrorMessageDto(){message = "Something went wrong"});
            }
            else
            {
                Log.Error(e, "Global exception handler");
                socket.SendDto(new ServerSendsErrorMessageDto(){message = e.Message});
            }
        }
    };
});
app.Run();
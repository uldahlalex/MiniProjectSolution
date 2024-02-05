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

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<CredentialService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<WebSocketStateService>();

builder.Services.AddNpgsqlDataSource(ChatRepository.ProperlyFormattedConnectionString,
    sourceBuilder => { sourceBuilder.EnableParameterLogging(); });
builder.Services.AddSingleton<ChatRepository>();
var services = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());

// Build the IServiceProvider from the DI container here
var serviceProvider = builder.Services.BuildServiceProvider();
// Set the IServiceProvider in the ServiceLocator
ServiceLocator.SetServiceProvider(serviceProvider);

// Use the built serviceProvider to resolve services
var app = builder.Build();
app.Services.GetService<ChatRepository>().ExecuteRebuildFromSqlScript();
var state = serviceProvider.GetService<WebSocketStateService>()!;
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
            Log.Error(e, "Global exception handler");
            if(app.Environment.IsProduction() && (e is ValidationException || e is AuthenticationException))
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
app.Run();

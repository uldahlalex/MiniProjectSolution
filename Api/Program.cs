using System.Reflection;
using Api;
using Fleck;
using lib;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNpgsqlDataSource(ChatRepository.ProperlyFormattedConnectionString, sourceBuilder =>
{
    sourceBuilder.EnableParameterLogging();
});
var services = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());
var app = builder.Build();
var server = new WebSocketServer("ws://0.0.0.0:8181");
server.RestartAfterListenError = true;
server.Start(socket =>
{
    socket.OnOpen = () => Console.WriteLine("Open!");
    socket.OnClose = () => Console.WriteLine("Close!");
    socket.OnMessage = async message =>
    {
        try
        {
            await app.InvokeClientEventHandler(services, socket, message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException);
            Console.WriteLine(e.StackTrace);
        }
    };
});
app.Run();
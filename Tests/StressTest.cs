using Api;
using Api.ClientEventHandlers;
using Api.Models.ServerEvents;
using NUnit.Framework;

namespace Tests;

[TestFixture]
[NonParallelizable]
public class StressTest
{
    
    [SetUp]
    public async Task Setup()
    {
        Startup.Start(null);
    }
    [Test]
    public async Task ManyEnterRooms()
    {
        var ws = await new WebSocketTestClient().ConnectAsync();

        await ws.DoAndAssert(new ClientWantsToSignInDto() { email = "bla@bla.dk", password = "qweqweqwe" },
            receivedMessages => receivedMessages.Count(e => e.eventType == nameof(ServerAuthenticatesUser)) == 1);

        var iterations = 200;
        for (var i = 1; i < iterations; i++)
            await ws.DoAndAssert(new ClientWantsToEnterRoomDto() { roomId = i });
        Task.Delay(10000).Wait();
        await ws.DoAndAssert(new ClientWantsToEnterRoomDto() { roomId = iterations },
            receivedMessages =>
            {
                return receivedMessages.Count(e => e.eventType == nameof(ServerAddsClientToRoom)) == iterations;
            });

    }
}
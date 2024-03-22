using System.Net.WebSockets;
using System.Text.Json;
using Api.ClientEventHandlers;
using Api.Models.ParameterModels;
using Api.Models.QueryModels;
using Api.Models.ServerEvents;
using Api.Repositories;
using Api.State;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;

namespace Api;

public class MQTTClientService(ChatRepository chatRepository)
{
    public async Task CommunicateWithBroker()
    {

        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();
        
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .Build();

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic("Messages"))
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                var message = e.ApplicationMessage.ConvertPayloadToString();
                Console.WriteLine("Received message: " + message);
                var messageObject = JsonSerializer.Deserialize<MqttClientWantsToSendMessageToRoom>(message);
                var timestamp = DateTimeOffset.UtcNow;
                var insertionResult = chatRepository.InsertMessage(new InsertMessageParams(
                    messageObject.message, timestamp, messageObject.sender, 1));
                WebSocketStateService.BroadcastMessage(1, new ServerBroadcastsMessageToClientsInRoom()
                {
                    message = new MessageWithSenderEmail()
                    {
                        sender = messageObject.sender,
                        timestamp = timestamp,
                        messageContent = messageObject.message,
                        room = 1,
                        email = "mqtt client",
                        id = insertionResult.id
                    }
                });
                var pongMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("response_topic")
                    .WithPayload("yes we received the message, thank you very much, " +
                                 "the websocket client(s) also has the data")
                    .WithQualityOfServiceLevel(e.ApplicationMessage.QualityOfServiceLevel)
                    .WithRetainFlag(e.ApplicationMessage.Retain)
                    .Build();
                await mqttClient.PublishAsync(pongMessage, CancellationToken.None);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.InnerException);
                Console.WriteLine(exc.StackTrace);
            }
        };
    
    }
}

public class MqttClientWantsToSendMessageToRoom
{
    public string message { get; set; }
    public int sender { get; set; }
}
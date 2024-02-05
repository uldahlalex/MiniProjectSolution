namespace Api.Models.ParameterModels;

public class GetPastMessagesParams(int room, int lastMessageId = int.MaxValue)
{
    public int room { get; private set; } = room;
    public int lastMessageId { get; private set; } = lastMessageId;
}
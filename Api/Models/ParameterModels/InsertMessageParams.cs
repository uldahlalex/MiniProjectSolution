namespace Externalities.ParameterModels;

public class InsertMessageParams(string? messageContent, DateTimeOffset timestamp, int sender, int room)
{
    public string? messageContent { get; set; } = messageContent;
    public DateTimeOffset timestamp { get; set; } = timestamp;
    public int sender { get; set; } = sender;
    public int room { get; set; } = room;
}
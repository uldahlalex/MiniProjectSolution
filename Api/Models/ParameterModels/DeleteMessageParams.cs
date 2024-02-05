namespace Externalities.ParameterModels;

public class DeleteMessageParams(int messageId)
{
    public int messageId { get; set; } = messageId;
}
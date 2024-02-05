namespace Externalities.ParameterModels;

public class IsMessageOwnerParams(int userId, int messageId)
{
    public int userId { get; set; } = userId;
    public int messageId { get; set; } = messageId;
}
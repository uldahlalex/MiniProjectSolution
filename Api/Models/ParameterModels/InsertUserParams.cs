namespace Api.Models.ParameterModels;

public class InsertUserParams(string email, string hash, string salt)
{
    public string email { get; private set; } = email;
    public string hash { get; private set; } = hash;
    public string salt { get; private set; } = salt;
}
using Serilog;

namespace Api.Helpers.cs;

public enum ENV_VAR_KEYS
{
    ASPNETCORE_ENVIRONMENT,
    PORT,
    AZ_VISION,
    AZ_CONTENT_FILTER,
    JWT_KEY,
    PG_CONN
}

public class EnvSetup
{
    public static void SetupEnv()
    {
        foreach (var KEY in (ENV_VAR_KEYS[])Enum.GetValues(typeof(ENV_VAR_KEYS)))
        {
            Log.Information("ENV VAR: ''{0}'' = ''{1}''", KEY, Environment.GetEnvironmentVariable(KEY.ToString()) ?? "null");
        }
    }
}
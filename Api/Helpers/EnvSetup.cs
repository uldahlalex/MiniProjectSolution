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
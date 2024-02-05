namespace Api.Helpers.cs;

public static class ServiceLocator
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}
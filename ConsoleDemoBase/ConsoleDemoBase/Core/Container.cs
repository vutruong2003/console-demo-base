namespace ConsoleDemoBase.Core;
public class Container
{
    private static Dictionary<string, Container> Instances = new();
    private const string Default_Identify = "Main_Container";

    private bool _disposed = false;
    private string _identifier { get; set; }
    public ServiceProvider ServiceProvider { get; set; }

    public event EventHandler<BuildMessage> OnRegister;

    private Container(string identifier)
    {
        _identifier = identifier;
    }

    public static Container GetInstance(string identify = null)
    {
        if (string.IsNullOrEmpty(identify))
        {
            identify = Default_Identify;
        }

        Instances.TryGetValue(identify, out var instance);

        if (instance is null)
        {
            instance = new Container(identify);
            Instances.TryAdd(identify, instance);
        }

        return instance;
    }

    public Runner GetRunner()
    {
        return Runner.GetRunner(this, _identifier);
    }

    public Runner Build(Action<IServiceCollection> startupAction = null)
    {
        var serviceCollection = new ServiceCollection();

        var runner = GetRunner();

        var configuration = new ConfigurationBuilder()
            .Build();

        if (startupAction is not null)
        {
            startupAction(serviceCollection);
        }

        OnRegister?.Invoke(this, new(serviceCollection, configuration));

        ServiceProvider = serviceCollection
                .BuildServiceProvider();

        return runner;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (ServiceProvider is not null)
        {
            ServiceProvider.Dispose();
            Instances.Remove(_identifier);

            _disposed = true;
        }
    }
}

using Spectre.Console;

namespace ConsoleDemoBase.Core;
public class Runner
{
    private static Dictionary<string, Runner> Instances = new();
    private const string Default_Identify = "Main_Container";

    private Container _container;
    private string _identifier { get; set; }
    private bool _disposed = false;

    private Runner(Container container, string identifier)
    {
        _container = container;
        _identifier = identifier;
        _container.OnRegister += Container_OnRegister;
    }

    public static Runner GetRunner(Container container, string identify)
    {
        if (string.IsNullOrEmpty(identify))
        {
            identify = Default_Identify;
        }

        Instances.TryGetValue(identify, out var instance);

        if (instance is null)
        {
            instance = new Runner(container, identify);
            Instances.TryAdd(identify, instance);
        }

        return instance;
    }

    private void Container_OnRegister(object sender, BuildMessage e)
    {
        var exampleType = typeof(IExample);

        // Register
        IList<Type> allExampleTypes = GetImplementations(typeof(IExample));

        foreach (var type in allExampleTypes)
        {
            e.Services.AddScoped(exampleType, type);
        }
    }

    public void Execute()
    {
        ExecuteAsync().Wait();
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        do
        {
            using var scoped = _container.ServiceProvider.CreateScope();
            var examples = scoped.ServiceProvider.GetServices<IExample>().ToList();

            if (examples is null || examples.Count == 0)
            {
                AnsiConsole.WriteLine("No function found.");

                return;
            }

            var allExamples = examples
                .Select(type => new
                {
                    type.GetType().Name,
                    Type = type
                })
                .OrderBy(x => x.Name)
                .Select((type, index) =>
                {
                    var name = type.Name;

                    return new
                    {
                        Order = index + 1,
                        Name = name,
                        Instance = type.Type
                    };
                })
                .Where(x => x.Order > 0)
                .OrderBy(x => x.Order)
                .ToDictionary(x => x.Order.ToString(), x => x);

            AnsiConsole.Clear();

            var choices = allExamples
                .OrderBy(x => x.Value.Order)
                .Select(x => new PromptMenuItem(x.Value.Order, x.Value.Name))
                .ToList();

            var chosen = AnsiConsole.Prompt(
                new SelectionPrompt<PromptMenuItem>()
                    .Title("Pick a [green]function[/]:")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more function)[/]")
                    .AddChoices(choices)
                    .AddChoices(new PromptMenuItem(-1, "Exit"))
            );

            if (chosen.Id == -1)
            {
                break;
            }

            AnsiConsole.Clear();

            if (allExamples.TryGetValue($"{chosen.Id}", out var functionType))
            {
                var panel = new Panel($"{functionType.Name}");
                panel.Border = BoxBorder.Rounded;
                panel.Expand();

                AnsiConsole.Write(panel);
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();

                await AnsiConsole.Status()
                    .StartAsync($"Running {functionType.Name}...",
                        async ctx =>
                        {
                            try
                            {
                                await functionType.Instance.ExecuteAsync(cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                AnsiConsole.WriteException(ex);
                            }
                        }
                    );

                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();

                panel = new Panel($"{functionType.Name} end.");
                panel.Border = BoxBorder.Rounded;
                panel.Expand();

                AnsiConsole.Write(panel);

                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();

                AnsiConsole.Prompt(
                    new TextPrompt<string>("Press enter to continue...")
                        .AllowEmpty());
            }
        }
        while (true);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Instances.Remove(_identifier);

        _disposed = true;
    }

    private List<Type> GetImplementations(Type targetType)
    {
        var assembly = Assembly.GetEntryAssembly();
        var allExamples = assembly
            .GetTypes()
            .Where(t => t.IsInterface == false && t.IsAssignableTo(targetType))
            .ToList();

        return allExamples;
    }

    private readonly static List<string> exitSigns = ["0", "exit"];
}
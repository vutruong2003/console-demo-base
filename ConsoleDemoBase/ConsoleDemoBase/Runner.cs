using System.Reflection;

namespace ConsoleDemoBase;
public class Runner
{
    private static List<Type> GetImplementations(Type targetType)
    {
        var assembly = Assembly.GetEntryAssembly();
        var allExamples = assembly
            .GetTypes()
            .Where(t => t.IsInterface == false && t.IsAssignableTo(targetType))
            .ToList();

        return allExamples;
    }

    private readonly static List<string> exitSigns = ["0", "exit"];

    public static void Execute()
    {
        do
        {
            IList<Type> allExampleTypes = GetImplementations(typeof(IExample));

            if (allExampleTypes is null || allExampleTypes.Count == 0)
            {
                Console.WriteLine("No function found.");

                return;
            }

            //Console.Clear();
            var allExamples = allExampleTypes
                .Select(type => new
                {
                    type.Name,
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
                        type.Type
                    };
                })
                .Where(x => x.Order > 0)
                .OrderBy(x => x.Order)
                .ToDictionary(x => x.Order.ToString(), x => x);

            foreach (var example in allExamples)
            {
                Console.WriteLine($"{example.Value.Order}. {example.Value.Name}");
            }

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Pick a function by number: (enter 0 or exit to cancel)");

            var chosen = (Console.ReadLine() ?? "1").Trim().Replace(" ", "").ToLower();
            Console.Clear();

            if (allExamples.TryGetValue(chosen, out var functionType))
            {
                Console.WriteLine("------------------------");
                Console.WriteLine($"Running {functionType.Name}...");
                Console.WriteLine("------------------------");
                Console.WriteLine();

                functionType.Type.GetMethod(nameof(IExample.Execute))?.Invoke(null, null);

                Console.WriteLine();
                Console.WriteLine("------------------------");
                Console.WriteLine($"{functionType.Name} end.");
                Console.WriteLine("------------------------");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }

            if (exitSigns.Contains(chosen))
            {
                return;
            }
        }
        while (true);
    }
}
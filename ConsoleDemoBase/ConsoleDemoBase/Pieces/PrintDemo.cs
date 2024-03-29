namespace ConsoleDemoBase.Pieces;
public class PrintDemo : IExample
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.WriteLine("This is sample for IExample running by assembly scanner");

        return Task.CompletedTask;
    }
}

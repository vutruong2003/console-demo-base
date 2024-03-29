# How it works

Adding a class to Pieces folder, which implement IExample interface. After the ExecuteAsync implemented, the function will be automatically added to main menu


# Example

```c#
// MyDemo.cs

public class MyDemo : IExample
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Hello world");

        return Task.CompletedTask;
    }
}

```

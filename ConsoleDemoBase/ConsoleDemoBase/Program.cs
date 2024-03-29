var container = Container.GetInstance();

container.OnRegister += (_, messages) =>
{
    var (services, configuration) = messages;
};

var runner = container.Build();

await runner.ExecuteAsync();

runner.Dispose();
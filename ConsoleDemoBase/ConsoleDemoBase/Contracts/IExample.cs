﻿namespace ConsoleDemoBase.Contracts;
public interface IExample
{
    void Execute() => ExecuteAsync().Wait();

    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

using ConsoleApp.Models.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.Tasks;

public class ThreadBackendTask : IBackendTask
{
    protected readonly ILogger _logger;

    protected virtual int ItemsCount => 100;

    protected record ThreadTaskItemConfig(int Number);
    protected record ThreadTaskItemResult(string Message);

    public ThreadBackendTask(ILogger logger)
    {
        _logger = logger;
    }

    public virtual async Task RunAsync()
    {
        var configs = CreateConfigs(ItemsCount);
        var results = await ExecuteAsync(configs);
        WriteResults(results);
    }

    protected virtual IEnumerable<ThreadTaskItemConfig> CreateConfigs(int count)
    {
        return Enumerable.Range(0, count).Select(CreateConfig);
    }

    protected virtual ThreadTaskItemConfig CreateConfig(int number)
    {
        return new ThreadTaskItemConfig(number);
    }

    protected virtual async Task<IEnumerable<ThreadTaskItemResult>> ExecuteAsync(
      IEnumerable<ThreadTaskItemConfig> configs)
    {
        var tasks = configs.Select(ExecuteAsync);
        return await Task.WhenAll(tasks);
    }

    protected virtual async Task<ThreadTaskItemResult> ExecuteAsync(ThreadTaskItemConfig config)
    {
        var message = $"Message {config.Number}";
        await Task.Delay(100);
        return new ThreadTaskItemResult(message);
    }

    protected virtual void WriteResults(IEnumerable<ThreadTaskItemResult> results)
    {
        foreach (var result in results)
        {
            WriteResult(result);
        }
    }

    protected virtual void WriteResult(ThreadTaskItemResult result)
    {
        _logger.LogInformation(result.Message);
    }

    Task IBackendTask.RunAsync()
    {
        throw new NotImplementedException();
    }
}

using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace ConsoleApp.Tasks
{
    public class ThreadBackendLimitedTask : ThreadBackendTask
    {
        // !!! NOTICE
        // Not sure if the task was about items count (because classes are named Thread...) or threads, so both are done
        // Please uncomment to see items limitation if it was required

        // protected override int ItemsCount => 5;
        protected override int ItemsCount => 100;
        protected int ThreadsCount = 5;

        public ThreadBackendLimitedTask(ILogger logger) : base(logger)
        {
        }

        // TPL based version, limits nr of threads exactly to 5
        protected override async Task<IEnumerable<ThreadTaskItemResult>> ExecuteAsync(IEnumerable<ThreadTaskItemConfig> configs)
        {
            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = ThreadsCount
            };
            ImmutableList<ThreadTaskItemResult> results = ImmutableList<ThreadTaskItemResult>.Empty;


            await Parallel.ForEachAsync(configs, parallelOptions: options, async (config, ct) =>
            {
                // Task.Run definitely runs in background thread, so UI won't be frozen
                var res = await Task.Run(() => ExecuteAsync(config));
                results.Add(res);
                _logger.LogInformation("Executed a batch");
            });

            return results.AsEnumerable();
        }

        // Task based version, limits to 5 threads but might be less nr of threads!
        //protected override async Task<IEnumerable<ThreadTaskItemResult>> ExecuteAsync(IEnumerable<ThreadTaskItemConfig> configs)
        //{
        //    var semaphore = new SemaphoreSlim(1, ThreadsCount + 1);

        //    // Task.Run definitely runs in background thread, so UI won't be frozen
        //    var tasks = configs.Select(config => Task.Run(async () =>
        //    {
        //        try
        //        {
        //            await semaphore.WaitAsync();
        //            _logger.LogInformation("Started the task " + config);
        //            return await ExecuteAsync(config);
        //        }
        //        finally
        //        {
        //            semaphore.Release();
        //            await Task.Delay(1);

        //            _logger.LogInformation("Released semaphore");
        //        }
        //    }));

        //    return await Task.WhenAll(tasks);
        //}
    }
}
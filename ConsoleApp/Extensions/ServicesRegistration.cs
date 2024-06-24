using ConsoleApp.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.Extensions
{
    public static class ServicesRegistration
    {
        public static ServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddTransient<ThreadBackendTask, ThreadBackendLimitedTask>()
                .AddTransient<ExpressionBackendTask, ExpressionBackendLimitedTask>()
                .AddSingleton<ILogger>(LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                }).CreateLogger("Console App logs"))
                .BuildServiceProvider();
        }
    }
}

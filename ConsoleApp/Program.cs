using ConsoleApp.Extensions;
using ConsoleApp.Tasks;
using Microsoft.Extensions.DependencyInjection;

using (var serviceScope = ServicesRegistration.BuildServiceProvider().CreateScope())
{
    var serviceProvider = serviceScope.ServiceProvider;

    var task1 = serviceProvider.GetRequiredService<ThreadBackendTask>();
    await task1.RunAsync();

    var task2 = serviceProvider.GetRequiredService<ExpressionBackendTask>();
    await task2.RunAsync();
}
using Consumer.ActiveMQ.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var commandArgs = Environment.GetCommandLineArgs();
if (commandArgs != null && commandArgs.Length > 1)
{
    var builder = Host.CreateDefaultBuilder()
    .UseConsoleLifetime();
    if (commandArgs[1] == "1")
    {
        builder.ConfigureServices((context, services) => services.AddHostedService<TransferConstMessagesNumberTestService>()).Build().Run();
    }
    else if (commandArgs[1] == "3")
    {
        builder.ConfigureServices((context, services) => services.AddHostedService<ConstMessagesSizeSumTestService>()).Build().Run();
    }
    if (commandArgs[1] == "4" && int.TryParse(commandArgs[2], out int numberOfProducers))
    {
        builder.ConfigureServices((context, services) => services.AddHostedService(sp => new LatencyTestService(numberOfProducers))).Build().Run();
    }
    else
    {
        Console.WriteLine("No arguments have been provided");
    }
}
else
{
    Console.WriteLine("No arguments have been provided");
}
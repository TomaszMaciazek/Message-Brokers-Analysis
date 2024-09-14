using Consumer.Kafka.Services;
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
        builder.ConfigureServices((context, services) => services.AddHostedService<TransferConstPacketSizeTestService>()).Build().Run();
    }
    else if (commandArgs[1] == "4")
    {
        builder.ConfigureServices((context, services) => services.AddHostedService<LatencySingleProducerTestService>()).Build().Run();
    }
    else if (commandArgs[1] == "5")
    {
        builder.ConfigureServices((context, services) => services.AddHostedService<LatencyMultipleProducersTestService>()).Build().Run();
    }
}
else
{
    Console.WriteLine("No arguments have been provided");
}
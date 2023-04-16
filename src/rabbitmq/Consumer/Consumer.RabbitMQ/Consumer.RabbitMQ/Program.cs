using Consumer.RabbitMQ.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var commandArgs = Environment.GetCommandLineArgs();
if (commandArgs != null && commandArgs.Length > 1)
{
    var builder = Host.CreateDefaultBuilder()
    .UseConsoleLifetime();
    if (commandArgs[1] == "1")
    {
        builder = builder.ConfigureServices((context, services) => services.AddHostedService<SingleMessageTestService>());
    }
    else if (commandArgs[1] == "2")
    {
        builder = builder.ConfigureServices((context, services) => services.AddHostedService<TransferMessagesTimeTestService>());
    }
    else if (commandArgs[1] == "3")
    {
        builder = builder.ConfigureServices((context, services) => services.AddHostedService<TransferPacketTestService>());
    }
    else if (commandArgs[1] == "4")
    {
        builder = builder.ConfigureServices((context, services) => services.AddHostedService<LatencyTestService>());
    }

    else if (commandArgs[1] == "5")
    {
        builder = builder.ConfigureServices((context, services) => services.AddHostedService<BreakdownTestService>());
    }
    builder.Build().Run();
}
else
{
    Console.WriteLine("No arguments have been provided");
}

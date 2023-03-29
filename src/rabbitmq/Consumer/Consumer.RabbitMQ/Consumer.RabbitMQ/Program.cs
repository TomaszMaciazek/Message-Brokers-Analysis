using Consumer.RabbitMQ.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var commandArgs = Environment.GetCommandLineArgs();
var builder = Host.CreateDefaultBuilder()
    .UseConsoleLifetime();
if (commandArgs != null && commandArgs.Length > 1)
{
    if (commandArgs[1] == "1")
    {
        builder = builder.ConfigureServices((context, services) => services.AddHostedService<SingleMessageTestService>());
    }
    else if (commandArgs[1] == "2")
    {
        if(int.TryParse(commandArgs[2], out int numberOfProducers))
        {
            Console.WriteLine($"{numberOfProducers} producers");
            builder = builder.ConfigureServices((context, services) => services.AddHostedService(sp => new TransferPacketTestService(numberOfProducers)));
        }
        else
        {
            Console.WriteLine("number of producers was not provided");
        }
    }

    else if (commandArgs[1] == "3")
    {
        if (int.TryParse(commandArgs[2], out int numberOfProducers))
        {
            Console.WriteLine($"{numberOfProducers} producers");
            builder = builder.ConfigureServices((context, services) => services.AddHostedService(sp => new LatencyTestService(numberOfProducers)));
        }
        else
        {
            Console.WriteLine("number of producers was not provided");
        }
    }

    else if (commandArgs[1] == "4")
    {
        builder = builder.ConfigureServices((context, services) => services.AddHostedService(sp => new BreakdownTestService()));
    }
}
else
{
    builder = builder.ConfigureServices((context, services) => services.AddHostedService<SingleMessageTestService>());
}

builder.Build().Run();
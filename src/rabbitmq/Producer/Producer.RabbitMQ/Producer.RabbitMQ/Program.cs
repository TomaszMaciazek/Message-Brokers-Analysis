using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Producer.RabbitMQ.Service;

var commandArgs = Environment.GetCommandLineArgs();
if (commandArgs != null && commandArgs.Length > 1)
{
    var builder = Host.CreateDefaultBuilder()
    .UseConsoleLifetime();
    if (commandArgs[1] == "1" && commandArgs.Length >= 5)
    {
        if (int.TryParse(commandArgs[2], out int numberOfMessages) && int.TryParse(commandArgs[3], out int messageSize) && bool.TryParse(commandArgs[4], out bool isSendingFanoutMessage))
        {
            builder.ConfigureServices((context, services) => 
                services.AddHostedService(sp => new TransferConstMessagesNumberTestService(numberOfMessages, isSendingFanoutMessage, messageSize))
            ).Build().Run();
        }
    }
    else if (commandArgs[1] == "3" && commandArgs.Length >= 5)
    {
        if (long.TryParse(commandArgs[2], out long size) && int.TryParse(commandArgs[3], out int messageSize) && bool.TryParse(commandArgs[4], out bool isSendingFanoutMessage ))
        {
            builder.ConfigureServices((context, services) => 
                services.AddHostedService(sp => new TransferPacketTestService(size, messageSize, isSendingFanoutMessage))
            ).Build().Run();
        }
        else
        {
            Console.WriteLine("Provided paramters are not correct");
        }
    }
    else if (commandArgs[1] == "4")
    {
        if (int.TryParse(commandArgs[2], out int sizeInBytes) && bool.TryParse(commandArgs[3], out bool isSendingFanoutMessage) && int.TryParse(commandArgs[4], out int messageSize))
        {
            builder.ConfigureServices((context, services) => 
                services.AddHostedService(sp => new LatencyTestService(sizeInBytes, isSendingFanoutMessage, messageSize))
            ).Build().Run();
        }
        else
        {
            Console.WriteLine("Provided paramters are not correct");
        }
    }
    else
    {
        Console.WriteLine("Provided paramters are not correct");
    }
}
else
{
    Console.WriteLine("No arguments have been provided");
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Producer.RabbitMQ.Service;

var commandArgs = Environment.GetCommandLineArgs();
if (commandArgs != null && commandArgs.Length > 1)
{
    var builder = Host.CreateDefaultBuilder()
    .UseConsoleLifetime();
    if (commandArgs[1] == "1" && commandArgs.Length >= 4)
    {
        if (int.TryParse(commandArgs[2], out int numberOfMessages) && bool.TryParse(commandArgs[3], out bool isSendingFanoutMessage))
        {
            builder.ConfigureServices((context, services) => 
                services.AddHostedService(sp => new TransferConstMessagesNumberTestService(numberOfMessages, isSendingFanoutMessage))
            ).Build().Run();
        }
    }
    else if (commandArgs[1] == "2" && commandArgs.Length >= 4)
    {
        if (int.TryParse(commandArgs[2], out int timeInMiliseconds) && bool.TryParse(commandArgs[3], out bool isSendingFanoutMessage))
        {
            builder.ConfigureServices((context, services) =>
                services.AddHostedService(sp => new TransferMessagesTimeTestService(timeInMiliseconds, isSendingFanoutMessage))
            ).Build().Run();
        }
    }
    else if (commandArgs[1] == "3" && commandArgs.Length >= 4)
    {
        if (long.TryParse(commandArgs[2], out long size) && bool.TryParse(commandArgs[3], out bool isSendingFanoutMessage ))
        {
            builder.ConfigureServices((context, services) => 
                services.AddHostedService(sp => new TransferPacketTestService(size, isSendingFanoutMessage))
            ).Build().Run();
        }
        else
        {
            Console.WriteLine("Provided paramters are not correct");
        }
    }
    else if (commandArgs[1] == "4")
    {
        if (int.TryParse(commandArgs[2], out int sizeInBytes) && bool.TryParse(commandArgs[3], out bool isSendingFanoutMessage))
        {
            builder.ConfigureServices((context, services) => 
                services.AddHostedService(sp => new LatencyConstantSizeTestService(sizeInBytes, isSendingFanoutMessage))
            ).Build().Run();
        }
        else
        {
            Console.WriteLine("Provided paramters are not correct");
        }
    }

    else if (commandArgs[1] == "5")
    {
        if (int.TryParse(commandArgs[2], out int numberOfMessages))
        {
            builder.ConfigureServices((context, services) => 
                services.AddHostedService(sp => new BreakdownTestService(numberOfMessages))
            ).Build().Run();
        }
        else
        {
            Console.WriteLine("Number of messages was not provided");
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

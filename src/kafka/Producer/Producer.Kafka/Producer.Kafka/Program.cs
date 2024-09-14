using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Producer.Kafka.Service;

var commandArgs = Environment.GetCommandLineArgs();
if (commandArgs != null && commandArgs.Length > 1)
{
    var builder = Host.CreateDefaultBuilder()
    .UseConsoleLifetime();
    if (commandArgs[1] == "1" && commandArgs.Length >= 4)
    {
        if (int.TryParse(commandArgs[2], out int numberOfMessages) && int.TryParse(commandArgs[3], out int messageSize))
        {
            builder.ConfigureServices((context, services) =>
                services.AddHostedService(sp => new TransferConstMessagesNumberTestService(numberOfMessages, messageSize))
            ).Build().Run();
        }
    }
    else if (commandArgs[1] == "2" && commandArgs.Length >= 4)
    {
        if (int.TryParse(commandArgs[2], out int numberOfMessages) && int.TryParse(commandArgs[3], out int messageSize))
        {
            builder.ConfigureServices((context, services) =>
                services.AddHostedService(sp => new TransferConstMessagesNumberMultipleConsumersTestService(numberOfMessages, messageSize))
            ).Build().Run();
        }
    }
    else if (commandArgs[1] == "3")
    {
        if (long.TryParse(commandArgs[2], out long size) && int.TryParse(commandArgs[3], out int messageSize))
        {
            builder.ConfigureServices((context, services) =>
                services.AddHostedService(sp => new TransferConstMessagesSizeSumTestService(size, messageSize))
            ).Build().Run();
        }
        else
        {
            Console.WriteLine("Provided paramters are not correct");
        }
    }
    else if (commandArgs[1] == "4")
    {
        if (int.TryParse(commandArgs[2], out int sizeInBytes) && int.TryParse(commandArgs[3], out int messageSize))
        {
            builder.ConfigureServices((context, services) =>
                services.AddHostedService(sp => new LatencyTestService(sizeInBytes, messageSize))
            ).Build().Run();
        }
        else
        {
            Console.WriteLine("Provided paramters are not correct");
        }
    }
    else if (commandArgs[1] == "5")
    {
        if (long.TryParse(commandArgs[2], out long size) && int.TryParse(commandArgs[3], out int messageSize))
        {
            builder.ConfigureServices((context, services) =>
                services.AddHostedService(sp => new TransferConstMessagesSizeSumTestMultipleConsumersService(size, messageSize))
            ).Build().Run();
        }
        else
        {
            Console.WriteLine("Provided paramters are not correct");
        }
    }
    else if (commandArgs[1] == "6")
    {
        if (int.TryParse(commandArgs[2], out int sizeInBytes) && int.TryParse(commandArgs[3], out int messageSize))
        {
            builder.ConfigureServices((context, services) =>
                services.AddHostedService(sp => new LatencyTestMultipleConsumersService(sizeInBytes, messageSize))
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
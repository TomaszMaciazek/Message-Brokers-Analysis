using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Producer.ActiveMQ.Service;

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
                services.AddHostedService(sp => new TransferConstMessagesNumberTestService(numberOfMessages,messageSize))
            ).Build().Run();
        }
    }
    else if (commandArgs[1] == "3" && commandArgs.Length >= 4)
    {
        if (int.TryParse(commandArgs[2], out int size) && int.TryParse(commandArgs[3], out int messageSize))
        {
            builder.ConfigureServices((context, services) =>
                services.AddHostedService(sp => new ConstMessagesSizeSumTestService(size, messageSize))
            ).Build().Run();
        }
    }
    else if (commandArgs[1] == "4" && commandArgs.Length >= 4)
    {
        if (int.TryParse(commandArgs[2], out int size) && int.TryParse(commandArgs[3], out int messageSize))
        {
            builder.ConfigureServices((context, services) =>
                services.AddHostedService(sp => new LatencyTestService(size, messageSize))
            ).Build().Run();
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
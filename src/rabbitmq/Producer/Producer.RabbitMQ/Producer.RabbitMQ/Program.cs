using Producer.RabbitMQ;
using Producer.RabbitMQ.Service;

var commandArgs = Environment.GetCommandLineArgs();

if (commandArgs != null && commandArgs.Length > 1)
{
    if (commandArgs[1] == "1")
    {

    }
    else if (commandArgs[1] == "2")
    {
        var parsedBytesSize = int.TryParse(commandArgs[2], out var value);
        var parsedSize = int.TryParse(commandArgs[3], out var size);
        if (parsedBytesSize && parsedSize)
        {
            TransferPacketTestService.RunPacketTransfer(BytesGenerator.GetByteArray(value), size);
        }
    }
    else if (commandArgs[1] == "3")
    {
        var parsedBytesSize = int.TryParse(commandArgs[2], out var value);
        var parsedNumberOfMessages = int.TryParse(commandArgs[3], out var numberOfMessages);
        if (parsedBytesSize && parsedNumberOfMessages)
        {
            LatencyConstantSizeTestService.Run(BytesGenerator.GetByteArray(value), numberOfMessages);
        }
    }
    else if (commandArgs[1] == "4")
    {
        var parsedNumberOfMessages = int.TryParse(commandArgs[2], out var numberOfMessages);
        if (parsedNumberOfMessages)
        {
            BreakdownTestService.Run(numberOfMessages);
        }
    }
}
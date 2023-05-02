using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Producer.RabbitMQ.Service
{
    public class TransferPacketMultipleQueueTestService : IHostedService
    {
        private const string topicExchangeName = "transfer-packet-exchange";
        private const string fanoutExchangeName = "transfer-packet-result-exchange";

        private readonly long _size;
        private readonly bool _isSendingFanoutMessage;
        private readonly int _numberOfConsumers;
        private readonly List<int> _byteSizes = new()
        {
            250, 1000, 4000,  16000, 64000, 256000, 1000000
        };

        private readonly IConnection conn;
        private readonly IModel channel;

        public TransferPacketMultipleQueueTestService(long size, int numberOfConsumers, bool isSendingFanoutMessage)
        {
            _size = size;
            _isSendingFanoutMessage = isSendingFanoutMessage;
            _numberOfConsumers= numberOfConsumers;
            Console.WriteLine("Transfer packet producer starting");
            Console.WriteLine("Connecting to rabbitmq");
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@rabbitmq:5672")
            };
            conn = factory.CreateConnection();
            channel = conn.CreateModel();
            channel.ContinuationTimeout = TimeSpan.FromSeconds(10000);
            channel.ExchangeDeclare(topicExchangeName, ExchangeType.Topic, autoDelete: true);
            if (_isSendingFanoutMessage)
            {
                channel.ExchangeDeclare(fanoutExchangeName, ExchangeType.Fanout, autoDelete: true);
            }
        }

        public void RunTest(byte[] data)
        {
            var lastIndex = (int)Math.Ceiling(Convert.ToDecimal(_size) / data.Length);
            var start = DateTime.Now;
            for (int i = 0; i < lastIndex; i++)
            {
                channel.BasicPublish(exchange: topicExchangeName, routingKey: "transfer-packet.key", body: data);
            }
            var publishEnd = DateTime.Now;
            Console.WriteLine($"Publish Start : {start:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Publish End : {publishEnd:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Publish End Ticks : {publishEnd.Ticks}");
            Console.WriteLine($"Publishing time in miliseconds : {(int)(publishEnd - start).TotalMilliseconds}");
            while(true) {
                for(int i = 1; i <= _numberOfConsumers; i++)
                {
                    if (channel.MessageCount($"transfer-packet-multiple-queue-{i}") > 0)
                    {
                        continue;
                    }
                }
                break;
            }
            var consumeEnd = DateTime.Now;
            if (_isSendingFanoutMessage)
            {
                channel.BasicPublish(fanoutExchangeName, routingKey: "", body: null);
            }
            Console.WriteLine($"Finished consuming at {consumeEnd:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Test time in miliseconds : {(int)(consumeEnd - start).TotalMilliseconds}");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var size in _byteSizes)
            {
                Console.WriteLine();
                Console.WriteLine($"Test for {size} bytes");
                RunTest(BytesGenerator.GetByteArray(size));
                Task.Delay(10000, cancellationToken).Wait(cancellationToken);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            channel.Close();
            conn.Close();
            return Task.CompletedTask;
        }
    }
}

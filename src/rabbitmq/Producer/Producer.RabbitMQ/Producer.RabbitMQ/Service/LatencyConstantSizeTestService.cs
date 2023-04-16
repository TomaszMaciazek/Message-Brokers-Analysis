using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Producer.RabbitMQ.Service
{
    public class LatencyConstantSizeTestService : IHostedService
    {
        private const string topicExchangeName = "latency-exchange";
        private const string fanoutExchangeName = "latency-result-exchange";
        private const string queueName = "latency-queue";

        private readonly int _numberOfMessages;
        private readonly bool _isSendingFanoutMessage;
        private readonly List<int> _byteSizes = new()
        {
            250, 1000, 4000,  16000, 64000, 256000, 1000000
        };

        private readonly IConnection conn;
        private readonly IModel channel;

        public LatencyConstantSizeTestService(int numberOfMessages, bool isSendingFanoutMessage)
        {
            _numberOfMessages = numberOfMessages;
            _isSendingFanoutMessage = isSendingFanoutMessage;
            Console.WriteLine("Latency producer starting");
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
            for (int i = 0; i < _numberOfMessages; i++)
            {
                var prop = channel.CreateBasicProperties();
                prop.Headers.Add("produce-time", DateTime.Now.Ticks);
                channel.BasicPublish(exchange: topicExchangeName, routingKey: "latency.key", body: data);
            }

            var publishEnd = DateTime.Now;
            Console.WriteLine($"Publish End : {publishEnd:yyyy-MM-dd HH:mm:ss.fffffff}");
            while (channel.MessageCount(queueName) > 0) { }
            var consumeEnd = DateTime.Now;
            Console.WriteLine($"Finished consuming at {consumeEnd:yyyy-MM-dd HH:mm:ss.fffffff}");
            if (_isSendingFanoutMessage)
            {
                channel.BasicPublish(exchange: fanoutExchangeName, routingKey: "", body: null);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var size in _byteSizes)
            {
                Console.WriteLine($"Test for {size} bytes");
                RunTest(BytesGenerator.GetByteArray(size));
                Task.Delay(5000, cancellationToken).Wait(cancellationToken);
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

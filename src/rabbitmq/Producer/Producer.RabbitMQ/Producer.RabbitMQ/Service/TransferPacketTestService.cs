using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Producer.RabbitMQ.Common;
using RabbitMQ.Client;
using System.Text;

namespace Producer.RabbitMQ.Service
{
    public class TransferPacketTestService : IHostedService
    {
        private const string topicExchangeName = "transfer-packet-exchange";
        private const string queueName = "transfer-packet-queue";
        private const string fanoutExchangeName = "transfer-packet-result-exchange";

        private readonly long _size;
        private readonly int _messageSize;
        private readonly bool _isSendingFanoutMessage;

        private readonly IConnection conn;
        private readonly IModel channel;

        public TransferPacketTestService(long size, int messageSize, bool isSendingFanoutMessage)
        {
            _size = size;
            _messageSize = messageSize;
            _isSendingFanoutMessage = isSendingFanoutMessage;
            Console.WriteLine("Transfer packet producer starting");
            Console.WriteLine("Connecting to rabbitmq");
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@rabbitmq:5672")
            };
            conn = factory.CreateConnection();
            channel = conn.CreateModel();
            Console.WriteLine("Model created");
            channel.ContinuationTimeout = TimeSpan.FromSeconds(10000);
            channel.ExchangeDeclare(topicExchangeName, ExchangeType.Topic, autoDelete: true);
            if (_isSendingFanoutMessage)
            {
                channel.ExchangeDeclare(fanoutExchangeName, ExchangeType.Fanout, autoDelete: true);
            }
            Console.WriteLine("Connected to rabbitmq");
        }

        public void RunTest(byte[] data)
        {
            var lastIndex = (int)Math.Ceiling(Convert.ToDecimal(_size) / data.Length);
            var start = DateTime.Now;
            for (int i = 0; i < lastIndex; i++)
            {
                channel.BasicPublish(exchange: topicExchangeName, routingKey: "transfer-packet.key", body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SimpleMessage(data))));
            }
            var publishEnd = DateTime.Now;
            Console.WriteLine($"Publish Start : {start:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Publish End : {publishEnd:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Publishing time in miliseconds : {(int)(publishEnd - start).TotalMilliseconds}");
            while(channel.MessageCount(queueName) > 0) { }
            var consumeEnd = DateTime.Now;
            if (_isSendingFanoutMessage)
            {
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new TestResultMessage { ProducerStartTicks = start.Ticks, ProducerFinishedTicks = publishEnd.Ticks }));
                channel.BasicPublish(fanoutExchangeName, routingKey: "", body: body);
            }
            Console.WriteLine($"Finished consuming at {consumeEnd:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Test time in miliseconds : {(int)(consumeEnd - start).TotalMilliseconds}");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Test for {_messageSize} bytes");
            RunTest(BytesGenerator.GetByteArray(_messageSize));
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

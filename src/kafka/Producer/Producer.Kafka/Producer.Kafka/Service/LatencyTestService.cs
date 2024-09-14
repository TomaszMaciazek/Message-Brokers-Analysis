using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Producer.Kafka.Common;

namespace Producer.Kafka.Service
{
    public class LatencyTestService : IHostedService
    {
        private const string topicName = "latency-topic";
        private readonly int _size;
        private readonly int _messageSize;

        private readonly IProducer<Null, string> _producer;
        public LatencyTestService(int size, int messageSize)
        {
            _size = size;
            _messageSize = messageSize;
            var config = new ProducerConfig {
                BootstrapServers = "broker:29092",
                MessageMaxBytes = 10000000,
                QueueBufferingMaxKbytes = 10000000,
                QueueBufferingMaxMessages = 20000000
            };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public void RunTest(byte[] data)
        {
            var lastIndex = (int)Math.Ceiling(Convert.ToDecimal(_size) / data.Length);
            var start = DateTime.UtcNow;
            for (int i = 1; i <= lastIndex; i++)
            {
                _producer.Produce(topicName, new Message<Null, string> { Value = JsonConvert.SerializeObject(new KafkaMessage(data, i == lastIndex, DateTime.UtcNow.Ticks)) ?? "" });
            }
            var end = DateTime.UtcNow;
            Console.WriteLine($"Start : {start:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Start Ticks : {start.Ticks}");
            Console.WriteLine($"End : {end:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Publishing time in miliseconds : {(int)(end - start).TotalMilliseconds}");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Test for {_messageSize} bytes");
            Console.WriteLine();
            RunTest(BytesGenerator.GetByteArray(_messageSize));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _producer.Dispose();
            return Task.CompletedTask;
        }
    }
}

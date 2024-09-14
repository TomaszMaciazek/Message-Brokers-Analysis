using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Producer.Kafka.Common;

namespace Producer.Kafka.Service
{
    public class TransferConstMessagesSizeSumTestMultipleConsumersService : IHostedService
    {
        private const string topicName = "transfer-const-messages-size-sum-topic";
        private readonly long _size;
        private readonly int _messageSize;
        private readonly IProducer<Null, string> _producer;
        private long _startTicks = 0;
        private const int _numberOfConsumers = 2;

        public TransferConstMessagesSizeSumTestMultipleConsumersService(long size, int messageSize)
        {
            _size = size;
            _messageSize = messageSize;
            var config = new ProducerConfig { BootstrapServers = "broker:29092", MessageMaxBytes = 10000000, QueueBufferingMaxKbytes = 10000000, QueueBufferingMaxMessages = 20000000 };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public void RunTest(byte[] data)
        {
            try
            {
                var lastIndex = (int)Math.Ceiling(Convert.ToDecimal(_size) / data.Length);
                var start = DateTime.Now;
                for (int i = 1; i <= lastIndex; i++)
                {
                    _producer.Produce(topicName, new Message<Null, string> { Value = JsonConvert.SerializeObject(new KafkaMessage(data, false, 0)) ?? "" });
                }
                var end = DateTime.Now;
                Console.WriteLine($"Start : {start:yyyy-MM-dd HH:mm:ss.fffffff}");
                Console.WriteLine($"Start Ticks : {start.Ticks}");
                Console.WriteLine($"End : {end:yyyy-MM-dd HH:mm:ss.fffffff}");
                Console.WriteLine($"Publishing time in miliseconds : {(int)(end - start).TotalMilliseconds}");
                _startTicks = start.Ticks;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            var parts = new List<TopicPartition>();
            for (int i = 0; i < _numberOfConsumers; i++)
            {
                parts.Add(new TopicPartition(topicName, new Partition(i)));

            }
            Console.WriteLine($"Test for {_messageSize} bytes");
            RunTest(BytesGenerator.GetByteArray(_messageSize));
            Task.Delay(30000, cancellationToken).Wait(cancellationToken);
            parts.ForEach(topicPart => _producer.Produce(topicPart, new Message<Null, string> { Value = JsonConvert.SerializeObject(new KafkaMessage(Array.Empty<byte>(), true, _startTicks)) }));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _producer.Dispose();
            return Task.CompletedTask;
        }
    }
}

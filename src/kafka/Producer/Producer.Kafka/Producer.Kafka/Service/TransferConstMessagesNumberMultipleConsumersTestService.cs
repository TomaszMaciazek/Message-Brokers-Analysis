using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Producer.Kafka.Common;
using System.Diagnostics;

namespace Producer.Kafka.Service
{
    public class TransferConstMessagesNumberMultipleConsumersTestService : IHostedService
    {
        private const string topicName = "const-messages-number-topic";

        private readonly List<long> _seconds;
        private readonly int _numberOfMessages;
        private readonly int _size;
        private long _startTicks = 0;
        private const int _numberOfConsumers = 2;

        private readonly IProducer<Null, string> _producer;
        public TransferConstMessagesNumberMultipleConsumersTestService(int numberOfMessages, int messageSize)
        {
            _numberOfMessages = numberOfMessages;
            _size = messageSize;
            _seconds= new List<long>();
            var config = new ProducerConfig { BootstrapServers = "broker:29092", MessageMaxBytes = 10000000, QueueBufferingMaxKbytes = 10000000 };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public void RunTest(byte[] data)
        {
            try
            {
                var stopWatch = new Stopwatch();
                var start = DateTime.Now;
                stopWatch.Start();
                for (int i = 1; i <= _numberOfMessages; i++)
                {
                    //wiele partycji powoduje że kolejność wiadomości może zostać zmieniona więc trzeba rozwiązać to inaczej
                    _producer.Produce(topicName, new Message<Null, string> { Value = JsonConvert.SerializeObject(new KafkaMessage(data, false, 0)) ?? ""});
                    _seconds.Add(stopWatch.Elapsed.Ticks / TimeSpan.TicksPerSecond);
                }
                var end = DateTime.Now;
                stopWatch.Stop();
                Console.WriteLine($"Start : {start:yyyy-MM-dd HH:mm:ss.fffffff}");
                Console.WriteLine($"Start Ticks : {start.Ticks}");
                Console.WriteLine($"End : {end:yyyy-MM-dd HH:mm:ss.fffffff}");
                Console.WriteLine($"Publishing time in miliseconds : {(int)(end - start).TotalMilliseconds}");
                var groupSeconds = _seconds.GroupBy(x => x);
                foreach (var group in groupSeconds)
                {
                    Console.WriteLine($"{group.Key}, {group.Count()}");
                }
                Console.WriteLine($"Number of publishes: {groupSeconds.Sum(x => x.Count())}");
                Console.WriteLine($"Avg publishes per second: {groupSeconds.Average(x => x.Count())}");
                _seconds.Clear();
                _startTicks = start.Ticks;
            }
            catch( Exception ex )
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
            Console.WriteLine($"Test for {_size} bytes - multiple consumers");
            Console.WriteLine();
            RunTest(BytesGenerator.GetByteArray(_size));
            Task.Delay(4000, cancellationToken).Wait(cancellationToken);
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

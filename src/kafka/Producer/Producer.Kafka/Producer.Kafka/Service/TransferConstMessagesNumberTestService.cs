using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Producer.Kafka.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Producer.Kafka.Service
{
    public class TransferConstMessagesNumberTestService : IHostedService
    {
        private const string topicName = "const-messages-number-topic";

        private readonly List<long> _seconds;
        private readonly int _numberOfMessages;

        private readonly IProducer<Null, string> _producer;

        public TransferConstMessagesNumberTestService(int numberOfMessages)
        {
            _numberOfMessages = numberOfMessages;
            _seconds= new List<long>();
            var config = new ProducerConfig { BootstrapServers = "broker:29092" };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public void RunTest(byte[] data)
        {
            var stopWatch = new Stopwatch();
            var start = DateTime.Now;
            stopWatch.Start();
            for (int i = 1; i <= _numberOfMessages; i++)
            {
                _producer.Produce(topicName, new Message<Null, string> { Value = JsonConvert.SerializeObject(new KafkaMessage(data, i == _numberOfMessages)) ?? ""});
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
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            int size = 250;
            Console.WriteLine($"Test for {size} bytes");
            Console.WriteLine();
            RunTest(BytesGenerator.GetByteArray(size));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _producer.Dispose();
            return Task.CompletedTask;
        }


    }
}

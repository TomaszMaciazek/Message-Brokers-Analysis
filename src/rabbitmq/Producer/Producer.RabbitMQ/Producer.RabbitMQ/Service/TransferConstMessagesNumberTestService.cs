using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Producer.RabbitMQ.Common;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;

namespace Producer.RabbitMQ.Service
{
    public class TransferConstMessagesNumberTestService : IHostedService
    {
        private const string topicExchangeName = "transfer-single-exchange";
        private const string fanoutExchangeName = "transfer-single-result-exchange";
        private const string queueName = "transfer-single-queue";

        private readonly List<long> _seconds;
        private readonly int _numberOfMessages;
        private readonly bool _isSendingFanoutMessage;
        private readonly List<int> _byteSizes = new()
        {
            250, 1000, 4000, 16000, 64000, 256000, 1000000
        };

        private readonly IConnection conn;
        private readonly IModel channel;

        public TransferConstMessagesNumberTestService(int numberOfMessages, bool isSendingFanoutMessage)
        {
            _numberOfMessages = numberOfMessages;
            _isSendingFanoutMessage= isSendingFanoutMessage;
            _seconds= new List<long>();

            Console.WriteLine("Transfer constant number of messages producer starting");
            Console.WriteLine("Connecting to rabbitmq");
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@rabbitmq:5672")
            };
            conn = factory.CreateConnection();
            channel = conn.CreateModel();
            channel.ContinuationTimeout = TimeSpan.FromSeconds(10000);
            channel.ExchangeDeclare(topicExchangeName, ExchangeType.Topic, true, autoDelete: true);
            if (_isSendingFanoutMessage)
            {
                channel.ExchangeDeclare(fanoutExchangeName, ExchangeType.Fanout, autoDelete: true);
            }
        }

        public void RunTest(byte[] data)
        {
            var stopWatch = new Stopwatch();
            var start = DateTime.Now;
            stopWatch.Start();
            for (int i = 0; i < _numberOfMessages; i++)
            {
                channel.BasicPublish(exchange: topicExchangeName, routingKey: "transfer-single.key", body: data);
                _seconds.Add(stopWatch.Elapsed.Ticks / TimeSpan.TicksPerSecond);
            }
            var end = DateTime.Now;
            stopWatch.Stop();
            Console.WriteLine($"Start : {start:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"End : {end:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Publishing time in miliseconds : {(int)(end - start).TotalMilliseconds}");
            while (channel.MessageCount(queueName) > 0) { }
            var consumeEnd = DateTime.Now;
            if (_isSendingFanoutMessage)
            {
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new TestResultMessage { ProducerStartTicks = start.Ticks, ProducerFinishedTicks = end.Ticks }));
                channel.BasicPublish(fanoutExchangeName, routingKey: "", body: body);
            }
            Console.WriteLine($"Finished consuming at {consumeEnd:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Test time in miliseconds : {(int)(consumeEnd - start).TotalMilliseconds}");
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
            foreach (var size in _byteSizes)
            {
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

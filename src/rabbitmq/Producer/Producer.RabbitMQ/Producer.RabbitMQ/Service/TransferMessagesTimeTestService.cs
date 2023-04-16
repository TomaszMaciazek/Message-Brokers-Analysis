using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System.Diagnostics;

namespace Producer.RabbitMQ.Service
{
    public class TransferMessagesTimeTestService : IHostedService
    {
        private const string topicExchangeName = "transfer-time-exchange";
        private const string fanoutExchangeName = "transfer-time-result";
        private const string queueName = "transfer-time-queue";

        private readonly List<long> _seconds;
        private readonly int _timeInMiliseconds;
        private readonly bool _isSendingFanoutMessage;
        private readonly List<int> _byteSizes = new()
        {
            250, 1000, 4000,  16000, 64000, 256000, 1000000 
        };

        private readonly IConnection conn;
        private readonly IModel channel;

        public TransferMessagesTimeTestService(int timeInMiliseconds, bool isSendingFanoutMessage)
        {
            _seconds = new List<long>();
            _timeInMiliseconds= timeInMiliseconds;
            _isSendingFanoutMessage= isSendingFanoutMessage;
            Console.WriteLine("Constant time transfer producer starting");
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
            var stopWatch = new Stopwatch();
            var start = DateTime.Now;
            stopWatch.Start();
            while (stopWatch.Elapsed.TotalMilliseconds < _timeInMiliseconds)
            {
                channel.BasicPublish(exchange: topicExchangeName, routingKey: "transfer-time.key", body: data);
                _seconds.Add(stopWatch.Elapsed.Ticks/TimeSpan.TicksPerSecond);
            }
            var publishEnd = DateTime.Now;
            stopWatch.Stop();
            Console.WriteLine($"Publish Start : {start:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Publish End : {publishEnd:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Publishing time in miliseconds : {(int)(publishEnd - start).TotalMilliseconds}");
            //wait for all messages in queue to be consumed
            while (channel.MessageCount(queueName) > 0) { }
            var consumeEnd = DateTime.Now;
            if (_isSendingFanoutMessage)
            {
                channel.BasicPublish(fanoutExchangeName, routingKey: "", body: null);
            }
            Console.WriteLine($"Finished consuming at {consumeEnd:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Test time in miliseconds : {(int)(consumeEnd - start).TotalMilliseconds}");
            var groupSeconds = _seconds.GroupBy(x => x).Where(x => x.Key * 1000 < _timeInMiliseconds);
            Console.WriteLine("second, count");
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
            foreach(var size in _byteSizes)
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

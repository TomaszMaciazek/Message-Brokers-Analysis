using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;

namespace Consumer.RabbitMQ.Service
{
    public class TransferMessagesTimeTestService : IHostedService
    {
        private IConnection? conn;
        private IModel? channel;
        private readonly List<int> records = new List<int>();
        private readonly Stopwatch stopwatch = new ();
        private const string topicExchangeName = "transfer-time-exchange";
        private const string fanoutExchangeName = "transfer-time-result";
        private const string queueName = "transfer-time-queue";

        public Task StartAsync(CancellationToken cancellationToken)
        {
            stopwatch.Start();
            Console.WriteLine("Connecting to rabbitmq");
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@rabbitmq:5672")
            };

            conn = factory.CreateConnection();
            channel = conn.CreateModel();
            channel.ContinuationTimeout = TimeSpan.FromSeconds(10000);
            channel.BasicQos(0, 1, false);
            channel.QueueDeclare(queueName, false, false, true, null);
            var fanoutQueue = channel.QueueDeclare().QueueName;
            channel.ExchangeDeclare(topicExchangeName, ExchangeType.Topic, autoDelete: true);
            channel.ExchangeDeclare(fanoutExchangeName, ExchangeType.Fanout, autoDelete: true);
            channel.QueueBind(queueName, topicExchangeName, "transfer-time.key");
            channel.QueueBind(fanoutQueue, fanoutExchangeName, "");
            Console.WriteLine("Connected to rabbitmq");

            var consumer = new EventingBasicConsumer(channel);
            var cancelConsumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                records.Add((int)stopwatch.Elapsed.TotalSeconds);
            };

            cancelConsumer.Received += (model, ea) =>
            {
                //wait for empty queue
                while (channel.MessageCount(queueName) > 0) { }
                Console.WriteLine("Received cancel message");
                var groupedRecords = records.GroupBy(x => x);
                Console.WriteLine($"Number of consumed messages: {records.Count}");
                Console.WriteLine($"Average number of messages per second: {groupedRecords.Select(x => x.Count()).DefaultIfEmpty(0).Average()}");
                records.Clear();
            };
            channel.BasicConsume(queue: fanoutQueue, autoAck: true, consumer: cancelConsumer);
            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            channel?.Close();
            conn?.Close();
            return Task.CompletedTask;
        }
    }
}

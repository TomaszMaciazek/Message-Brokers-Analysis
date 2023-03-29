using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;

namespace Consumer.RabbitMQ.Service
{
    public class SingleMessageTestService : IHostedService
    {
        private IConnection? conn;
        private IModel? channel;
        private readonly List<int> records = new List<int>();
        private readonly Stopwatch stopwatch = new Stopwatch();
        private string consumerTag = string.Empty;
        private const string queueName = "eksp-1-queue-";
        private const string exchangeName = "eksp-1-exchange";
        private readonly string cancelQueue = $"cancel-queue-{Guid.NewGuid()}";
        private const string cancelExchange = "cancel-exchange";

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
            channel.QueueDeclare(cancelQueue, false, false, true, null);
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, autoDelete: true);
            channel.ExchangeDeclare(cancelExchange, ExchangeType.Topic, autoDelete: true);
            channel.QueueBind(queueName, exchangeName, "eksp-1.key");
            channel.QueueBind(cancelQueue, cancelExchange, "cancel.key");
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
            channel.BasicConsume(queue: cancelQueue, autoAck: true, consumer: cancelConsumer);
            consumerTag = channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            stopwatch.Stop();
            channel?.Close();
            conn?.Close();
            return Task.CompletedTask;
        }
    }
}

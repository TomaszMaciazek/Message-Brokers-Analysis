using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Consumer.RabbitMQ.Service
{
    public class BreakdownTestService : IHostedService
    {
        private IConnection? conn;
        private IModel? channel;
        private const string exchangeName = "breakdown-exchange";
        private const string queueName = "breakdown-queue";
        private string consumerTag = string.Empty;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Latency consumer starting");
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
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true, autoDelete: true);
            channel.QueueBind(queueName, exchangeName, "breakdown.key");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine(message);
            };

            consumerTag = channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            channel?.BasicCancel(consumerTag);
            channel?.Close();
            conn?.Close();
            return Task.CompletedTask;
        }
    }
}

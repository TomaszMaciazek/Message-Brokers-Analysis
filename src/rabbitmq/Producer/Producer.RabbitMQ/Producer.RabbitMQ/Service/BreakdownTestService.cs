using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System.Text;

namespace Producer.RabbitMQ.Service
{
    public class BreakdownTestService : IHostedService
    {
        private const string exchangeName = "breakdown-exchange";
        private const string queueName = "breakdown-queue";
        private readonly int _numberOfMessages;

        public BreakdownTestService(int numberOfMessages)
        {
            _numberOfMessages = numberOfMessages;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Breakdown producer starting");
            Console.WriteLine("Connecting to rabbitmq");
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@rabbitmq:5672")
            };

            var conn = factory.CreateConnection();
            var channel = conn.CreateModel();
            channel.ContinuationTimeout = TimeSpan.FromSeconds(10000);
            channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, exchangeName, "", null);
            //deploy messages to the queue before shutting down rabbitmq container
            for (int i = 0; i < _numberOfMessages; i++)
            {
                channel.BasicPublish(exchange: exchangeName, routingKey: "", body: Encoding.Default.GetBytes($"Message number {i}"));
            }
            Console.WriteLine($"Finished publishing messages at {DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff}");
            channel.Close();
            conn.Close();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

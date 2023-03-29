using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.RabbitMQ.Service
{
    public class BreakdownTestService
    {
        private const string exchangeName = "breakdown-exchange";

        public static void Run(int numberOfMessages)
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
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true, autoDelete: true);
            for (int i = 0; i < numberOfMessages; i++)
            {
                channel.BasicPublish(exchange: exchangeName, routingKey: "breakdown.key", body: Encoding.Default.GetBytes($"Message number {i}"));
            }
            Console.WriteLine($"Finished publishing messages at {DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff}");
        }
    }
}

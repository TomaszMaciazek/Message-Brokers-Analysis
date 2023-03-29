using RabbitMQ.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Producer.RabbitMQ.Service
{
    public class LatencyConstantSizeTestService
    {
        private const string exchangeName = "latency-exchange";
        private const string fanoutExchangeName = "latency-result-exchange";

        public static void Run(byte[] data, int numberOfMessages)
        {
            Stopwatch stopWatch = new();
            List<long> ticksList = new();
            Console.WriteLine("Latency producer starting");
            Console.WriteLine("Connecting to rabbitmq");
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@rabbitmq:5672")
            };

            var conn = factory.CreateConnection();
            var channel = conn.CreateModel();
            channel.ContinuationTimeout = TimeSpan.FromSeconds(10000);
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, autoDelete: true);
            channel.ExchangeDeclare(fanoutExchangeName, ExchangeType.Fanout, autoDelete: true);
            for (int i = 0; i < numberOfMessages; i++)
            {
                channel.BasicPublish(exchange: exchangeName, routingKey: "throughput.key", body: data);
                ticksList.Add(stopWatch.ElapsedTicks);
            }
            channel.BasicPublish(exchange: fanoutExchangeName, routingKey: "", body: Encoding.Default.GetBytes($"Publishing finished by producer at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff}"));
            Console.WriteLine($"Finished publishing messages");
        }
    }
}

using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.RabbitMQ.Service
{
    public class TransferMessagesSingleTestService
    {
        private const string exchangeName = "transfer-single-exchange";
        private const string fanoutExchangeName = "transfer-single-result-exchange";
        public static void RunPacketTransfer(byte[] data, int numberOfMessages)
        {
            Console.WriteLine("Transfer messages producer starting");
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
            var start = DateTime.Now;
            for (int i = 0; i < numberOfMessages; i++)
            {
                channel.BasicPublish(exchange: exchangeName, routingKey: "messages.key", body: data);
            }
            var end = DateTime.Now;
            channel.BasicPublish(exchange: fanoutExchangeName, routingKey: "", body: Encoding.Default.GetBytes($"Publishing finished by producer at {end:yyyy-MM-dd HH:mm:ss.fffffff}"));
            Console.WriteLine($"Start : {start:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"End : {end:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Publishing time in miliseconds : {(int)((TimeSpan)(end - start)).TotalMilliseconds}");
        }
    }
}

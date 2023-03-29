using RabbitMQ.Client;
using System.Text;

namespace Producer.RabbitMQ.Service
{
    public class TransferPacketTestService
    {
        private const string exchangeName = "transfer-exchange";
        private const string fanoutExchangeName = "transfer-packet-result-exchange";

        public static void RunPacketTransfer(byte[] data, int size)
        {
            Console.WriteLine("Transfer producer starting");
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

            var lastIndex = (int)Math.Ceiling(Convert.ToDecimal(size) / data.Length);
            var start = DateTime.Now;
            for (int i = 0; i < lastIndex; i++)
            {
                channel.BasicPublish(exchange: exchangeName, routingKey: "transfer.key", body: data);
            }
            var end = DateTime.Now;
            channel.BasicPublish(exchange: fanoutExchangeName, routingKey: "", body: Encoding.Default.GetBytes($"Publishing finished by producer at {end:yyyy-MM-dd HH:mm:ss.fffffff}"));
            Console.WriteLine($"Start : {start:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"End : {end:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Publishing time in miliseconds : {(int)((TimeSpan)(end - start)).TotalMilliseconds}");
        }
    }
}

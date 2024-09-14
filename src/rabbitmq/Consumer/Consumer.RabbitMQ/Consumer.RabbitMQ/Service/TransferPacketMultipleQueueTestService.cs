using Consumer.RabbitMQ.Common;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Consumer.RabbitMQ.Service
{
    public class TransferPacketMultipleQueueTestService : IHostedService
    {
        private IConnection? conn;
        private IModel? channel;
        private string consumerTag = string.Empty;

        private const string topicExchangeName = "transfer-packet-exchange";
        private const string fanoutExchangeName = "transfer-packet-result-exchange";
        private readonly int _consumerNumber;

        public TransferPacketMultipleQueueTestService(int consumerNumber)
        {
            _consumerNumber = consumerNumber;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Transfer consumer starting");
            Console.WriteLine("Connecting to rabbitmq");
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@rabbitmq:5672")
            };

            conn = factory.CreateConnection();
            channel = conn.CreateModel();
            channel.ContinuationTimeout = TimeSpan.FromSeconds(10000);
            channel.BasicQos(0, 1, false);
            channel.QueueDeclare($"transfer-packet-multiple-queue-{_consumerNumber}",false,true,true);
            var fanoutQueue = channel.QueueDeclare().QueueName;
            channel.ExchangeDeclare(topicExchangeName, ExchangeType.Topic, autoDelete: true);
            channel.ExchangeDeclare(fanoutExchangeName, ExchangeType.Fanout, autoDelete: true);
            channel.QueueBind($"transfer-packet-multiple-queue-{_consumerNumber}", topicExchangeName, $"transfer-packet-{_consumerNumber}.key");
            channel.QueueBind(fanoutQueue, fanoutExchangeName, "");

            DateTime? last = null;
            var consumer = new EventingBasicConsumer(channel);
            var cancelConsumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {

                last = DateTime.Now;
            };

            cancelConsumer.Received += (model, ea) =>
            {
                Console.WriteLine("Received cancel message");
                var body = ea.Body.ToArray();
                var message = JsonConvert.DeserializeObject<TestResultMessage>(Encoding.UTF8.GetString(body));
                string dateString = last.HasValue ? last.Value.ToString("yyyy - MM - dd HH: mm: ss.fffffff") : "no messages were sent";
                Console.WriteLine($"Last message: {dateString}");
                if(message != null && last.HasValue)
                {
                    var elapsedSpan = new TimeSpan(last.Value.Ticks - message.ProducerStartTicks);
                    Console.WriteLine($"Average latency: {(int)elapsedSpan.TotalMilliseconds} ms");
                }
                last = null;
            };
            channel.BasicConsume(queue: fanoutQueue, autoAck: true, consumer: cancelConsumer);
            consumerTag = channel.BasicConsume(queue: $"transfer-packet-multiple-queue-{_consumerNumber}", autoAck: true, consumer: consumer);
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

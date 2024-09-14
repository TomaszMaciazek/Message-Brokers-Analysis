using Consumer.RabbitMQ.Common;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Consumer.RabbitMQ.Service
{
    public class TransferPacketTestService : IHostedService
    {
        private IConnection? conn;
        private IModel? channel;
        private string consumerTag = string.Empty;

        private const string topicExchangeName = "transfer-packet-exchange";
        private const string queueName = "transfer-packet-queue";
        private const string fanoutExchangeName = "transfer-packet-result-exchange";
        
        private DateTime? lastMessage = null;

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
            channel.QueueDeclare(queueName, true, false, true, null);
            var fanoutQueue = channel.QueueDeclare().QueueName;
            channel.ExchangeDeclare(topicExchangeName, ExchangeType.Topic, autoDelete: true);
            channel.ExchangeDeclare(fanoutExchangeName, ExchangeType.Fanout, autoDelete: true);
            channel.QueueBind(queueName, topicExchangeName, "transfer-packet.key");
            channel.QueueBind(fanoutQueue, fanoutExchangeName, "");

            var consumer = new EventingBasicConsumer(channel);
            var cancelConsumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                lastMessage = DateTime.Now;
                var message = JsonConvert.DeserializeObject<SimpleMessage>(Encoding.UTF8.GetString(ea.Body.ToArray()));
                message = null;
            };

            cancelConsumer.Received += (model, ea) =>
            {
                Console.WriteLine("Received cancel message");
                var body = ea.Body.ToArray();
                var message = JsonConvert.DeserializeObject<TestResultMessage>(Encoding.UTF8.GetString(body));
                string dateString = lastMessage.HasValue ? lastMessage.Value.ToString("yyyy - MM - dd HH: mm: ss.fffffff") : "no messages were sent";
                Console.WriteLine($"Last message: {dateString}");
                if (message != null && lastMessage.HasValue)
                {
                    var elapsedSpan = new TimeSpan(lastMessage.Value.Ticks - message.ProducerStartTicks);
                    Console.WriteLine($"Consumption time: {(int)elapsedSpan.TotalMilliseconds} ms");
                }
                lastMessage = null;
                Console.WriteLine();
            };
            channel.BasicConsume(queue: fanoutQueue, autoAck: true, consumer: cancelConsumer);
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

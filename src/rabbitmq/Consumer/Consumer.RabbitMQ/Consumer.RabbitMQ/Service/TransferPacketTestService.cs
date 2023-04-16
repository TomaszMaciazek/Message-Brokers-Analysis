using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            int receivedMessagesCounter = 0;
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
            channel.QueueDeclare(queueName, false, false, true, null);
            var fanoutQueue = channel.QueueDeclare().QueueName;
            channel.ExchangeDeclare(topicExchangeName, ExchangeType.Topic, autoDelete: true);
            channel.ExchangeDeclare(fanoutExchangeName, ExchangeType.Fanout, autoDelete: true);
            channel.QueueBind(queueName, topicExchangeName, "transfer-packet.key");
            channel.QueueBind(fanoutQueue, fanoutExchangeName, "");

            var consumer = new EventingBasicConsumer(channel);
            var cancelConsumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                //increment received messages counter
                receivedMessagesCounter++;
            };

            cancelConsumer.Received += (model, ea) =>
            {
                Console.WriteLine("Received cancel message");
                Console.WriteLine($"Number of consumed messages: {receivedMessagesCounter}");
                receivedMessagesCounter = 0;
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

using Consumer.RabbitMQ.Common;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

namespace Consumer.RabbitMQ.Service
{
    public class ConstMessagesNumberTestService : IHostedService
    {
        private IConnection? conn;
        private IModel? channel;
        private readonly Stopwatch stopwatch = new();
        private const string exchangeName = "transfer-single-exchange";
        private const string fanoutExchangeName = "transfer-single-result-exchange";
        private const string queueName = "transfer-single-queue";

        private DateTime? lastMessage = null;

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
            channel.QueueDeclare(queueName, true, false, true, null);
            var fanoutQueue = channel.QueueDeclare().QueueName;
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, true, true);
            channel.ExchangeDeclare(fanoutExchangeName, ExchangeType.Fanout, autoDelete: true);
            channel.QueueBind(queueName, exchangeName, "transfer-single.key");
            channel.QueueBind(fanoutQueue, fanoutExchangeName, "");
            Console.WriteLine("Connected to rabbitmq");
            var consumer = new EventingBasicConsumer(channel);
            var cancelConsumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                lastMessage = DateTime.Now;
                var message = JsonConvert.DeserializeObject<SimpleMessage>(Encoding.UTF8.GetString(ea.Body.ToArray()));
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
            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
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

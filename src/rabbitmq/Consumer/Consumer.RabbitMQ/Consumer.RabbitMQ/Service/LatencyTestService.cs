using Consumer.RabbitMQ.Common;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Consumer.RabbitMQ.Service
{
    public class LatencyTestService : IHostedService
    {
        private IConnection? conn;
        private IModel? channel;
        private string consumerTag = string.Empty;
        private string fanoutConsumerTag = string.Empty;
        private const string topicExchangeName = "latency-exchange";
        private const string fanoutExchangeName = "latency-result-exchange";
        private const string queueName = "latency-queue";

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var ticksList = new List<long>();
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

            //create queue for fanout exchange
            var fanoutQueue = channel.QueueDeclare().QueueName;

            channel.ExchangeDeclare(topicExchangeName, ExchangeType.Topic, autoDelete: true);
            channel.ExchangeDeclare(fanoutExchangeName, ExchangeType.Fanout, autoDelete: true);
            channel.QueueBind(queueName, topicExchangeName, "latency.key");
            channel.QueueBind(fanoutQueue, fanoutExchangeName, "");

            var consumer = new EventingBasicConsumer(channel);
            var fanoutConsumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var message = JsonConvert.DeserializeObject<LatencyTestMessage>(Encoding.UTF8.GetString(ea.Body.ToArray()));
                var ticks = DateTime.Now.Ticks - message.ProduceTicks;
                ticksList.Add(ticks);
            };
            fanoutConsumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine(message ?? "");
                if (ticksList.Any())
                {
                    var elapsedSpan = new TimeSpan((long)ticksList.Average());
                    var min = new TimeSpan(ticksList.Min());
                    var max = new TimeSpan(ticksList.Max());
                    Console.WriteLine($"Average latency: {(int)elapsedSpan.TotalMilliseconds} ms");
                    Console.WriteLine($"Minimal latency: {(int)min.TotalMilliseconds} ms");
                    Console.WriteLine($"Maximal latency: {(int)max.TotalMilliseconds} ms");
                    Console.WriteLine();
                    ticksList.Clear();
                }
                ticksList.Clear();
            };
            consumerTag = channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            fanoutConsumerTag = channel.BasicConsume(queue: fanoutQueue, autoAck: true, consumer: fanoutConsumer);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            channel?.BasicCancel(consumerTag);
            channel?.BasicCancel(fanoutConsumerTag);
            channel?.Close();
            conn?.Close();
            return Task.CompletedTask;
        }
    }
}

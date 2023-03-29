using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer.RabbitMQ.Service
{
    public class LatencyTestService : IHostedService
    {
        private IConnection? conn;
        private IModel? channel;
        private string consumerTag = string.Empty;
        private string fanoutConsumerTag = string.Empty;
        private const string queueName = "latency-queue";
        private const string exchangeName = "latency-exchange";
        private const string fanoutExchangeName = "latency-result-exchange";
        private readonly int numberOfProducers = 1;
        private readonly string fanoutQueue = string.Empty;

        public LatencyTestService(int numberOfProducers)
        {
            this.numberOfProducers = numberOfProducers;
            fanoutQueue = Guid.NewGuid().ToString();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var ticksList = new List<long>();
            Console.WriteLine("Latency consumer starting");
            Console.WriteLine("Connecting to rabbitmq");
            int remainingProducers = numberOfProducers;
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@rabbitmq:5672")
            };

            conn = factory.CreateConnection();
            channel = conn.CreateModel();
            channel.ContinuationTimeout = TimeSpan.FromSeconds(10000);
            channel.BasicQos(0, 1, false);
            channel.QueueDeclare(queueName, false, false, true, null);
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, autoDelete: true);
            channel.ExchangeDeclare(fanoutExchangeName, ExchangeType.Fanout, autoDelete: true);
            channel.QueueBind(queueName, exchangeName, "latency.key");
            //create fanout exchange queue with random name
            channel.QueueBind(fanoutQueue, fanoutExchangeName, "");


            var consumer = new EventingBasicConsumer(channel);
            var fanoutConsumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var ticks = DateTime.Now.Ticks - (long)ea.BasicProperties.Headers["produce-time"];
                ticksList.Add(ticks);
            };
            fanoutConsumer.Received += (model, ea) =>
            {
                remainingProducers--;
                //if all producers finished their job
                if (remainingProducers <= 0)
                {
                    //wait for empty queue
                    while(channel.MessageCount(queueName) > 0) { }
                    if (ticksList.Any())
                    {
                        var elapsedSpan = new TimeSpan((long)ticksList.Average());
                        Console.WriteLine($"Average latency: {(int)elapsedSpan.TotalMilliseconds} ms");
                    }
                    ticksList.Clear();
                    remainingProducers = numberOfProducers;
                }
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

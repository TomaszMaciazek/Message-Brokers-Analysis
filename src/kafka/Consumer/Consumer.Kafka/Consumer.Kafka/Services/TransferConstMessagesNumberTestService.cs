using Confluent.Kafka;
using Consumer.Kafka.Common;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Consumer.Kafka.Services
{
    public class TransferConstMessagesNumberTestService : IHostedService
    {
        private const string _groupId = "const-messages-number-consumers";
        private const string _bootstrapServers = "broker:29092";
        private const string _topic = "const-messages-number-topic";
        private readonly IConsumer<Null, string> consumer;

        public TransferConstMessagesNumberTestService()
        {
            var config = new ConsumerConfig
            {
                GroupId = _groupId,
                BootstrapServers = _bootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            consumer = new ConsumerBuilder<Null, string>(config).Build();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Consumption started");
            DateTime? lastMessage;
            consumer.Subscribe(_topic);
            try
            {
                while (true)
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    if (consumeResult.Message != null)
                    {
                        var message = JsonConvert.DeserializeObject<KafkaMessage>(consumeResult.Message.Value);
                        if (message != null) {
                            lastMessage = DateTime.Now;
                            if (message.IsLastMessage) {
                                Console.WriteLine($"Consumption of {message.Data.Length} bytes messages finished at {lastMessage:yyyy-MM-dd HH:mm:ss.fffffff}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            consumer.Close();
            consumer.Dispose();
            return Task.CompletedTask;
        }
    }
}

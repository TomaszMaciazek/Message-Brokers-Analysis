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
                AutoOffsetReset = AutoOffsetReset.Earliest,
                MessageMaxBytes = 20971520
            };
            consumer = new ConsumerBuilder<Null, string>(config).Build();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Consumption started");
            DateTime? lastConsumption = null;
            consumer.Subscribe(_topic);
            try
            {
                while (true)
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    if (consumeResult.Message != null)
                    {
                        var received = DateTime.Now;
                        var message = JsonConvert.DeserializeObject<KafkaMessage>(consumeResult.Message.Value);
                        //Jeśli partycja nie brała udziału w konsumpcji to nie wyświetlamy
                        if (message != null) {

                            if (message.IsLastMessage && lastConsumption.HasValue) {
                                Console.WriteLine($"Consumption of {message.Data.Length} bytes messages finished at {lastConsumption:yyyy-MM-dd HH:mm:ss.fffffff}");
                                var elapsedSpan = new TimeSpan(lastConsumption.Value.Ticks - message.ProduceTicks);
                                Console.WriteLine($"Consumption time: {(int)elapsedSpan.TotalMilliseconds} ms");
                                lastConsumption = null;
                            }
                            else
                            {
                                lastConsumption = received;
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

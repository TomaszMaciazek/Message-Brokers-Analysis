using Confluent.Kafka;
using Consumer.Kafka.Common;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Consumer.Kafka.Services
{
    public class LatencySingleProducerTestService : IHostedService
    {
        private const string _groupId = "latency-consumers";
        private const string _bootstrapServers = "broker:29092";
        private const string _topic = "latency-topic";
        private readonly IConsumer<Null, string> consumer;
        private readonly IList<long> latencies = new List<long>();

        public LatencySingleProducerTestService()
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
            consumer.Subscribe(_topic);
            try
            {
                while (true)
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    if (consumeResult.Message != null)
                    {
                        var received = DateTime.UtcNow;
                        var message = JsonConvert.DeserializeObject<KafkaMessage>(consumeResult.Message.Value);
                        if (message != null)
                        {
                            if (message.IsLastMessage)
                            {
                                if(message.Data.Length > 0)
                                {
                                    Console.WriteLine("Last message");
                                    var ticks = received.Ticks - message.ProduceTicks;
                                    latencies.Add(ticks);
                                }
                                Console.WriteLine($"{message.Data.Length} bytes");
                                if (latencies.Any())
                                {
                                    var minLat = latencies.Min();
                                    var maxLat = latencies.Max();
                                    var avGLat = latencies.Average();
                                    var elapsedSpan = new TimeSpan((long)latencies.Average());
                                    var min = new TimeSpan(latencies.Min());
                                    var max = new TimeSpan(latencies.Max());
                                    Console.WriteLine($"Average latency: {(int)elapsedSpan.TotalMilliseconds} ms");
                                    Console.WriteLine($"Average latency ticks: {avGLat}");
                                    Console.WriteLine($"Minimal latency: {(int)min.TotalMilliseconds} ms");
                                    Console.WriteLine($"Minimal ticks {minLat}");
                                    Console.WriteLine($"Maximal latency: {(int)max.TotalMilliseconds} ms");
                                    Console.WriteLine($"Maximal ticks {maxLat}");
                                    Console.WriteLine();
                                    latencies.Clear();
                                }
                            }
                            else
                            {
                                var ticks = received.Ticks - message.ProduceTicks;
                                latencies.Add(ticks);
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
            Console.WriteLine("Closing connection");
            consumer.Close();
            consumer.Dispose();
            return Task.CompletedTask;
        }
    }
}

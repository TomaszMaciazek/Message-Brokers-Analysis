using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Consumer.ActiveMQ.Common;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Consumer.ActiveMQ.Services
{
    public class LatencyTestService : IHostedService
    {
        private readonly IConnection connection;
        private readonly ISession session;
        private readonly IMessageConsumer queueConsumer;
        private readonly IMessageConsumer topicConsumer;

        private const string queueName = "latency-queue";
        private const string topicName = "latency-topic";
        private readonly int _numberOfProducers;
        private readonly IList<long> latencies = new List<long>();

        private int counter = 0;

        public LatencyTestService(int numberOfProducers)
        {
            _numberOfProducers = numberOfProducers;
            latencies = new List<long>();
            var connecturi = new Uri("activemq:tcp://activemq:61616");
            var connectionFactory = new ConnectionFactory(connecturi);
            connectionFactory.OptimizeAcknowledge = true;

            connection = connectionFactory.CreateConnection();
            connection.Start();

            session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

            var queue = session.GetQueue(queueName);
            queueConsumer = session.CreateConsumer(queue);
            queueConsumer.Listener += QueueConsumer_Listener;
            var topic = session.GetTopic(topicName);
            topicConsumer = session.CreateConsumer(topic);
            topicConsumer.Listener += TopicConsumer_Listener;
        }

        private void QueueConsumer_Listener(IMessage message)
        {
            try
            {
                var received = DateTime.Now;
                if (message is ITextMessage txtMessage)
                {
                    var receivedMessage = JsonConvert.DeserializeObject<SimpleMessage>(txtMessage.Text);
                    if (receivedMessage != null)
                    {

                        var ticks = received.Ticks - receivedMessage.ProduceTicks;
                        latencies.Add(ticks);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void TopicConsumer_Listener(IMessage message)
        {
            try
            {
                if (message is ITextMessage txtMessage)
                {
                    counter++;
                    //if all producers finished their work
                    if (counter == _numberOfProducers && latencies.Any())
                    {
                        counter = 0;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            queueConsumer.Close();
            topicConsumer.Close();
            queueConsumer.Close();
            session.Close();
            connection.Close();
            return Task.CompletedTask;
        }
    }
}

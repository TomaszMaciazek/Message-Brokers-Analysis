using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Consumer.ActiveMQ.Common;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Consumer.ActiveMQ.Services
{
    public class ConstMessagesSizeSumTestService : IHostedService
    {
        private readonly IConnection connection;
        private readonly ISession session;
        private readonly IMessageConsumer consumer;

        private const string queueName = "transfer-const-sum-queue";

        public ConstMessagesSizeSumTestService()
        {
            var connecturi = new Uri("activemq:tcp://activemq:61616");
            var connectionFactory = new ConnectionFactory(connecturi);
            connectionFactory.OptimizeAcknowledge = true;

            connection = connectionFactory.CreateConnection();
            connection.Start();

            session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

            var queue = session.GetQueue(queueName);
            consumer = session.CreateConsumer(queue);
            consumer.Listener += Consumer_Listener;
        }

        private void Consumer_Listener(IMessage message)
        {
            try
            {
                var received = DateTime.Now;
                if (message is ITextMessage txtMessage)
                {
                    var receivedMessage = JsonConvert.DeserializeObject<SimpleMessage>(txtMessage.Text);
                    if (receivedMessage != null)
                    {

                        if (receivedMessage.IsLastMessage)
                        {
                            Console.WriteLine($"Consumption finished at {received:yyyy-MM-dd HH:mm:ss.fffffff}");
                            var start = new DateTime(receivedMessage.ProduceTicks);
                            Console.WriteLine($"Production started at {start:yyyy-MM-dd HH:mm:ss.fffffff}");
                            var elapsedSpan = new TimeSpan(received.Ticks - receivedMessage.ProduceTicks);
                            Console.WriteLine($"Consumption time: {(int)elapsedSpan.TotalMilliseconds} ms");
                        }
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
            consumer.Close();
            session.Close();
            connection.Close();
            return Task.CompletedTask;
        }
    }
}

using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Producer.ActiveMQ.Common;

namespace Producer.ActiveMQ.Service
{
    public class ConstMessagesSizeSumTestService : IHostedService
    {
        private readonly IConnection connection;
        private readonly ISession session;
        private readonly IMessageProducer producer;

        private const string queueName = "transfer-const-sum-queue";

        private readonly int _size;
        private readonly int _messageSize;

        public ConstMessagesSizeSumTestService(int size, int messageSize)
        {
            _size = size;
            _messageSize = messageSize;

            var connecturi = new Uri("activemq:tcp://activemq:61616");
            var connectionFactory = new ConnectionFactory(connecturi);
            connection = connectionFactory.CreateConnection();
            connection.Start();

            session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

            var queue = session.GetQueue(queueName);
            producer = session.CreateProducer(queue);
            producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
        }

        public void RunTest(byte[] data)
        {
            try
            {
                var lastIndex = (int)Math.Ceiling(Convert.ToDecimal(_size) / data.Length);
                var start = DateTime.Now;
                for (int i = 1; i <= lastIndex; i++)
                {
                    var message = session.CreateTextMessage(JsonConvert.SerializeObject(new SimpleMessage(data, i == lastIndex, start.Ticks)) ?? "");
                    producer.Send(message);

                }
                var end = DateTime.Now;
                Console.WriteLine($"Start : {start:yyyy-MM-dd HH:mm:ss.fffffff}");
                Console.WriteLine($"Start Ticks : {start.Ticks}");
                Console.WriteLine($"End : {end:yyyy-MM-dd HH:mm:ss.fffffff}");
                Console.WriteLine($"End Ticks : {end.Ticks}");
                Console.WriteLine($"Publishing time in miliseconds : {(int)(end - start).TotalMilliseconds}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

        Console.WriteLine($"Test for {_messageSize} bytes");
        Console.WriteLine();
        RunTest(BytesGenerator.GetByteArray(_messageSize));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            producer.Close();
            session.Close();
            connection.Close();
            return Task.CompletedTask;
        }
    }
}

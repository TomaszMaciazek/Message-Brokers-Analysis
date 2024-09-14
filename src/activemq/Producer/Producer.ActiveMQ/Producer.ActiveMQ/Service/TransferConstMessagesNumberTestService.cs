using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Producer.ActiveMQ.Common;
using System.Diagnostics;

namespace Producer.ActiveMQ.Service
{
    public class TransferConstMessagesNumberTestService : IHostedService
    {
        private readonly IConnection connection;
        private readonly ISession session;
        private readonly IMessageProducer producer;

        private const string queueName = "transfer-const-number-queue";

        private readonly List<long> _seconds;
        private readonly int _numberOfMessages;
        private readonly int _messageSize;

        public TransferConstMessagesNumberTestService(int numberOfMessages, int messageSize)
        {
            _numberOfMessages = numberOfMessages;
            _messageSize = messageSize;
            _seconds= new List<long>();

            var connecturi = new Uri("activemq:tcp://activemq:61616");
            var connectionFactory = new ConnectionFactory(connecturi);
            connection = connectionFactory.CreateConnection();
            connection.Start();

            session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

            var queue = session.GetQueue(queueName);
            producer = session.CreateProducer(queue);
            //Because of heap overloading with bigger messages
            producer.DeliveryMode =/* _messageSize >= 500000 ? MsgDeliveryMode.Persistent :*/ MsgDeliveryMode.NonPersistent;
        }

        public void RunTest(byte[] data)
        {
            try
            {
                var stopWatch = new Stopwatch();
                var start = DateTime.Now;
                stopWatch.Start();
                for (int i = 1; i <= _numberOfMessages; i++)
                {
                    var message = session.CreateTextMessage(JsonConvert.SerializeObject(new SimpleMessage(data, i == _numberOfMessages, start.Ticks)) ?? "");
                    producer.Send(message);
                    _seconds.Add(stopWatch.Elapsed.Ticks / TimeSpan.TicksPerSecond);
                }
                var end = DateTime.Now;
                stopWatch.Stop();
                Console.WriteLine($"Start : {start:yyyy-MM-dd HH:mm:ss.fffffff}");
                Console.WriteLine($"Start Ticks : {start.Ticks}");
                Console.WriteLine($"End : {end:yyyy-MM-dd HH:mm:ss.fffffff}");
                Console.WriteLine($"End Ticks : {end.Ticks}");
                Console.WriteLine($"Publishing time in miliseconds : {(int)(end - start).TotalMilliseconds}");
                var groupSeconds = _seconds.GroupBy(x => x);
                foreach (var group in groupSeconds)
                {
                    Console.WriteLine($"{group.Key}, {group.Count()}");
                }
                Console.WriteLine($"Number of publishes: {groupSeconds.Sum(x => x.Count())}");
                Console.WriteLine($"Avg publishes per second: {groupSeconds.Average(x => x.Count())}");
                _seconds.Clear();
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

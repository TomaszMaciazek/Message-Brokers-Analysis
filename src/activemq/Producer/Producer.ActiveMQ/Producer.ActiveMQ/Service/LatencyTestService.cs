using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Producer.ActiveMQ.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Producer.ActiveMQ.Service
{
    public class LatencyTestService : IHostedService
    {
        private readonly IConnection connection;
        private readonly ISession session;
        private readonly IMessageProducer queueProducer;
        private readonly IMessageProducer topicProducer;

        private const string queueName = "latency-queue";
        private const string topicName = "latency-topic";

        private readonly int _size;
        private readonly int _messageSize;

        public LatencyTestService(int size, int messageSize)
        {
            _size = size;
            _messageSize = messageSize;
            var connecturi = new Uri("activemq:tcp://activemq:61616");
            var connectionFactory = new ConnectionFactory(connecturi);
            connection = connectionFactory.CreateConnection();
            connection.Start();

            session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

            var queue = session.GetQueue(queueName);
            queueProducer = session.CreateProducer(queue);
            queueProducer.DeliveryMode = MsgDeliveryMode.NonPersistent;

            var topic = session.GetTopic(topicName);
            topicProducer = session.CreateProducer(topic);
            topicProducer.DeliveryMode|= MsgDeliveryMode.NonPersistent;
        }

        public void RunTest(byte[] data)
        {
            var lastIndex = (int)Math.Ceiling(Convert.ToDecimal(_size) / data.Length);
            var start = DateTime.UtcNow;
            for (int i = 1; i <= lastIndex; i++)
            {
                var message = session.CreateTextMessage(JsonConvert.SerializeObject(new SimpleMessage(data, false, DateTime.Now.Ticks)) ?? "");
                queueProducer.Send(message);
            }
            var end = DateTime.UtcNow;
            Console.WriteLine($"Start : {start:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Start Ticks : {start.Ticks}");
            Console.WriteLine($"End : {end:yyyy-MM-dd HH:mm:ss.fffffff}");
            Console.WriteLine($"Publishing time in miliseconds : {(int)(end - start).TotalMilliseconds}");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Test for {_messageSize} bytes");
            Console.WriteLine();
            RunTest(BytesGenerator.GetByteArray(_messageSize));
            Task.Delay(30000, cancellationToken).Wait(cancellationToken);
            var message = session.CreateTextMessage("End");
            topicProducer.Send(message);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

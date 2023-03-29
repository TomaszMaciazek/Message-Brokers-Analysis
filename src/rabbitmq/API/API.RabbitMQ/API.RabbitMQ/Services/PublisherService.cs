using Producer.RabbitMQ.API.Model;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;

namespace Producer.RabbitMQ.API.Services
{
    public interface IPublisherService
    {
        Task<ThroughputResult> SendMessagesForExperimentOne(int seconds, byte[] body);
        Task<TransferResult> TransferTimeExperiment(byte[] body);
    }
    public class PublisherService : IPublisherService
    {
        private readonly IConnection conn;
        private readonly IModel channel;

        private readonly string exchangeForExperimentOne = "eksp-1-exchange";
        private readonly string exchangeForTransfer = "transfer-exchange";
        private readonly string exchangeCancel = "cancel-exchange";

        public PublisherService()
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@rabbitmq:5672")
            };

            conn = factory.CreateConnection();
            channel = conn.CreateModel();
            channel.ContinuationTimeout = TimeSpan.FromSeconds(10000);
            channel.ExchangeDeclare(exchangeForExperimentOne, ExchangeType.Topic, autoDelete: true);
            channel.ExchangeDeclare(exchangeCancel, ExchangeType.Topic, autoDelete: true);
        }

        public async Task<ThroughputResult> SendMessagesForExperimentOne(int seconds, byte[] body)
        {
            var records = new List<int>();
            Stopwatch stopWatch = new();
            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(seconds));
            await Task.Run(() =>
            {
                stopWatch.Start();
                while (!source.Token.IsCancellationRequested)
                {
                    channel.BasicPublish(exchange: exchangeForExperimentOne, routingKey: "eksp-1.key", body: body);
                    records.Add((int)stopWatch.Elapsed.TotalSeconds);
                }
            }, source.Token);
            var time = stopWatch.Elapsed.TotalSeconds;
            channel.BasicPublish(exchange: exchangeCancel, routingKey: "cancel.key", body: null);
            stopWatch.Stop();
            var avg = (int)records.GroupBy(x => x).Select(x => x.Count()).DefaultIfEmpty(0).Average();
            var count = records.Count;
            records.Clear();
            return new ThroughputResult
            {
                SentMessages = count,
                Seconds = time,
                AvgNumberOfMessages = avg
            };
        }
    
        public async Task<TransferResult> TransferTimeExperiment(byte[] body)
        {
            //how many publishes we need for 1 GB
            var lastIndex = (int)Math.Ceiling(1000000000m / body.Length);
            var start = DateTime.Now;
            await Task.Run(() =>
            {
                for (int i = 0; i < lastIndex; i++)
                {
                    channel.BasicPublish(exchange: exchangeForTransfer, routingKey: "transfer.key", body: body);
                }
            });
            var end = DateTime.Now;
            return new TransferResult { Start = start, StartTicks = start.Ticks, End = end, EndTicks = end.Ticks };
        }
    }
}

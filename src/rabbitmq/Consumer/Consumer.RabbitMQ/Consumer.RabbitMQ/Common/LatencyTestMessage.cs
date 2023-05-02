namespace Consumer.RabbitMQ.Common
{
    public class LatencyTestMessage
    {
        public long ProduceTicks { get; set; }
        public byte[]? Data { get; set; }
    }
}

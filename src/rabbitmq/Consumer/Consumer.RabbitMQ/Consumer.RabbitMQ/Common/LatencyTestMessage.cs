namespace Consumer.RabbitMQ.Common
{
    public class LatencyTestMessage : SimpleMessage
    {
        public long ProduceTicks { get; set; }
    }
}

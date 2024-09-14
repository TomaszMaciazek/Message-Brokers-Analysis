namespace Producer.RabbitMQ.Common
{
    public class LatencyTestMessage : SimpleMessage
    {
        public long ProduceTicks { get; set; }

        public LatencyTestMessage(long produceTicks, byte[]? data): base(data)
        {
            ProduceTicks = produceTicks;
        }
    }
}

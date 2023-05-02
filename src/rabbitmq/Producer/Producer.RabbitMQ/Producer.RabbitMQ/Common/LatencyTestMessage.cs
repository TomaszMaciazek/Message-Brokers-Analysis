namespace Producer.RabbitMQ.Common
{
    public class LatencyTestMessage
    {
        public long ProduceTicks { get; set; }
        public byte[]? Data { get; set; }

        public LatencyTestMessage(long produceTicks, byte[]? data)
        {
            ProduceTicks = produceTicks;
            Data = data;
        }
    }
}

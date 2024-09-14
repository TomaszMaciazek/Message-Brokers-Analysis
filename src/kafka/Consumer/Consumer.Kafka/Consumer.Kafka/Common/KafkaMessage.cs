namespace Consumer.Kafka.Common
{
    public class KafkaMessage
    {
        public byte[] Data { get; set; }
        public bool IsLastMessage { get; set; }
        public long ProduceTicks { get; set; }
    }
}

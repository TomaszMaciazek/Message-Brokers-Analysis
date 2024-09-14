namespace Producer.Kafka.Common
{
    public class KafkaMessage
    {
        public byte[] Data { get; set; }
        public bool IsLastMessage { get; set; }
        public long ProduceTicks { get; set; }

        public KafkaMessage(byte[] data, bool isLastMessage, long produceTicks)
        {
            Data = data;
            IsLastMessage = isLastMessage;
            ProduceTicks = produceTicks;
        }
    }
}

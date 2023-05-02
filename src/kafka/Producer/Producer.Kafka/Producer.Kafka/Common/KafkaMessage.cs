namespace Producer.Kafka.Common
{
    public class KafkaMessage
    {
        public byte[] Data { get; set; }
        public bool IsLastMessage { get; set; }
        public KafkaMessage(byte[] data, bool isLastMessage)
        {
            Data = data;
            IsLastMessage = isLastMessage;
        }
    }
}

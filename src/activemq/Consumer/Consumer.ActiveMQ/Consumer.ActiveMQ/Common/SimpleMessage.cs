namespace Consumer.ActiveMQ.Common
{
    public class SimpleMessage
    {
        public byte[] Data { get; set; }
        public bool IsLastMessage { get; set; }
        public long ProduceTicks { get; set; }
    }
}

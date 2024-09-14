namespace Producer.RabbitMQ.Common
{
    public class SimpleMessage
    {
        public byte[]? Data { get; set; }

        public SimpleMessage(byte[]? data)
        {
            Data = data;
        }
    }
}

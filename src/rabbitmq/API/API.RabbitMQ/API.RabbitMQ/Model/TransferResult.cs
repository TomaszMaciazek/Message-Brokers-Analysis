namespace Producer.RabbitMQ.API.Model
{
    public class TransferResult
    {
        public DateTime Start { get; set; }
        public long StartTicks { get; set; }
        public DateTime End { get; set; }
        public long EndTicks { get; set; }
        public int TimeElapsed { get => (int)((TimeSpan)(End - Start)).TotalMilliseconds; }
    }
}

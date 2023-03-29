namespace Producer.RabbitMQ.API.Model
{
    public class ThroughputResult
    {
        public double Seconds { get; set; }
        public int SentMessages { get; set; }
        public int AvgNumberOfMessages { get; set; }
    }
}

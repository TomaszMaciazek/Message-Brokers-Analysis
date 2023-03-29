namespace Producer.RabbitMQ
{
    public static class BytesGenerator
    {
        public static byte[] GetByteArray(int size)
        {
            var rnd = new Random();
            byte[] b = new byte[size];
            rnd.NextBytes(b);
            return b;
        }
    }
}

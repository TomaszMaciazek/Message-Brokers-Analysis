namespace Producer.RabbitMQ.API.Helpers
{
    public static class BytesHelper
    {
        public static byte[] GetByteArray(int size)
        {
            Random rnd = new Random();
            byte[] b = new byte[size];
            rnd.NextBytes(b);
            return b;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.ActiveMQ
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

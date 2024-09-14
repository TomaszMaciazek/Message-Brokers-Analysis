using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.ActiveMQ.Common
{
    public class SimpleMessage
    {
        public byte[] Data { get; set; }
        public bool IsLastMessage { get; set; }
        public long ProduceTicks { get; set; }

        public SimpleMessage(byte[] data, bool isLastMessage, long produceTicks)
        {
            Data = data;
            IsLastMessage = isLastMessage;
            ProduceTicks = produceTicks;
        }
    }
}

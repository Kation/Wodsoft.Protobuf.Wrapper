using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Extensions.Test
{
    public class MessageModel
    {
        public string Title { get; set; }

        public int Number { get; set; }

        public ProtobufModel Model { get; set; }
    }
}

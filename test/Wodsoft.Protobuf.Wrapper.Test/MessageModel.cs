using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class MessageModel
    {
        public string Title { get; set; }

        public int Number { get; set; }

        public ProtobufModel Model { get; set; }

        public ByteString ByteString { get; set; }

        public List<ProtobufModel> Models { get; set; }
    }
}

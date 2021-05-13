using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class SimplyModel
    {
        public int IntValue { get; set; }

        public double DoubleValue { get; set; }

        public string StringValue { get; set; }

        public long LongValue { get; set; }

        public float FloatValue { get; set; }

        public bool BooleanValue { get; set; }

        public uint UInt32Value { get; set; }

        public ulong UInt64Value { get; set; }

        public byte ByteValue { get; set; }

        public sbyte SByteValue { get; set; }

        public short ShortValue { get; set; }

        public ushort UShortValue { get; set; }

        public DateTime DateTime { get; set; }

        public DateTimeOffset DateTimeOffset { get; set; }

        public TimeSpan TimeSpan { get; set; }

        public byte[] BytesValue { get; set; }
    }
}

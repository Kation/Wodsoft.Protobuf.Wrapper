using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Wodsoft.Protobuf.Extensions.Test
{
    public class SerializationTest
    {
        [Fact]
        public void Simply_Class_Test()
        {
            SimplyModel model = new SimplyModel
            {
                IntValue = 100,
                DoubleValue = 222.222,
                StringValue = "This is a simply object",
                BooleanValue = true,
                ByteValue = 200,
                FloatValue = 1.2f,
                LongValue = 34234,
                SByteValue = -97,
                UInt32Value = 203947109,
                UInt64Value = 21389172947123912,
                ShortValue = -16234,
                UShortValue = 36312
            };

            MemoryStream stream = new MemoryStream();
            Message.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message.Deserialize<SimplyModel>(stream);

            Assert.Equal(model2.IntValue, model.IntValue);
            Assert.Equal(model2.DoubleValue, model.DoubleValue);
            Assert.Equal(model2.StringValue, model.StringValue);
            Assert.Equal(model2.BooleanValue, model.BooleanValue);
            Assert.Equal(model2.ByteValue, model.ByteValue);
            Assert.Equal(model2.FloatValue, model.FloatValue);
            Assert.Equal(model2.LongValue, model.LongValue);
            Assert.Equal(model2.SByteValue, model.SByteValue);
            Assert.Equal(model2.UInt32Value, model.UInt32Value);
            Assert.Equal(model2.UInt64Value, model.UInt64Value);
            Assert.Equal(model2.ShortValue, model.ShortValue);
            Assert.Equal(model2.UShortValue, model.UShortValue);
        }

        [Fact]
        public void Enum_Class_Test()
        {
            EnumModel model = new EnumModel
            {
                ByteEnum = ByteEnum.Two,
                SByteEnum = SByteEnum.Disabled,
                Int16Enum = Int16Enum.One,
                Int32Enum = Int32Enum.Disabled,
                Int64Enum = Int64Enum.Disabled,
                UInt16Enum = UInt16Enum.Normal,
                UInt32Enum = UInt32Enum.Three,
                UInt64Enum = UInt64Enum.Three
            };

            MemoryStream stream = new MemoryStream();
            Message<EnumModel>.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message<EnumModel>.Deserialize(stream);

            Assert.Equal(model2.ByteEnum, model.ByteEnum);
            Assert.Equal(model2.SByteEnum, model.SByteEnum);
            Assert.Equal(model2.Int16Enum, model.Int16Enum);
            Assert.Equal(model2.Int32Enum, model.Int32Enum);
            Assert.Equal(model2.Int64Enum, model.Int64Enum);
            Assert.Equal(model2.UInt16Enum, model.UInt16Enum);
            Assert.Equal(model2.UInt32Enum, model.UInt32Enum);
            Assert.Equal(model2.UInt64Enum, model.UInt64Enum);
        }
    }
}

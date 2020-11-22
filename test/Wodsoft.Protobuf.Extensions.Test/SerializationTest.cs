using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            Message.Serialize(stream, model);

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

        [Fact]
        public void Collection_Class_Test()
        {
            CollectionModel model = new CollectionModel();
            model.StringList = new List<string>();
            model.StringList.Add("1");
            model.StringList.Add("2");
            model.StringList.Add("3");
            model.StringList2 = new List<string>();
            model.StringList2.Add("4");
            model.StringList2.Add("5");
            model.StringList2.Add("6");
            model.IntList = new List<int>();
            model.IntList.Add(10);
            model.IntList.Add(20);
            model.IntList.Add(30);
            model.NullableIntList = new List<int?>();
            model.NullableIntList.Add(50);
            model.NullableIntList.Add(6);
            model.NullableIntList.Add(-9);
            model.ByteList = new List<byte>();
            model.ByteList.Add(23);
            model.ByteList.Add(45);
            model.NullableByteList = new List<byte?>();
            model.NullableByteList.Add(123);
            model.NullableByteList.Add(254);
            model.NullableByteList.Add(0);
            model.SByteCollection = new List<sbyte>();
            model.SByteCollection.Add(-45);
            model.SByteCollection.Add(21);
            model.SByteCollection2 = new Collection<sbyte>();
            model.SByteCollection2.Add(-87);
            model.SByteCollection2.Add(88);
            model.NullableShortCollection = new List<short?>();
            model.NullableShortCollection.Add(-8752);
            model.NullableShortCollection.Add(5782);
            model.NullableShortCollection2 = new Collection<short?>();
            model.NullableShortCollection2.Add(-4278);
            model.NullableShortCollection2.Add(7886);
            model.UShortArray = new ushort[] { 16588, 65521 };
            model.NullableUShortArray = new ushort?[] { 32556, 1565 };
            model.LongArray = new long[] { 16581231238, 6545232521 };
            model.NullableLongArray = new long?[] { 325512312341232346, 1512312312433443465 };

            MemoryStream stream = new MemoryStream();
            Message.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message<CollectionModel>.Deserialize(stream);

            Assert.Equal(model.StringList, model2.StringList);
            Assert.Equal(model.StringList2, model2.StringList2);
            Assert.Equal(model.IntList, model2.IntList);
            Assert.Equal(model.NullableIntList, model2.NullableIntList);
            Assert.Equal(model.ByteList, model2.ByteList);
            Assert.Equal(model.NullableByteList, model2.NullableByteList);
            Assert.Equal(model.SByteCollection, model2.SByteCollection);
            Assert.Equal(model.SByteCollection2, model2.SByteCollection2);
            Assert.Equal(model.NullableShortCollection, model2.NullableShortCollection);
            Assert.Equal(model.NullableShortCollection2, model2.NullableShortCollection2);
            Assert.Equal<ushort>(model.UShortArray, model2.UShortArray);
            Assert.Equal<ushort?>(model.NullableUShortArray, model2.NullableUShortArray);
            Assert.Equal<long>(model.LongArray, model2.LongArray);
            Assert.Equal<long?>(model.NullableLongArray, model2.NullableLongArray);
        }
    }
}

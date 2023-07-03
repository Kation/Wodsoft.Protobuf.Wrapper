using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using Wodsoft.Protobuf.Generators;
using Xunit;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class BuilderTest
    {
        [Fact]
        public void Create_Simply_Type_Test()
        {
            MessageBuilder.GetMessageType<SimplyModel>();
            MessageBuilder.GetMessageType<SimplyModel2>();
            MessageBuilder.GetMessageType<NullableModel>();
            MessageBuilder.GetMessageType<EnumModel>();
            MessageBuilder.GetMessageType<CollectionModel>();
            MessageBuilder.GetMessageType<DictionaryModel>();
            MessageBuilder.GetMessageType<ContentModel>();
            MessageBuilder.GetMessageType<MessageModel>();
            MessageBuilder.GetMessageType<ObjectCollectionModel>();
            MessageBuilder.GetMessageType<ObjectDictionaryModel>();
            MessageBuilder.GetMessageType<PointModel>();
            MessageBuilder.GetMessageType<ValueConstructorModel>();
            MessageBuilder.GetMessageType<EnumArrayModel>();

            var generator = new Lokad.ILPack.AssemblyGenerator();
            var bytes = generator.GenerateAssemblyBytes(MessageBuilder.GetAssembly());
            File.WriteAllBytes("dynamic.dll", bytes);

            ByteEnum e = ByteEnum.One;
            byte bytea = (byte)e;
            ByteEnum d = (ByteEnum)bytea;
            Console.WriteLine(d);            
        }
    }
}
